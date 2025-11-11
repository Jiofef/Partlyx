using Partlyx.Services.Helpers;
using Partlyx.ViewModels.PartsViewModels.Interfaces;

namespace Partlyx.ViewModels.PartsViewModels
{
    public static class ISelectedPartsExtensions
    {
        public static bool HasAny(this ISelectedParts selected, PartTypeEnumVM type)
        {
            if (type == PartTypeEnumVM.Resource)
                return selected.Resources.Count > 0;
            if (type == PartTypeEnumVM.Recipe)
                return selected.Recipes.Count > 0;
            if (type == PartTypeEnumVM.Component)
                return selected.Components.Count > 0;

            return false;
        }
        public static int GetNotEmptyCollectionsAmount(this ISelectedParts selected)
        {
            return BoolHelper.GetTrueBooleansAmount(
                selected.Resources.Count > 0,
                selected.Recipes.Count > 0,
                selected.Components.Count > 0);
        }
        public static bool HasOnly(this ISelectedParts selected, PartTypeEnumVM type)
        {
            int notEmptyCollectionsAmount = selected.GetNotEmptyCollectionsAmount();
            bool result = notEmptyCollectionsAmount == 1 && selected.HasAny(type);
            return result;
        }
        public static PartTypeEnumVM? GetOnlyNotEmptyCollectionPartsTypeOrNull(this ISelectedParts selected)
        {
            int notEmptyCollectionsAmount = selected.GetNotEmptyCollectionsAmount();

            if (selected.Resources.Count > 0)
                return PartTypeEnumVM.Resource;
            if (selected.Recipes.Count > 0)
                return PartTypeEnumVM.Recipe;
            if (selected.Components.Count > 0)
                return PartTypeEnumVM.Component;

            return null;
        }
        public static List<IVMPart> GetAllTheParts(this ISelectedParts selected)
        {
            var parts = new List<IVMPart>();

            parts.AddRange(selected.Resources);
            parts.AddRange(selected.Recipes);
            parts.AddRange(selected.Components);

            return parts;
        }
    }
}
