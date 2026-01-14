using Partlyx.ViewModels.GraphicsViewModels.HierarchyViewModels;
using Partlyx.ViewModels.PartsViewModels;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;

namespace Partlyx.ViewModels.Graph.PartsGraph
{
    public class ComponentPathGraphInstanceManager : PartlyxObservable, IPartsGraphInstanceManager
    {
        public PartsGraphBuilderViewModel ParentBuilder { get; }
        public RecipeComponentPath Path { get; }
        public RecipeComponentPathItem PathItem { get; }

        private readonly Dictionary<int, ComponentGraphNodeViewModel> _pathNodeMap = new();
        private readonly Dictionary<Guid, List<ComponentGraphNodeViewModel>> _sideNodeMap = new();

        public ComponentPathGraphInstanceManager(PartsGraphBuilderViewModel builder, RecipeComponentPathItem pathItem)
        {
            ParentBuilder = builder;
            PathItem = pathItem;
            Path = pathItem.Path;

            Disposables.Add(PathItem.WhenAnyValue(pi => pi.SavedEnterValue).Subscribe(_ => UpdateCosts()));
            Disposables.Add(PathItem.WhenAnyValue(pi => pi.SavedCalculateFromOutput).Subscribe(_ => UpdateCosts()));
        }

        public void BuildGraph()
        {
            var b = ParentBuilder;
            var steps = Path.Steps.ToList();
            if (steps.Count < 2) return;

            _pathNodeMap.Clear();
            _sideNodeMap.Clear();

            var pathComponentUids = new HashSet<Guid>(steps.Select(s => s.Uid));

            for (int i = 0; i + 1 < steps.Count; i += 2)
            {
                var stepIn = steps[i];
                var stepOut = steps[i + 1];
                var recipe = stepIn.ParentRecipe ?? stepOut.ParentRecipe;
                if (recipe == null) continue;

                var recipeNode = recipe.ToNode();
                b.AddNode(recipeNode);
                if (b.RootNode == null) b.RootNode = recipeNode;

                var nodeIn = GetOrCreatePathNode(i, stepIn, b);
                var nodeOut = GetOrCreatePathNode(i + 1, stepOut, b);

                // ENFORCE TOP-DOWN FLOW: nodeIn is always Top, nodeOut is always Bottom
                nodeIn.AddChild(recipeNode);
                recipeNode.AddChild(nodeOut);

                bool isReversed = stepIn.IsOutput;

                var allComponents = (recipe.Inputs ?? Enumerable.Empty<RecipeComponentViewModel>())
                    .Concat(recipe.Outputs ?? Enumerable.Empty<RecipeComponentViewModel>());

                foreach (var comp in allComponents)
                {
                    if (pathComponentUids.Contains(comp.Uid)) continue;

                    var sideNode = comp.ToNode();
                    b.AddNode(sideNode);

                    // Maintain vertical consistency for side-components:
                    // If flow is standard (In -> Out): Inputs on Top, Outputs on Bottom.
                    // If flow is reversed (Out -> In): Outputs on Top, Inputs on Bottom.
                    if (comp.IsOutput == isReversed) sideNode.AddChild(recipeNode);
                    else recipeNode.AddChild(sideNode);

                    if (!_sideNodeMap.ContainsKey(recipe.Uid))
                        _sideNodeMap[recipe.Uid] = new List<ComponentGraphNodeViewModel>();
                    _sideNodeMap[recipe.Uid].Add(sideNode);
                }
            }

            // Identify leaf nodes (nodes with only children or only parents)
            foreach (var node in b.Nodes.OfType<ComponentGraphNodeViewModel>())
            {
                if (node.Parents.Any() ^ node.Children.Any())
                {
                    if (!b.ComponentLeafs.Contains(node)) b.ComponentLeafs.Add(node);
                }
            }

            UpdateCosts();
            b.BuildEdges();
            b.BuildLayout();
        }

        private ComponentGraphNodeViewModel GetOrCreatePathNode(int index, RecipeComponentViewModel comp, PartsGraphBuilderViewModel b)
        {
            int key = (index > 0) ? ((index % 2 != 0) ? index : index - 1) : 0;
            if (_pathNodeMap.TryGetValue(key, out var existingNode)) return existingNode;

            var newNode = comp.ToNode();
            b.AddNode(newNode);
            _pathNodeMap[key] = newNode;
            return newNode;
        }

        public void UpdateCosts()
        {
            if (Path.Steps.Count < 2) return;

            // Determine the starting flow based on SavedCalculateFromOutput property
            double initialInputFlow = PathItem.SavedCalculateFromOutput
                ? Path.CalculateRequiredInput(PathItem.SavedEnterValue) // Find input that results in given output
                : PathItem.SavedEnterValue; // Normal calculation from input

            double currentFlow = initialInputFlow;
            var currentNode = Path.Steps.First;

            // Helper to update node properties based on its role in the graph
            void UpdateNodeState(ComponentGraphNodeViewModel node, double cost)
            {
                node.AbsCost = cost;
                // Rule: if it has children, it's an Input (IsOutput = false), otherwise it's an Output
                node.IsOutput = !node.Children.Any();
            }

            // Set cost for the root node of the path
            if (_pathNodeMap.TryGetValue(0, out var rootNode))
                UpdateNodeState(rootNode, currentFlow);

            int stepIndex = 0;
            while (currentNode != null && currentNode.Next != null)
            {
                var pathIn = currentNode.Value;
                var pathOut = currentNode.Next.Value;
                var recipe = pathIn.ParentRecipe;

                if (recipe != null)
                {
                    bool isReverseStep = recipe.Outputs.Any(o => o.Uid == pathIn.Uid);
                    double crafts = recipe.GetCraftsCount(pathIn.Uid, currentFlow, isReverseStep);

                    // Update Path Input node
                    if (_pathNodeMap.TryGetValue(stepIndex == 0 ? 0 : stepIndex - 1, out var nodeIn))
                        UpdateNodeState(nodeIn, pathIn.Quantity * crafts);

                    // Update Path Output node
                    if (_pathNodeMap.TryGetValue(stepIndex + 1, out var nodeOut))
                        UpdateNodeState(nodeOut, pathOut.Quantity * crafts);

                    // Update Side nodes (components not in the main path spine)
                    if (_sideNodeMap.TryGetValue(recipe.Uid, out var sideNodes))
                    {
                        foreach (var sNode in sideNodes)
                        {
                            var vm = recipe.Inputs.Concat(recipe.Outputs).FirstOrDefault(c => c.Uid == sNode.Part?.Uid);
                            if (vm != null)
                                UpdateNodeState(sNode, vm.Quantity * crafts);
                        }
                    }

                    // Carry flow to the next step
                    currentFlow = pathOut.Quantity * crafts;
                }
                currentNode = currentNode.Next.Next;
                stepIndex += 2;
            }
        }
    }
}