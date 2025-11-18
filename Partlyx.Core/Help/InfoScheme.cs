using Partlyx.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Core.Help
{
    public class InfoScheme
    {
        public static InfoScheme ApplicationHelp { get; }
        static InfoScheme()
        {
            var howToUsePartlyxSection = new InfoSection(Key: "help_What_is_Partlyx", ContentKey: "help_text_What_is_Partlyx");
            var helpSectionsGroup = new InfoSectionsGroup("Help")
            .WithSubGroups
            ([
                new InfoSectionsGroup("help_HowToUsePartlyx").WithSections([
                                howToUsePartlyxSection,
                                new InfoSection(Key: "help_Parts_tree", ContentKey:"help_text_Parts_tree"),
                                new InfoSection(Key: "help_Some_QOL_Features", ContentKey:"help_text_Some_QOL_Features"),
                                ]),
            ]);
            ApplicationHelp = new InfoScheme(helpSectionsGroup, howToUsePartlyxSection);
        }

        public InfoScheme(GroupBase<InfoSection> group, InfoSection mainSection)
        {
            InfoGroup = group.ToReadOnlyGroup();
            MainSection = mainSection;
        }
        public ReadonlyGroup<InfoSection> InfoGroup { get; }
        public InfoSection MainSection { get; }
    }
}
