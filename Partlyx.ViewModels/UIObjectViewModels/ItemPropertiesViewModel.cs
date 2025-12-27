using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Partlyx.Core.Contracts;
using Partlyx.Core.Partlyx;
using Partlyx.Infrastructure.Events;
using Partlyx.UI.Avalonia.Helpers;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.ItemProperties;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class ItemPropertiesViewModel : ObservableObject, IDisposable
    {
        private readonly IVMPartsStore _partsStore;
        private readonly PartsServiceViewModel _services;
        private readonly IDialogService _dialogService;
        private readonly ILocalizationService _loc;
        private readonly IServiceProvider _serviceProvider; // Only for creating VM windows

        private readonly IDisposable _focusedPartChangedSubscription;

        public IGlobalFocusedPart FocusedPart { get; }
        public ObservableCollection<ItemPropertyViewModel> Properties { get; } = new();

        private string _focusedPartTextAnnotation = "";
        public string FocusedPartTextAnnotation { get => _focusedPartTextAnnotation; set => SetProperty(ref _focusedPartTextAnnotation, value); }


        public ItemPropertiesViewModel(PartsServiceViewModel services, IDialogService ds, IGlobalFocusedPart focusedPart, 
            IEventBus bus, ILocalizationService loc, IServiceProvider serviceProvider, IVMPartsStore store)
        {
            _services = services;
            _dialogService = ds;
            _loc = loc;
            _partsStore = store;

            _focusedPartChangedSubscription = bus.Subscribe<GlobalFocusedPartChangedEvent>(ev => OnFocusedPartChanged());

            FocusedPart = focusedPart;
            _serviceProvider = serviceProvider;
        }

        public void OnFocusedPartChanged()
        {
            Properties.ClearAndDispose();

            FocusedPartTextAnnotation = "";

            var focusedPart = FocusedPart.FocusedPart;
            if (focusedPart == null) return;

            switch (focusedPart.PartType)
            {
                case PartTypeEnumVM.Resource:
                    var resource = (ResourceViewModel)focusedPart;
                    FocusedPartTextAnnotation = _loc.Get("resource_properties_annotation", resource.Name);
                    LoadResourceProperties(resource);
                    break;
                case PartTypeEnumVM.Recipe:
                    var recipe = (RecipeViewModel)focusedPart;
                    FocusedPartTextAnnotation = _loc.Get("recipe_properties_annotation", recipe.Name);
                    LoadRecipeProperties(recipe);
                    break;
                case PartTypeEnumVM.Component:
                    var component = (RecipeComponentViewModel)focusedPart;
                    FocusedPartTextAnnotation = _loc.Get("recipe_component_properties_annotation", component.LinkedResource?.Value?.Name ?? "", component.Quantity);
                    LoadComponentProperties(component);
                    break;
            }
        }

        [RelayCommand]
        public async Task<ISelectedParts?> SelectPartsFromWindow()
        {
            var result = await _dialogService.ShowDialogAsync<ComponentCreateViewModel>();
            if (result is not ISelectedParts selected)
                return null;

            return selected;
        }

        public void LoadResourceProperties(ResourceViewModel resource)
        {
            // Creating "Name" property
            var nameProperty = new TextBoxItemPropertyViewModel() { Name = _loc["Name"], Item = resource };
            nameProperty.Text = resource.Name;
            nameProperty.SaveChangesTask = new(
                async (arg) => 
                {
                    if (arg is not string name) return;
                    await _services.ResourceService.RenameResource(resource, name); 
                });
            nameProperty.CancelChangesTask = new(
                (args) => { nameProperty.Text = resource.Name; return Task.CompletedTask; });
            var nameUpdateSubscription = resource.WhenAnyValue(r => r.Name).Subscribe((args) => nameProperty.Text = resource.Name);
            nameProperty.Subscriptions.Add(nameUpdateSubscription);
            Properties.Add(nameProperty);

            // Creating "Icon" property
            var iconProperty = new IconItemPropertyViewModel(resource.Icon) { Name = _loc["Icon"], Item = resource };
            iconProperty.SaveChangesTask = new(
                async (arg) =>
                {
                    if (arg is not IconViewModel icon) return;
                    await _services.ResourceService.SetIcon(resource, icon);
                });
            iconProperty.SelectButtonPressedTask = new(
                async (arg) =>
                {
                    var dialogVM = _serviceProvider.GetRequiredService<IconsMenuViewModel>();
                    dialogVM.EnableIconSelection = true;
                    dialogVM.IconSetTarget = resource;

                    if (resource.Icon.Content != null)
                        dialogVM.SelectItemWith(resource.Icon.Content);


                    var result = await _dialogService.ShowDialogAsync(dialogVM);

                    if (result is not IconViewModel icon) return;
                    await _services.ResourceService.SetIcon(resource, icon);
                    icon?.Dispose();
                });
            var iconUpdateSubscription = resource.WhenAnyValue(r => r.Icon).Subscribe((args) => iconProperty.Icon = resource.Icon);
            iconProperty.Subscriptions.Add(iconUpdateSubscription);
            Properties.Add(iconProperty);


            // Creating "Default recipe" property
            var defaultRecipeProperty = new RecipeItemPropertyViewModel() { Name = _loc["Default_recipe"], Item = resource, Part = resource.LinkedDefaultRecipe?.Value };
            defaultRecipeProperty.SelectButtonPressedTask = new(
                async (arg) =>
                {
                    var allTheRecipesList = _partsStore.Recipes.Values.ToList();
                    var dialogVM = new RecipesSelectionViewModel(_dialogService, new IsolatedSelectedParts())
                    { 
                        EnableMultiSelect = false, 
                        Items = new ObservableCollection<RecipeViewModel>(allTheRecipesList), 
                        IsSelectionNecessaryToConfirm = true 
                    };

                    var result = await _dialogService.ShowDialogAsync(dialogVM);

                    if (result is not ISelectedParts selected) return;
                    var recipe = selected.GetSingleRecipeOrNull()!;

                    var args = new PartSetValueInfo<ResourceViewModel, RecipeViewModel>(resource, recipe);
                    await _services.ResourceService.SetDefaultRecipe(args);
                });
            defaultRecipeProperty.SaveChangesTask = new(
                async (arg) =>
                {
                    if (arg is not RecipeViewModel recipe) return;
                    await _services.ResourceService.SetDefaultRecipe(resource, recipe);
                });
            var defaultRecipeUpdateSubscription = resource.WhenAnyValue(r => r.LinkedDefaultRecipe!.Value).Subscribe((args) => defaultRecipeProperty.Part = resource.LinkedDefaultRecipe?.Value);
            defaultRecipeProperty.Subscriptions.Add(defaultRecipeUpdateSubscription);
            Properties.Add(defaultRecipeProperty);
        }

        public void LoadRecipeProperties(RecipeViewModel recipe)
        {
            // Creating "Name" property
            var nameProperty = new TextBoxItemPropertyViewModel() { Name = _loc["Name"], Item = recipe };
            nameProperty.Text = recipe.Name;
            nameProperty.SaveChangesTask = new(
                async (arg) =>
                {
                    if (arg is not string name) return;
                    var args = new PartSetValueInfo<RecipeViewModel, string>(recipe, name);
                    await _services.RecipeService.RenameRecipe(args);
                });
            nameProperty.CancelChangesTask = new(
                (args) => { nameProperty.Text = recipe.Name; return Task.CompletedTask; });
            var nameUpdateSubscription = recipe.WhenAnyValue(r => r.Name).Subscribe((args) => nameProperty.Text = recipe.Name);
            nameProperty.Subscriptions.Add(nameUpdateSubscription);
            Properties.Add(nameProperty);

            // Creating "Icon" property
            var iconProperty = new IconItemPropertyViewModel(recipe.Icon) { Name = _loc["Icon"], Item = recipe };
            iconProperty.SaveChangesTask = new(
                async (arg) =>
                {
                    if (arg is not IconViewModel icon) return;
                    await _services.RecipeService.SetIcon(recipe, icon);
                });
            iconProperty.SelectButtonPressedTask = new(
                async (arg) =>
                {
                    var dialogVM = _serviceProvider.GetRequiredService<IconsMenuViewModel>();
                    dialogVM.EnableIconSelection = true;
                    dialogVM.IconSetTarget = recipe;

                    if (recipe.Icon.Content != null)
                        dialogVM.SelectItemWith(recipe.Icon.Content);

                    var result = await _dialogService.ShowDialogAsync(dialogVM);

                    if (result is not IconViewModel icon) return;
                    await _services.RecipeService.SetIcon(recipe, icon);
                    icon?.Dispose();
                });
            var iconUpdateSubscription = recipe.WhenAnyValue(r => r.Icon).Subscribe((args) => iconProperty.Icon = recipe.Icon);
            iconProperty.Subscriptions.Add(iconUpdateSubscription);
            Properties.Add(iconProperty);

            // Creating "Is reversible" property
            var isReversibleProperty = new CheckBoxItemPropertyViewModel() { Name = _loc["Is_reversible"], Item = recipe };
            isReversibleProperty.IsChecked = recipe.IsReversible;
            isReversibleProperty.SaveChangesTask = new(
                async arg =>
                {
                    if (arg is not bool isReversible) return;
                    await _services.RecipeService.SetIsReversible(recipe, isReversible);
                });
            var isReversibleUpdateSubscription = recipe.WhenAnyValue(r => r.IsReversible).Subscribe(args => isReversibleProperty.IsChecked = recipe.IsReversible);
            isReversibleProperty.Subscriptions.Add(isReversibleUpdateSubscription);
            Properties.Add(isReversibleProperty);
        }

        public void LoadComponentProperties(RecipeComponentViewModel component)
        {
            // Creating "Selected recipe" property
            var selectedRecipeProperty = new RecipeItemPropertyViewModel() { Name = _loc["Selected_recipe"], Item = component, Part = component.LinkedSelectedRecipe?.Value };
            selectedRecipeProperty.SelectButtonPressedTask = new(
                async (arg) =>
                {
                    var allTheRecipesList = _partsStore.Recipes.Values.ToList();
                    var dialogVM = new RecipesSelectionViewModel(_dialogService, new IsolatedSelectedParts())
                    {
                        EnableMultiSelect = false,
                        Items = new ObservableCollection<RecipeViewModel>(allTheRecipesList),
                        IsSelectionNecessaryToConfirm = true
                    };

                    var result = await _dialogService.ShowDialogAsync(dialogVM);

                    if (result is not ISelectedParts selected) return;
                    var recipe = selected.GetSingleRecipeOrNull()!;

                    await _services.ComponentService.SetSelectedRecipe(component, recipe);
                });
            selectedRecipeProperty.SaveChangesTask = new(
                async (arg) =>
                {
                    if (arg is not RecipeViewModel recipe) return;
                    await _services.ComponentService.SetSelectedRecipe(component, recipe);
                });
            var selectedRecipeUpdateSubscription = component.WhenAnyValue(r => r.LinkedSelectedRecipe!.Value).Subscribe((args) => selectedRecipeProperty.Part = component.LinkedSelectedRecipe?.Value);
            selectedRecipeProperty.Subscriptions.Add(selectedRecipeUpdateSubscription);
            Properties.Add(selectedRecipeProperty);

            // Creating "Quantity" property
            var quantityProperty = new SpinBoxItemPropertyViewModel() { Name = _loc["Needed_amount"], Item = component };
            quantityProperty.Value = (decimal)component.Quantity;
            quantityProperty.SaveChangesTask = new(
                async arg =>
                {
                    if (arg is not decimal quantity) return;
                    await _services.ComponentService.SetQuantityAsync(component, (double)quantity);
                });
            var quantityUpdateSubscription = component.WhenAnyValue(r => r.Quantity).Subscribe(args => quantityProperty.Value = (decimal)component.Quantity);
            quantityProperty.Subscriptions.Add(quantityUpdateSubscription);
            Properties.Add(quantityProperty);
        }

        public void Dispose()
        {
            _focusedPartChangedSubscription.Dispose();

            Properties.ClearAndDispose();
        }
    }
}
