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
using Microsoft.Extensions.Options;

namespace Partlyx.Core.Settings
{
    public class SettingsScheme
    {
        public static SettingsScheme ApplicationSettings { get; }
        static SettingsScheme()
        {
            var optionsGroup = new SchematicOptionsGroup("Options")
                .WithSubGroups
                ([
                    new SchematicOptionsGroup("settings_General").WithOptions([
                        new SchematicOption(Key: SettingKeys.Language, Name: "Language", DefaultValueJson: Serialize(CultureInfo.CurrentCulture.Name), TypeName: TypeNames.Language),
                        ]),
                    new SchematicOptionsGroup("settings_Parts").WithOptions([
                        new SchematicOption(Key: SettingKeys.CreateResourceWithRecipeByDefault, Name: "Create_resource_with_recipe_by_default", DefaultValueJson: Serialize(false), TypeName: TypeNames.Bool),
                        new SchematicOption(Key: SettingKeys.DefaultRecipeOutputAmount, Name: "Default_recipe_craft_amount", DefaultValueJson: Serialize(1.0), TypeName: TypeNames.Double),
                        new SchematicOption(Key: SettingKeys.DefaultRecipeInputAmount, Name: "Default_component_quantity", DefaultValueJson: Serialize(1.0), TypeName: TypeNames.Double),
                        ]),
                ]);

            ApplicationSettings = new SettingsScheme(optionsGroup);
        }

        public SettingsScheme(SchematicOptionsGroup optionsGroup)
        {
            OptionsGroup = optionsGroup.ToReadOnlyGroup();

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

        public ReadonlyGroup<SchematicOption> OptionsGroup { get; }

        private List<SchematicOption> _options = new();
        public IReadOnlyList<SchematicOption> Options => _options;
    }
}
