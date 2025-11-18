using Partlyx.Core.Technical;

namespace Partlyx.Core.OtherSaves
{
    public class SaveScheme
    {
        private static readonly List<SaveScheme> _globalSchemes = new();
        public static IReadOnlyList<SaveScheme> GlobalSchemes => _globalSchemes;
        public static SaveScheme AppUsingSave;
        static SaveScheme()
        {
            AppUsingSave = new("app_using_save", [
                new SchematicSaveEntity("is_first_time_opened", TypeNames.Bool, true),
                ]);

            _globalSchemes.Add(AppUsingSave);
        }
        public SaveScheme(string schemeName, IEnumerable<SchematicSaveEntity> saveProperties)
        {
            SchemeName = schemeName;
            _saveProperties = saveProperties.ToList();
            _savePropertiesDic = _saveProperties.ToDictionary(p => p.Key);
        }
        public string SchemeName { get; set; } = "save";

        private List<SchematicSaveEntity> _saveProperties;
        public IReadOnlyList<SchematicSaveEntity> SaveProperties => _saveProperties;

        private Dictionary<string, SchematicSaveEntity> _savePropertiesDic;
        public Dictionary<string, SchematicSaveEntity> SavePropertiesDic => new(_savePropertiesDic);

        public Dictionary<string, object?> GetAsObjectsDictionary()
        {
            var dic = new Dictionary<string, object?>();
            foreach (var prop in SaveProperties)
            {
                dic.Add(prop.Key, prop.DefaultValue);
            }
            return dic;
        }
    }
    public record SchematicSaveEntity(string Key, string TypeName, object? DefaultValue, bool AllowNull = false);
}
