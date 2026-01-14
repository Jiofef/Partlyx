using DynamicData;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using System.Collections.ObjectModel;
using ReactiveUI;

namespace Partlyx.ViewModels.Graph.PartsGraph
{
    public class ComponentSumController : IDisposable
    {
        private readonly SourceCache<ComponentNodeSumCompilation, ResourceViewModel> _compilationsCache = new(x => x.ComponentsResource!);

        private readonly ReadOnlyObservableCollection<ComponentNodeSumCompilation> _positiveCompilations;
        public ReadOnlyObservableCollection<ComponentNodeSumCompilation> PositiveCompilations => _positiveCompilations;

        private readonly ReadOnlyObservableCollection<ComponentNodeSumCompilation> _negativeCompilations;
        public ReadOnlyObservableCollection<ComponentNodeSumCompilation> NegativeCompilations => _negativeCompilations;

        private readonly IDisposable _cleanup;

        public ComponentSumController()
        {
            // Filter positive compilations
            var positiveSub = _compilationsCache.Connect()
                .AutoRefresh(x => x.Sum) // Tracks Sum changes to re-evaluate filter
                .Filter(x => x.Sum >= 0)
                .Bind(out _positiveCompilations)
                .Subscribe();

            // Filter negative compilations
            var negativeSub = _compilationsCache.Connect()
                .AutoRefresh(x => x.Sum)
                .Filter(x => x.Sum < 0)
                .Bind(out _negativeCompilations)
                .Subscribe();

            _cleanup = System.Reactive.Disposables.Disposable.Create(() =>
            {
                positiveSub.Dispose();
                negativeSub.Dispose();
            });
        }

        public void ClearComponents()
        {
            var compilations = _compilationsCache.Items.ToList();
            _compilationsCache.Clear();

            foreach (var compilation in compilations)
            {
                compilation.Dispose();
            }
        }

        public void AddComponentNode(ComponentGraphNodeViewModel node)
        {
            if (node.Value is not RecipeComponentViewModel component ||
                component.LinkedResource?.Value is not ResourceViewModel resource)
                return;

            var compilation = _compilationsCache.Lookup(resource);
            ComponentNodeSumCompilation targetCompilation;

            if (!compilation.HasValue)
            {
                targetCompilation = new ComponentNodeSumCompilation(resource);
                _compilationsCache.AddOrUpdate(targetCompilation);
            }
            else
            {
                targetCompilation = compilation.Value;
            }

            var sumObject = new ComponentNodeSumObject(node);
            targetCompilation.SumObjectChildren.Add(sumObject);
        }

        public void RemoveComponentNode(ComponentGraphNodeViewModel node)
        {
            if (node.Value is not RecipeComponentViewModel component ||
                component.LinkedResource?.Value is not ResourceViewModel resource)
                return;

            var compilationOpt = _compilationsCache.Lookup(resource);
            if (!compilationOpt.HasValue) return;

            var compilation = compilationOpt.Value;
            var sumChild = compilation.SumObjectChildren
                .FirstOrDefault(x => x is ComponentNodeSumObject cso && cso.ComponentNode == node);

            if (sumChild != null)
            {
                compilation.SumObjectChildren.Remove(sumChild);
                (sumChild as IDisposable)?.Dispose();
            }

            if (compilation.SumObjectChildren.Count == 0)
            {
                _compilationsCache.Remove(resource);
                compilation.Dispose();
            }
        }

        public void Dispose()
        {
            _cleanup.Dispose();
            _compilationsCache.Dispose();
        }
    }
}