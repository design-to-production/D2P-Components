using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace D2P_Core.Interfaces
{
    public interface IComponentBase : IComponentType, IDocObject
    {
        IMember ParentMember { get; set; }
        IEnumerable<IMember> Members { get; }

        Guid ID { get; set; }
        int GroupIndex { get; }
        string Name { get; }
        string ShortName { get; }
        Plane Plane { get; }

        //IComponentBase ParentMember { get; }
        //IEnumerable<IComponentBase> ChildMembers { get; }
    }
}
