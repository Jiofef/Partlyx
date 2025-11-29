using Material.Icons;
using Partlyx.Services.Helpers;
using Partlyx.ViewModels.GraphicsViewModels.IconViewModels;
using Partlyx.ViewModels.UIObjectViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using static Material.Icons.MaterialIconKind;

namespace Partlyx.UI.Avalonia.VMImplementations
{
    public class MaterialAvaloniaVectorIconCatalog : IIconVectorCatalog
    {
        public string LibraryName => "MaterialIcons";
        private readonly List<string> _allTheIconKeys;
        private readonly ReadOnlyDictionary<int, string[]> _allTheIconKeyValueGroupsDic;
        private readonly IReadOnlyList<int> _allTheIconKeysList;

        private readonly List<string> _baseIconKeys;
        private readonly Dictionary<int, string[]> _baseIconKeyValueGroupsDic;
        private readonly IReadOnlyList<int> _baseIconKeysList;

        private readonly List<MaterialIconKind> _baseIconKinds = new()
        {
            Circle, Rectangle, Triangle
        };

        public MaterialAvaloniaVectorIconCatalog()
        {
            _allTheIconKeys = Enum.GetNames(typeof(MaterialIconKind)).ToList();
            _allTheIconKeysList = EnumMapper<MaterialIconKind>.GetOrderedKeys();
            _allTheIconKeyValueGroupsDic = EnumMapper<MaterialIconKind>.GetKeyValueGroups();

            _baseIconKeys = _baseIconKinds.Select(k => k.ToString()).ToList();
            _baseIconKeysList = _baseIconKinds.Select(k => (int)k).ToList();
            _baseIconKeyValueGroupsDic = EnumMapper<MaterialIconKind>.GetKeyValueGroupsForValues(_baseIconKeysList);
        }

        public IReadOnlyList<string> GetAllIconKeys()
            => _allTheIconKeys;
        public IReadOnlyList<string> GetBaseIconKeys()
            => _baseIconKeys;

        private List<StoreVectorIconContentViewModel>? _allIconsContentForStoreCache;
        public IReadOnlyList<StoreVectorIconContentViewModel> GetAllIconsContentForStore(bool returnCachedIfPossible = true)
        {
            if (returnCachedIfPossible && _allIconsContentForStoreCache != null)
                return _allIconsContentForStoreCache;

            var list = new List<StoreVectorIconContentViewModel>(_allTheIconKeysList.Count);
            foreach (var key in _allTheIconKeysList)
            {
                var valueNames = _allTheIconKeyValueGroupsDic[key];
                string firstValueName = valueNames[0];
                var iconContent = new StoreVectorIconContentViewModel(firstValueName)
                {
                    SearchKeys = valueNames.ToList()
                };
                list.Add(iconContent);
            }

            _allIconsContentForStoreCache = list;
            return list;
        }

        private List<StoreVectorIconContentViewModel>? _baseIconsContentForStoreCache;
        public IReadOnlyList<StoreVectorIconContentViewModel> GetBaseIconsContentForStore(bool returnCachedIfPossible = true)
        {
            if (returnCachedIfPossible && _baseIconsContentForStoreCache != null)
                return _baseIconsContentForStoreCache;

            var list = new List<StoreVectorIconContentViewModel>(_baseIconKeysList.Count);
            foreach (var key in _baseIconKeysList)
            {
                var valueNames = _baseIconKeyValueGroupsDic[key];
                string firstValueName = valueNames[0];
                var iconContent = new StoreVectorIconContentViewModel(firstValueName)
                {
                    SearchKeys = valueNames.ToList()
                };
                list.Add(iconContent);
            }

            _baseIconsContentForStoreCache = list;
            return list;
        }
    }
}
