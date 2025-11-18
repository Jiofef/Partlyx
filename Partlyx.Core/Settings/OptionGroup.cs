using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Core.Settings
{
    public class SchematicOptionsGroup : GroupBase<SchematicOption>
    {
        public List<SchematicOption> Options { get => ContentList; set => ContentList = value; }

        public SchematicOptionsGroup(string name) 
        {
            Name = name;
        }
        public SchematicOptionsGroup WithOptions(IEnumerable<SchematicOption> options)
            => (SchematicOptionsGroup)WithContent(options);
        public List<SchematicOption> GetAsOneLevelOptionsList() => GetAsOneLevelContentList();

        new public SchematicOptionsGroup WithSubGroups(IEnumerable<GroupBase<SchematicOption>> groups)
            => (SchematicOptionsGroup)base.WithSubGroups(groups);
    }
}
