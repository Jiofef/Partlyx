using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.GraphicsViewModels.HierarchyViewModels
{
    public static class MultiParentHierarchyTransformObjectExtensions
    {
        public static void AddChildren<TMPHO1, TMPHO2>(this ICollection<TMPHO1> group, ICollection<TMPHO2> secondGroup)
            where TMPHO1 : MultiParentHierarchyTransformObject
            where TMPHO2 : MultiParentHierarchyTransformObject
        {
            foreach (var firstGroupElement in group)
            {
                foreach (var secondGroupElement in secondGroup)
                    firstGroupElement.AddChild(secondGroupElement);
            }
        }
        public static void AddChildren<TMPHO1, TMPHO2>(this TMPHO1 element, ICollection<TMPHO2> group) 
            where TMPHO1 : MultiParentHierarchyTransformObject
            where TMPHO2 : MultiParentHierarchyTransformObject
        {
            foreach (var groupElement in group)
                element.AddChild(groupElement);        
        }

        public static void AddChildToAll<TMPHO1, TMPHO2>(this ICollection<TMPHO1> group, TMPHO2 element)
            where TMPHO1 : MultiParentHierarchyTransformObject
            where TMPHO2 : MultiParentHierarchyTransformObject
        {
            foreach (var groupElement in group)
                groupElement.AddChild(element);
        }
    }
}
