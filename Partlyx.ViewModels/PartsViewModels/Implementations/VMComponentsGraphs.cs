using Partlyx.Infrastructure.Events;
using Partlyx.Services.ServiceImplementations;
using Partlyx.ViewModels.Graph;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using Partlyx.ViewModels.UIServices.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    public record ComponentGraphsUpdatedEvent();

    public class VMComponentsGraphs : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IVMPartsStore _store;
        private readonly List<IDisposable> _subscriptions = new();

        public class RecipeComponentGraph
        {
            public HashSet<RecipeComponentViewModel> Components { get; } = new();
            public HashSet<Guid> ResourceUids { get; } = new();
            public Dictionary<RecipeComponentViewModel, HashSet<RecipeComponentViewModel>> Adjacency { get; } = new();
        }

        private readonly List<RecipeComponentGraph> _allGraphs = new();
        private readonly Dictionary<Guid, RecipeComponentGraph> _resourceToGraph = new();
        private readonly Dictionary<Guid, RecipeComponentGraph> _componentToGraph = new();

        private readonly ThrottledInvoker _updateInvoker = new(TimeSpan.FromMilliseconds(100));

        public VMComponentsGraphs(IEventBus bus, IVMPartsStore store)
        {
            _bus = bus;
            _store = store;

            _subscriptions.Add(_bus.Subscribe<RecipeUpdatedViewModelEvent>(_ => _updateInvoker.InvokeAsync(Rebuild)));
            _subscriptions.Add(_bus.Subscribe<RecipeComponentUpdatedViewModelEvent>(_ => _updateInvoker.InvokeAsync(Rebuild)));
            _subscriptions.Add(_bus.Subscribe<PartsVMInitializationFinishedEvent>(_ => _updateInvoker.InvokeAsync(Rebuild)));

            Rebuild();
        }

        private void Rebuild()
        {
            _allGraphs.Clear();
            _resourceToGraph.Clear();
            _componentToGraph.Clear();

            var allComponents = _store.Components.Values.ToList();

            // 1. CLUSTERING: Group everything that touches the same resource or recipe
            foreach (var comp in allComponents)
            {
                if (_componentToGraph.ContainsKey(comp.Uid)) continue;

                var currentGraph = new RecipeComponentGraph();
                _allGraphs.Add(currentGraph);

                FloodFill(comp, currentGraph);
            }

            // 2. ADJACENCY: Define directional flow within each merged graph
            foreach (var graph in _allGraphs)
            {
                BuildAdjacency(graph);
            }

            _bus.Publish(new ComponentGraphsUpdatedEvent());
        }

        private void FloodFill(RecipeComponentViewModel start, RecipeComponentGraph graph)
        {
            var stack = new Stack<RecipeComponentViewModel>();
            stack.Push(start);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!graph.Components.Add(current)) continue;

                _componentToGraph[current.Uid] = graph;

                // Connect by Resource
                if (current.Resource != null)
                {
                    graph.ResourceUids.Add(current.Resource.Uid);
                    _resourceToGraph[current.Resource.Uid] = graph;

                    if (_store.ComponentsWithResource.TryGetValue(current.Resource.Uid, out var siblings))
                    {
                        foreach (var sibling in siblings) stack.Push(sibling);
                    }
                }

                // Connect by Recipe
                var recipe = current.ParentRecipe;
                if (recipe != null)
                {
                    foreach (var rc in recipe.Inputs.Concat(recipe.Outputs)) stack.Push(rc);
                }
            }
        }

        private void BuildAdjacency(RecipeComponentGraph graph)
        {
            foreach (var comp in graph.Components)
            {
                if (!graph.Adjacency.TryGetValue(comp, out var neighbors))
                {
                    neighbors = new HashSet<RecipeComponentViewModel>();
                    graph.Adjacency[comp] = neighbors;
                }

                var recipe = comp.ParentRecipe;
                if (recipe == null) continue;

                bool isInput = recipe.Inputs.Any(i => i.Uid == comp.Uid);
                bool isOutput = recipe.Outputs.Any(o => o.Uid == comp.Uid);

                // RULE A: Internal Transformation (In -> Out)
                if (isInput)
                {
                    foreach (var output in recipe.Outputs) neighbors.Add(output);
                }

                // RULE B: Reversible Transformation (Out -> In)
                if (isOutput && recipe.IsReversible)
                {
                    foreach (var input in recipe.Inputs) neighbors.Add(input);
                }

                // RULE C: Resource Bridge (Output A -> Input B)
                if (isOutput && comp.Resource != null)
                {
                    if (_store.ComponentsWithResource.TryGetValue(comp.Resource.Uid, out var siblings))
                    {
                        foreach (var sibling in siblings)
                        {
                            if (sibling.Uid == comp.Uid) continue;
                            if (sibling.ParentRecipe?.Inputs.Any(i => i.Uid == sibling.Uid) == true)
                                neighbors.Add(sibling);
                        }
                    }
                }
            }
        }

        public List<RecipeComponentPath> FindPathsBetweenResources(ResourceViewModel startRes, ResourceViewModel endRes)
        {
            if (!_resourceToGraph.TryGetValue(startRes.Uid, out var graph) ||
                !_resourceToGraph.TryGetValue(endRes.Uid, out var endGraph) ||
                graph != endGraph)
                return new List<RecipeComponentPath>();

            var results = new List<RecipeComponentPath>();
            var startNodes = _store.ComponentsWithResource[startRes.Uid]
                .Where(c => c.ParentRecipe?.Inputs.Any(i => i.Uid == c.Uid) == true);

            var targetUids = _store.ComponentsWithResource[endRes.Uid]
                .Where(c => c.ParentRecipe?.Outputs.Any(o => o.Uid == c.Uid) == true)
                .Select(c => c.Uid).ToHashSet();

            foreach (var startNode in startNodes)
            {
                var queue = new Queue<(RecipeComponentViewModel Node, List<RecipeComponentViewModel> Path)>();
                queue.Enqueue((startNode, new List<RecipeComponentViewModel> { startNode }));

                while (queue.Count > 0)
                {
                    var (current, path) = queue.Dequeue();

                    if (targetUids.Contains(current.Uid) && path.Count % 2 == 0)
                    {
                        results.Add(RecipeComponentPath.FromList(path));
                        continue;
                    }

                    if (path.Count > 20) continue;

                    if (graph.Adjacency.TryGetValue(current, out var neighbors))
                    {
                        foreach (var next in neighbors)
                        {
                            if (path.Any(p => p.Uid == next.Uid)) continue;
                            queue.Enqueue((next, new List<RecipeComponentViewModel>(path) { next }));
                        }
                    }
                }
            }
            return results;
        }

        public void Dispose() => _subscriptions.ForEach(s => s.Dispose());
    }
}