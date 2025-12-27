using Partlyx.Infrastructure.Events;
using Partlyx.Services.ServiceImplementations;
using Partlyx.ViewModels.Graph;
using Partlyx.ViewModels.PartsViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Partlyx.ViewModels.PartsViewModels.Implementations
{
    // Class events
    public record ComponentGraphsUpdatedEvent();
    public class VMComponentsGraphs : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IVMPartsStore _store;
        private readonly List<IDisposable> _subscriptions = new();

        public class RecipeComponentGraph
        {
            public HashSet<RecipeComponentViewModel> Components { get; } = new();
            public HashSet<ResourceViewModel> Resources { get; } = new();
            public Dictionary<RecipeComponentViewModel, HashSet<RecipeComponentViewModel>> AdjacencyList { get; } = new();
        }

        private Dictionary<Guid, RecipeComponentGraph> _graphsByResourceUid = new();
        private Dictionary<Guid, RecipeComponentGraph> _componentToGraph = new();
        private List<RecipeComponentGraph> _allGraphs = new();
        private readonly Dictionary<(int, int), List<RecipeComponentPath>> _pathCache = new();

        private readonly ThrottledInvoker _updateInvoker = new(TimeSpan.FromMilliseconds(100));
        public VMComponentsGraphs(IEventBus bus, IVMPartsStore store)
        {
            _bus = bus;
            _store = store;

            // Subscribe to events
            _subscriptions.Add(_bus.Subscribe<RecipeComponentVMAddedToStoreEvent>(ev => _updateInvoker.InvokeAsync(BuildGraphs)));
            _subscriptions.Add(_bus.Subscribe<RecipeComponentVMRemovedFromStoreEvent>(ev => _updateInvoker.InvokeAsync(BuildGraphs)));
            _subscriptions.Add(_bus.Subscribe<RecipeVMAddedToStoreEvent>(ev => _updateInvoker.InvokeAsync(BuildGraphs)));
            _subscriptions.Add(_bus.Subscribe<RecipeVMRemovedFromStoreEvent>(ev => _updateInvoker.InvokeAsync(BuildGraphs)));
            _subscriptions.Add(_bus.Subscribe<ResourceVMAddedToStoreEvent>(ev => _updateInvoker.InvokeAsync(BuildGraphs)));
            _subscriptions.Add(_bus.Subscribe<ResourceVMRemovedFromStoreEvent>((ev) => RebuildGraphs()));

            BuildGraphs();
        }

        public RecipeComponentGraph? GetGraphForResource(Guid resourceUid)
        {
            _graphsByResourceUid.TryGetValue(resourceUid, out var graph);
            return graph;
        }

        private void BuildGraphs()
        {
            _graphsByResourceUid.Clear();
            _componentToGraph.Clear();
            _allGraphs.Clear();
            _pathCache.Clear();

            // Index all components by their Resource UID for O(1) lookups
            var componentsByResource = _store.Components.Values
                .GroupBy(c => c.Resource.Uid)
                .ToDictionary(g => g.Key, g => g.ToList());

            var unvisited = new HashSet<RecipeComponentViewModel>(_store.Components.Values);

            while (unvisited.Count > 0)
            {
                var start = unvisited.First();
                var graph = new RecipeComponentGraph();

                TraverseAndCollect(start, graph, unvisited, componentsByResource);

                _allGraphs.Add(graph);
                foreach (var comp in graph.Components)
                {
                    _componentToGraph[comp.Uid] = graph;
                    _graphsByResourceUid[comp.Resource.Uid] = graph;
                    UpdateAdjacency(comp, graph, componentsByResource);
                }
            }

            _bus.Publish(new ComponentGraphsUpdatedEvent());
        }



        public RecipeComponentPath? FindShortestPath(RecipeComponentViewModel from, RecipeComponentViewModel to)
        {
            var graph = GetGraphForResource(from.Resource.Uid);
            if (graph == null || !graph.Components.Contains(to)) return null;

            // BFS to find shortest path
            var queue = new Queue<List<RecipeComponentViewModel>>();
            var visited = new HashSet<RecipeComponentViewModel>();
            queue.Enqueue(new List<RecipeComponentViewModel> { from });
            visited.Add(from);

            while (queue.Count > 0)
            {
                var path = queue.Dequeue();
                var current = path.Last();

                if (current == to)
                {
                    return RecipeComponentPath.FromList(path);
                }

                // Get neighbors
                var neighbors = GetNeighbors(current, graph);
                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        var newPath = new List<RecipeComponentViewModel>(path) { neighbor };
                        queue.Enqueue(newPath);
                    }
                }
            }

            return null;
        }

        public List<RecipeComponentPath> FindPathsBetweenSets(HashSet<RecipeComponentViewModel> startSet, HashSet<RecipeComponentViewModel> endSet, int maxPaths = 5)
        {
            if (startSet.Count == 0 || endSet.Count == 0) return new();

            // Generate cache key
            int startKey = GetSetHashCode(startSet);
            int endKey = GetSetHashCode(endSet);
            var cacheKey = (startKey, endKey);

            if (_pathCache.TryGetValue(cacheKey, out var cachedPaths))
                return cachedPaths;

            var results = PerformMultiSourceBFS(startSet, endSet, maxPaths);

            _pathCache[cacheKey] = results;
            return results;
        }

        private List<RecipeComponentPath> PerformMultiSourceBFS(HashSet<RecipeComponentViewModel> startSet, HashSet<RecipeComponentViewModel> endSet, int maxPaths)
        {
            var results = new List<RecipeComponentPath>();
            var firstComp = startSet.First();

            if (!_componentToGraph.TryGetValue(firstComp.Uid, out var graph)) return results;

            // Queue: (Current node, Path to it)
            var queue = new Queue<(RecipeComponentViewModel Node, List<RecipeComponentViewModel> Path)>();
            var visited = new HashSet<RecipeComponentViewModel>();

            // Multi-source: initialize with all start nodes
            foreach (var startNode in startSet)
            {
                queue.Enqueue((startNode, new List<RecipeComponentViewModel> { startNode }));
                visited.Add(startNode);
            }

            while (queue.Count > 0 && results.Count < maxPaths)
            {
                var (current, path) = queue.Dequeue();

                if (endSet.Contains(current))
                {
                    results.Add(RecipeComponentPath.FromList(path));
                    if (results.Count >= maxPaths) break;
                    // Continue to find other paths
                }

                if (graph.AdjacencyList.TryGetValue(current, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            var newPath = new List<RecipeComponentViewModel>(path) { neighbor };
                            queue.Enqueue((neighbor, newPath));
                        }
                    }
                }
            }
            return results;
        }

        private int GetSetHashCode(HashSet<RecipeComponentViewModel> set)
        {
            // Combined hash for set (order-independent)
            int hash = 0;
            foreach (var item in set) hash ^= item.Uid.GetHashCode();
            return hash;
        }

        private IEnumerable<RecipeComponentViewModel> GetNeighbors(RecipeComponentViewModel component, RecipeComponentGraph graph)
        {
            // Use AdjacencyList for direct neighbors
            if (graph.AdjacencyList.TryGetValue(component, out var neighbors))
            {
                return neighbors;
            }

            return Enumerable.Empty<RecipeComponentViewModel>();
        }

        private void TraverseAndCollect(
            RecipeComponentViewModel node, 
            RecipeComponentGraph graph, 
            HashSet<RecipeComponentViewModel> unvisited,
            Dictionary<Guid, List<RecipeComponentViewModel>> componentsByResource)
        {
            var stack = new Stack<RecipeComponentViewModel>();
            stack.Push(node);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (!unvisited.Contains(current)) continue;

                unvisited.Remove(current);
                graph.Components.Add(current);

                var related = GetStructuralConnections(current, componentsByResource);
                foreach (var r in related)
                {
                    if (unvisited.Contains(r)) 
                        stack.Push(r);
                }
            }
        }

        private IEnumerable<RecipeComponentViewModel> GetStructuralConnections(
            RecipeComponentViewModel comp, 
            Dictionary<Guid, List<RecipeComponentViewModel>> componentsByResource)
        {
            var list = new List<RecipeComponentViewModel>();

            // Recipe connections
            if (comp.LinkedParentRecipe?.Value is RecipeViewModel r)
            {
                list.AddRange(r.Inputs);
                list.AddRange(r.Outputs);
            }

            // Resource identity connections using the pre-built index
            if (componentsByResource.TryGetValue(comp.Resource.Uid, out var siblings))
            {
                list.AddRange(siblings);
            }

            return list;
        }

        private void UpdateAdjacency(
            RecipeComponentViewModel comp, 
            RecipeComponentGraph graph,
            Dictionary<Guid, List<RecipeComponentViewModel>> componentsByResource)
        {
            if (!graph.AdjacencyList.TryGetValue(comp, out var neighbors))
            {
                neighbors = new HashSet<RecipeComponentViewModel>();
                graph.AdjacencyList[comp] = neighbors;
            }

            // Directional recipe logic
            if (comp.LinkedParentRecipe?.Value is RecipeViewModel recipe)
            {
                if (recipe.Inputs.Contains(comp))
                {
                    foreach (var output in recipe.Outputs) 
                        neighbors.Add(output);
                }
                else if (recipe.IsReversible)
                {
                    foreach (var input in recipe.Inputs) 
                        neighbors.Add(input);
                }
            }

            // Connect same-resource components within the current graph boundary
            if (componentsByResource.TryGetValue(comp.Resource.Uid, out var allSiblings))
            {
                foreach (var sibling in allSiblings)
                {
                    if (sibling != comp && graph.Components.Contains(sibling))
                    {
                        neighbors.Add(sibling);
                    }
                }
            }
        }

        private void RebuildGraphs()
        {
            BuildGraphs();
        }

        public void Dispose()
        {
            foreach (var sub in _subscriptions)
            {
                sub.Dispose();
            }
        }
    }
}
