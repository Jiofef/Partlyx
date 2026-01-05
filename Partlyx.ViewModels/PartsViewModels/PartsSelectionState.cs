using CommunityToolkit.Mvvm.ComponentModel;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Partlyx.ViewModels.PartsViewModels
{
    public sealed partial class PartsSelectionState : ObservableObject, IDisposable
    {
        private readonly ICollection<object> _parts;
        private bool _hasAnything;
        private bool _hasResource;
        private bool _hasRecipe;
        private bool _hasOnlyResources;
        private bool _hasOnlyRecipes;
        private bool _hasOnlyComponents;
        private bool _hasManyComponents;
        private bool _hasManyRecipes;
        private bool _hasManyResources;
        private bool _hasComponent;
        private int _resourceCount;
        private int _recipeCount;
        private int _componentCount;
        public PartsSelectionState(INotifyCollectionChanged parts)
        {
            _parts = (ICollection<object>)parts;

            RecalculateCounts();
            parts.CollectionChanged += Parts_CollectionChanged;
        }

        //  Public properties
        public bool HasAnything { get => _hasAnything; set => SetProperty(ref _hasAnything, value); }
        public bool HasOnlyResources { get => _hasOnlyResources; set => SetProperty(ref _hasOnlyResources, value); }
        public bool HasOnlyRecipes { get => _hasOnlyRecipes; set => SetProperty(ref _hasOnlyRecipes, value); }
        public bool HasOnlyComponents { get => _hasOnlyComponents; set => SetProperty(ref _hasOnlyComponents, value); }
        public bool HasResource { get => _hasResource; set => SetProperty(ref _hasResource, value); }

        public bool HasRecipe { get => _hasRecipe; set => SetProperty(ref _hasRecipe, value); }

        public bool HasComponent { get => _hasComponent; set => SetProperty(ref _hasComponent, value); }

        public bool HasManyResources { get => _hasManyResources; set => SetProperty(ref _hasManyResources, value); }

        public bool HasManyRecipes { get => _hasManyRecipes; set => SetProperty(ref _hasManyRecipes, value); }

        public bool HasManyComponents { get => _hasManyComponents; set => SetProperty(ref _hasManyComponents, value); }

        public int ResourceCount { get => _resourceCount; set => SetProperty(ref _resourceCount, value); }
        public int RecipeCount { get => _recipeCount; set => SetProperty(ref _recipeCount, value); }
        public int ComponentCount { get => _componentCount; set => SetProperty(ref _componentCount, value); }

        public int PartsCount => _parts.Count;

        //     Collection logic
        private void Parts_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                        foreach (var item in e.NewItems.OfType<IVMPart>())
                            Increment(item);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                        foreach (var item in e.OldItems.OfType<IVMPart>())
                            Decrement(item);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                        foreach (var item in e.OldItems.OfType<IVMPart>())
                            Decrement(item);

                    if (e.NewItems != null)
                        foreach (var item in e.NewItems.OfType<IVMPart>())
                            Increment(item);
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Reset:
                    RecalculateCounts();
                    break;
            }

            UpdateFlags();
        }

        private void Increment(IVMPart part)
        {
            switch (part)
            {
                case ResourceViewModel:
                    ResourceCount++;
                    break;
                case RecipeViewModel:
                    RecipeCount++;
                    break;
                case RecipeComponentViewModel:
                    ComponentCount++;
                    break;
            }
        }

        private void Decrement(IVMPart part)
        {
            switch (part)
            {
                case ResourceViewModel:
                    ResourceCount = Math.Max(0, ResourceCount - 1);
                    break;
                case RecipeViewModel:
                    RecipeCount = Math.Max(0, RecipeCount - 1);
                    break;
                case RecipeComponentViewModel:
                    ComponentCount = Math.Max(0, ComponentCount - 1);
                    break;
            }
        }

        private void RecalculateCounts()
        {
            ResourceCount = _parts.OfType<ResourceViewModel>().Count();
            RecipeCount = _parts.OfType<RecipeViewModel>().Count();
            ComponentCount = _parts.OfType<RecipeComponentViewModel>().Count();

            UpdateFlags();
        }

        private void UpdateFlags()
        {
            HasResource = ResourceCount > 0;
            HasRecipe = RecipeCount > 0;
            HasComponent = ComponentCount > 0;

            HasManyResources = ResourceCount > 1;
            HasManyRecipes = RecipeCount > 1;
            HasManyComponents = ComponentCount > 1;

            HasAnything = HasResource || HasRecipe || HasComponent;

            HasOnlyResources = HasResource && !HasRecipe && !HasComponent;
            HasOnlyRecipes = !HasResource && HasRecipe && !HasComponent;
            HasOnlyComponents = !HasResource && !HasRecipe && HasComponent;

            FlagsUpdated?.Invoke();
        }
        public event Action FlagsUpdated = delegate { };
        public void Dispose()
        {
            ((INotifyCollectionChanged)_parts).CollectionChanged -= Parts_CollectionChanged;
        }
    }
}