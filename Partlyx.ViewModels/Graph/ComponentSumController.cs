using DynamicData;
using Partlyx.ViewModels.PartsViewModels.Implementations;
using System.Collections.ObjectModel;

namespace Partlyx.ViewModels.Graph
{
    public class ComponentSumController
    {
        private Dictionary<ResourceViewModel, ComponentNodeSumCompilation> _compilationsDic = new();
        private ObservableCollection<ComponentNodeSumCompilation> _compilations = new();
        public ReadOnlyObservableCollection<ComponentNodeSumCompilation> Compilations { get; }

        public ComponentSumController()
        {
            Compilations = new(_compilations);
        }

        public void ClearComponents()
        {
            _compilationsDic.Clear();
            _compilations.Clear();
        }

        public void AddComponentNode(ComponentGraphNodeViewModel node)
        {
            var component = node.Value as RecipeComponentViewModel;
            if (component == null) return;

            var resource = component.LinkedResource?.Value;
            if (resource == null) return;

            if (!_compilationsDic.ContainsKey(resource))
            {
                var newCompilation = new ComponentNodeSumCompilation(resource);
                _compilationsDic.Add(resource, newCompilation);
                _compilations.Add(newCompilation);
            }

            var compilation = _compilationsDic[resource];
            var sumObject = new ComponentNodeSumObject(node);
            compilation.SumObjectChildren.Add(sumObject);
        }

        public void RemoveComponentNode(ComponentGraphNodeViewModel node)
        {
            var component = node.Value as RecipeComponentViewModel;
            if (component == null) return;

            var resource = component.LinkedResource?.Value;
            if (resource == null) return;

            if (!_compilationsDic.ContainsKey(resource))
                return;

            var compilation = _compilationsDic[resource];

            var sumChild = compilation.SumObjectChildren.FirstOrDefault(x => 
            {
                if (x is not ComponentNodeSumObject componentSumObj) return false;

                return componentSumObj.ComponentNode == node;
            });
            if (sumChild != null)
                compilation.SumObjectChildren.Remove(sumChild);

            if (compilation.SumObjectChildren.Count == 0)
            {
                _compilationsDic.Remove(resource);
                _compilations.Remove(compilation);
            }
        }
    }
}