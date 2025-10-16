using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData.Binding;
using Partlyx.Core;
using Partlyx.Infrastructure.Events;
using Partlyx.ViewModels.ItemProperties;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;
using Partlyx.ViewModels.UIServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.UIObjectViewModels
{
    public partial class ItemPropertiesViewModel : ObservableObject, IDisposable
    {
        private readonly PartsServiceViewModel _services;
        private readonly IDialogService _dialogService;

        private readonly IDisposable _focusedPartChangedSubscription;

        public IGlobalFocusedPart FocusedPart { get; }
        public ObservableCollection<ItemPropertyViewModel> Properties { get; } = new();

        private string _focusedPartTextAnnotation = "";
        public string FocusedPartTextAnnotation { get => _focusedPartTextAnnotation; set => SetProperty(ref _focusedPartTextAnnotation, value); }



        public ItemPropertiesViewModel(PartsServiceViewModel services, IDialogService ds, IGlobalFocusedPart focusedPart, IEventBus bus)
        {
            _services = services;
            _dialogService = ds;

            _focusedPartChangedSubscription = bus.Subscribe<FocusedPartChangedEvent>(ev => OnFocusedPartChanged());

            FocusedPart = focusedPart;
        }

        public void OnFocusedPartChanged()
        {
            Properties.Clear();

            FocusedPartTextAnnotation = "";

            var focusedPart = FocusedPart.FocusedPart;
            if (focusedPart == null) return;

            switch (focusedPart.PartType)
            {
                case PartTypeEnumVM.Resource:
                    var resource = (ResourceViewModel)focusedPart;
                    FocusedPartTextAnnotation = $"\"{resource.Name}\" resource";
                    LoadResourceProperties(resource);
                    break;
                case PartTypeEnumVM.Recipe:
                    var recipe = (RecipeViewModel)focusedPart;
                    FocusedPartTextAnnotation = $"\"{recipe.Name}\" recipe";
                    LoadRecipeProperties(recipe);
                    break;
                case PartTypeEnumVM.Component:
                    var component = (RecipeComponentViewModel)focusedPart;
                    FocusedPartTextAnnotation = $"\"{component.LinkedResource?.Value?.Name}\" x{component.Quantity} component";
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
            var nameProperty = new TextBoxItemPropertyViewModel() { Name = "Name", Item = resource };
            nameProperty.Text = resource.Name;
            nameProperty.SaveChangesTask = new(
                async (arg) => 
                {
                    if (arg is not string name) return;
                    var args = new PartSetValueInfo<ResourceViewModel, string>(resource, name);
                    await _services.ResourceService.RenameResource(args); 
                });
            nameProperty.CancelChangesTask = new(
                (args) => { nameProperty.Text = resource.Name; return Task.CompletedTask; });
            var nameUpdateSubscription = resource.WhenValueChanged(r => r.Name).Subscribe((args) => nameProperty.Text = resource.Name);
            nameProperty.Subscriptions.Add(nameUpdateSubscription);
            Properties.Add(nameProperty);

            // Creating "Default recipe" property
            var defaultRecipeProperty = new RecipeItemPropertyViewModel() { Name = "Default recipe", Item = resource, Part = resource.LinkedDefaultRecipe?.Value };
            defaultRecipeProperty.SelectButtonPressedTask = new(
                async (arg) =>
                {
                    var dialogVM = new PartsSelectionWindowViewModel<RecipeViewModel>(new IsolatedSelectedParts())
                    { 
                        EnableMultiSelect = false, 
                        Items = resource.Recipes, 
                        IsSelectionNecessaryToConfirm = true 
                    };

                    var result = await _dialogService.ShowDialogAsync(dialogVM);

                    if (result is not ISelectedParts selected) return;
                    var recipe = selected.GetSingleRecipeOrNull()!;

                    var args = new PartSetValueInfo<ResourceViewModel, RecipeViewModel>(resource, recipe);
                    await _services.ResourceService.SetDefaultRecipe(args);
                });
            var defaultRecipeUpdateSubscription = resource.WhenValueChanged(r => r.LinkedDefaultRecipe).Subscribe((args) => defaultRecipeProperty.Part = resource.LinkedDefaultRecipe?.Value);
            defaultRecipeProperty.Subscriptions.Add(defaultRecipeUpdateSubscription);
            Properties.Add(defaultRecipeProperty);
        }

        public void LoadRecipeProperties(RecipeViewModel recipe)
        {
            // Creating "Name" property
            var nameProperty = new TextBoxItemPropertyViewModel() { Name = "Name", Item = recipe };
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
            var nameUpdateSubscription = recipe.WhenValueChanged(r => r.Name).Subscribe((args) => nameProperty.Text = recipe.Name);
            nameProperty.Subscriptions.Add(nameUpdateSubscription);
            Properties.Add(nameProperty);
        }

        public void LoadComponentProperties(RecipeComponentViewModel component)
        {
            // Creating "Selected recipe" property
            var selectedRecipeProperty = new RecipeItemPropertyViewModel() { Name = "Selected recipe", Item = component, Part = component.LinkedSelectedRecipe?.Value };
            selectedRecipeProperty.SelectButtonPressedTask = new(
                async (arg) =>
                {
                    var dialogVM = new PartsSelectionWindowViewModel<RecipeViewModel>(new IsolatedSelectedParts())
                    {
                        EnableMultiSelect = false,
                        Items = component.LinkedResource?.Value?.Recipes,
                        IsSelectionNecessaryToConfirm = true
                    };

                    var result = await _dialogService.ShowDialogAsync(dialogVM);

                    if (result is not ISelectedParts selected) return;
                    var recipe = selected.GetSingleRecipeOrNull()!;

                    var args = new PartSetValueInfo<RecipeComponentViewModel, RecipeViewModel>(component, recipe);
                    await _services.ComponentService.SetSelectedRecipe(args);
                });
            var selectedRecipeUpdateSubscription = component.WhenValueChanged(r => r.LinkedSelectedRecipe).Subscribe((args) => selectedRecipeProperty.Part = component.LinkedSelectedRecipe?.Value);
            selectedRecipeProperty.Subscriptions.Add(selectedRecipeUpdateSubscription);
            Properties.Add(selectedRecipeProperty);
        }

        public void Dispose()
        {
            _focusedPartChangedSubscription.Dispose();
        }
    }
}
