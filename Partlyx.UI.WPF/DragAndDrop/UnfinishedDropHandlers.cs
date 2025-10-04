// Do not pay attention to this file. Some of this code will be used later in creating appropriate classes.

//public class ResourcesListDropHandler : DependencyObject, IDropTarget
//{
//    private readonly DefaultDropHandler _defaultHandler = new DefaultDropHandler();

//    public static readonly DependencyProperty ResourcesDropCommandProperty =
//DependencyProperty.Register(
//nameof(ResourcesDropCommand),
//typeof(ICommand),
//typeof(ResourcesListDropHandler),
//new PropertyMetadata(null));
//    public ICommand? ResourcesDropCommand
//    {
//        get => (ICommand?)GetValue(ResourcesDropCommandProperty);
//        set => SetValue(ResourcesDropCommandProperty, value);
//    }

//    public static readonly DependencyProperty RecipesDropCommandProperty =
//        DependencyProperty.Register(
//        nameof(RecipesDropCommand),
//        typeof(ICommand),
//        typeof(ResourcesListDropHandler),
//        new PropertyMetadata(null));
//    public ICommand? RecipesDropCommand
//    {
//        get => (ICommand?)GetValue(RecipesDropCommandProperty);
//        set => SetValue(RecipesDropCommandProperty, value);
//    }

//    public static readonly DependencyProperty ComponentsDropCommandProperty =
//        DependencyProperty.Register(
//        nameof(ComponentsDropCommand),
//        typeof(ICommand),
//        typeof(ResourcesListDropHandler),
//        new PropertyMetadata(null));
//    public ICommand? ComponentsDropCommand
//    {
//        get => (ICommand?)GetValue(ComponentsDropCommandProperty);
//        set => SetValue(ComponentsDropCommandProperty, value);
//    }

//    public void DragOver(IDropInfo dropInfo)
//    {
//        _defaultHandler.DragOver(dropInfo);

//        if (DropInfoHelpers.TryGetItemsOfType<RecipeItemViewModel>(dropInfo, out var recipes))
//        {
//            dropInfo.Effects = DragDropEffects.Copy;
//            dropInfo.DropTargetHintAdorner = DropTargetAdorners.Highlight;
//        }
//    }

//    public void Drop(IDropInfo dropInfo)
//    {
//        if (DropInfoHelpers.TryGetItemsOfType<RecipeItemViewModel>(dropInfo, out var recipes))
//        {
//            // Moving recipes to other resource
//        }
//        else
//            _defaultHandler.Drop(dropInfo);
//    }
//}

//public class RecipesListDropHandler : DependencyObject, IDropTarget
//{
//    private readonly DefaultDropHandler _defaultHandler = new DefaultDropHandler();

//    public static readonly DependencyProperty ResourcesDropCommandProperty =
//DependencyProperty.Register(
//nameof(ResourcesDropCommand),
//typeof(ICommand),
//typeof(RecipesListDropHandler),
//new PropertyMetadata(null));
//    public ICommand? ResourcesDropCommand
//    {
//        get => (ICommand?)GetValue(ResourcesDropCommandProperty);
//        set => SetValue(ResourcesDropCommandProperty, value);
//    }

//    public static readonly DependencyProperty RecipesDropCommandProperty =
//        DependencyProperty.Register(
//        nameof(RecipesDropCommand),
//        typeof(ICommand),
//        typeof(RecipesListDropHandler),
//        new PropertyMetadata(null));
//    public ICommand? RecipesDropCommand
//    {
//        get => (ICommand?)GetValue(RecipesDropCommandProperty);
//        set => SetValue(RecipesDropCommandProperty, value);
//    }

//    public static readonly DependencyProperty ComponentsDropCommandProperty =
//        DependencyProperty.Register(
//        nameof(ComponentsDropCommand),
//        typeof(ICommand),
//        typeof(RecipesListDropHandler),
//        new PropertyMetadata(null));
//    public ICommand? ComponentsDropCommand
//    {
//        get => (ICommand?)GetValue(ComponentsDropCommandProperty);
//        set => SetValue(ComponentsDropCommandProperty, value);
//    }

//    public void DragOver(IDropInfo dropInfo)
//    {
//        _defaultHandler.DragOver(dropInfo);

//        if (DropInfoHelpers.TryGetItemsOfType<ResourceItemViewModel>(dropInfo, out var resources) )
//        {
//            dropInfo.Effects = DragDropEffects.Copy;
//            dropInfo.DropTargetHintAdorner = DropTargetAdorners.Highlight;
//        }
//        else if (DropInfoHelpers.TryGetItemsOfType<RecipeComponentItemViewModel>(dropInfo, out var components))
//        {
//            dropInfo.Effects = DragDropEffects.Move;
//            dropInfo.DropTargetHintAdorner = DropTargetAdorners.Highlight;
//        }
//    }

//    public void Drop(IDropInfo dropInfo)
//    {
//        if (DropInfoHelpers.TryGetItemsOfType<ResourceItemViewModel>(dropInfo, out var resources))
//        {
//            // Create components from resources in hovered recipe
//            if (dropInfo.TargetItem is not RecipeItemViewModel recipe)
//                return;


//        }
//        else if (DropInfoHelpers.TryGetItemsOfType<RecipeComponentItemViewModel>(dropInfo, out var components))
//        {
//            // Move components to other recipe
//            if (dropInfo.TargetItem is not RecipeItemViewModel recipe)
//                return;

//            ComponentsDropCommand?.Execute(components, recipe);
//        }
//        else
//            _defaultHandler.Drop(dropInfo);
//    }
//}