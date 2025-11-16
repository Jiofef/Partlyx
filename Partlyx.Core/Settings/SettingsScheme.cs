using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using static System.Text.Json.JsonSerializer;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Partlyx.Core.Technical;

namespace Partlyx.Core.Settings
{
    public class SettingsScheme
    {
        public static SettingsScheme ApplicationSettings { get; }
        static SettingsScheme()
        {
            var optionsGroup = new SchematicOptionsGroupEntity("Options")
                .WithSubGroups
                ([
                    new SchematicOptionsGroupEntity("settings_General").WithOptions([
                        new SchematicOption(Key: SettingKeys.Language, Name: "Language", DefaultValueJson: Serialize(CultureInfo.CurrentCulture.Name), TypeName: TypeNames.Language),
                        ]),
                ]);

            ApplicationSettings = new SettingsScheme(optionsGroup);
        }

        public SettingsScheme(SchematicOptionsGroupEntity optionsGroup)
        {
            OptionsGroup = optionsGroup.ToReadOnly();

            _options = optionsGroup.GetAsOneLevelOptionsList();
            UpdateDictionaryFromOptionsList();
        }
        private void UpdateDictionaryFromOptionsList()
        {
            _optionsDictionary = new();

            foreach (var opt in _options)
               _optionsDictionary.Add(opt.Key, opt);
        }

        private Dictionary<string, SchematicOption> _optionsDictionary = new();
        public ReadOnlyDictionary<string, SchematicOption> OptionsDictionary => new(_optionsDictionary);

        public ReadonlyOptionsGroupEntity OptionsGroup { get; }

        private List<SchematicOption> _options = new();
        public IReadOnlyList<SchematicOption> Options => _options;
    }
}
