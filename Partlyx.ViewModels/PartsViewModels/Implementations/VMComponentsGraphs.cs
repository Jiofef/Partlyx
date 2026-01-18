using Partlyx.Infrastructure.Events;
using Partlyx.Services.ServiceImplementations;
using Partlyx.ViewModels.Graph.PartsGraph;
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

            _subscriptions.Add(_bus.Subscribe<RecipeUpdatedViewModelEvent>(_ => _updateInvoker.Invoke(Rebuild)));
            _subscriptions.Add(_bus.Subscribe<RecipeComponentUpdatedViewModelEvent>(_ => _updateInvoker.Invoke(Rebuild)));
            _subscriptions.Add(_bus.Subscribe<PartsVMInitializationFinishedEvent>(_ => _updateInvoker.InvokeAsync(Rebuild)));

            Rebuild();
        }

        public void Rebuild()
        {
            _allGraphs.Clear();
            _resourceToGraph.Clear();
            _componentToGraph.Clear();

            var allComponents = _store.Components.Values.ToList();
            if (allComponents.Count == 0) return;

            foreach (var comp in allComponents)
            {
                if (_componentToGraph.ContainsKey(comp.Uid)) continue;
                var currentGraph = new RecipeComponentGraph();
                _allGraphs.Add(currentGraph);
                FloodFill(comp, currentGraph);
            }

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

                if (current.Resource != null)
                {
                    graph.ResourceUids.Add(current.Resource.Uid);
                    _resourceToGraph[current.Resource.Uid] = graph;

                    if (_store.ComponentsWithResource.TryGetValue(current.Resource.Uid, out var siblings))
                        foreach (var sibling in siblings) stack.Push(sibling);
                }

                var recipe = current.ParentRecipe;
                if (recipe != null)
                    foreach (var rc in recipe.Inputs.Concat(recipe.Outputs)) stack.Push(rc);
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

                // --- 1. INTERNAL CONNECTIONS ---
                if (isInput) // Forward: In -> Out
                    foreach (var output in recipe.Outputs) neighbors.Add(output);

                if (isOutput && recipe.IsReversible) // Reverse: Out -> In
                    foreach (var input in recipe.Inputs) neighbors.Add(input);

                // --- 2. UNIVERSAL RESOURCE BRIDGES ---
                // A component can act as a "result" of a recipe step if:
                // - It's a standard Output (forward move finished)
                // - It's an Input of a Reversible recipe (backward move finished)
                bool isResult = isOutput || (isInput && recipe.IsReversible);

                if (isResult && comp.Resource != null && _store.ComponentsWithResource.TryGetValue(comp.Resource.Uid, out var siblings))
                {
                    foreach (var sibling in siblings)
                    {
                        if (sibling.Uid == comp.Uid || sibling.ParentRecipe == null) continue;
                        var sibRecipe = sibling.ParentRecipe;

                        bool sibIsInput = sibRecipe.Inputs.Any(i => i.Uid == sibling.Uid);
                        bool sibIsOutput = sibRecipe.Outputs.Any(o => o.Uid == sibling.Uid);

                        // A sibling component can be an "entry point" to the next recipe if:
                        // - It's a standard Input (forward move starts)
                        // - It's an Output of a Reversible recipe (backward move starts)
                        bool isEntry = sibIsInput || (sibIsOutput && sibRecipe.IsReversible);

                        if (isEntry)
                        {
                            neighbors.Add(sibling);
                        }
                    }
                }
            }
        }

        public List<RecipeComponentPath> FindPathsBetweenResources(ResourceViewModel startRes, ResourceViewModel endRes, int maxPathLength = 64)
        {
            if (!_resourceToGraph.TryGetValue(startRes.Uid, out var graph) ||
                !_resourceToGraph.TryGetValue(endRes.Uid, out var endGraph) || graph != endGraph)
                return new List<RecipeComponentPath>();

            var results = new List<RecipeComponentPath>();

            // Valid starts: any component of StartRes that can "enter" a recipe (Input or Reversible Output)
            var startNodes = _store.ComponentsWithResource[startRes.Uid]
                .Where(c => c.ParentRecipe != null &&
                           (c.ParentRecipe.Inputs.Any(i => i.Uid == c.Uid) || c.ParentRecipe.IsReversible));

            // Valid ends: any component of EndRes that can "exit" a recipe (Output or Reversible Input)
            var targetUids = _store.ComponentsWithResource[endRes.Uid]
                .Where(c => c.ParentRecipe != null &&
                           (c.ParentRecipe.Outputs.Any(o => o.Uid == c.Uid) || c.ParentRecipe.IsReversible))
                .Select(c => c.Uid).ToHashSet();

            foreach (var startNode in startNodes)
            {
                var queue = new Queue<(RecipeComponentViewModel Node, List<RecipeComponentViewModel> Path)>();
                queue.Enqueue((startNode, new List<RecipeComponentViewModel> { startNode }));

                while (queue.Count > 0)
                {
                    var (current, path) = queue.Dequeue();

                    if (targetUids.Contains(current.Uid) && path.Count >= 2 && path.Count % 2 == 0)
                    {
                        results.Add(RecipeComponentPath.FromList(path));
                        // Continue BFS to find all possible paths
                    }

                    if (path.Count > maxPathLength) continue;

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