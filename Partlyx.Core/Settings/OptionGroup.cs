using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Core.Settings
{
    public class SchematicOptionsGroupEntity
    {
        public string Name;
        public List<SchematicOption> Options = new();
        public List<SchematicOptionsGroupEntity> SubGroups = new();

        public SchematicOptionsGroupEntity(string name) 
        {
            Name = name;
        }
        public SchematicOptionsGroupEntity WithOptions(IEnumerable<SchematicOption> options)
        {
            Options.AddRange(options);
            return this;
        }
        public SchematicOptionsGroupEntity WithSubGroups(IEnumerable<SchematicOptionsGroupEntity> groups)
        {
            SubGroups.AddRange(groups);
            return this;
        }
        public List<SchematicOption> GetAsOneLevelOptionsList()
        {
            var list = new List<SchematicOption>();

            RecursiveAddToList(this);

            void RecursiveAddToList(SchematicOptionsGroupEntity group)
            {
                list.AddRange(group.Options);
                foreach (var subGroup in group.SubGroups)
                    RecursiveAddToList(subGroup);
            }

            return list;
        }

        public ReadonlyOptionsGroupEntity ToReadOnly()
            => new ReadonlyOptionsGroupEntity(Name, Options, SubGroups);
    }

    public class ReadonlyOptionsGroupEntity
    {
        public ReadonlyOptionsGroupEntity(string name, List<SchematicOption> options, List<SchematicOptionsGroupEntity> subGroups) 
        {
            Name = name;
            Options = options;
            SubGroups = subGroups.Select(sg => sg.ToReadOnly()).ToList(); // Making all the subgroups readonly
        }
        public string Name { get; }
        public IReadOnlyList<SchematicOption> Options;
        public IReadOnlyList<ReadonlyOptionsGroupEntity> SubGroups;
    }
}
