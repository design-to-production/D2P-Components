using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace D2P_Core.Interfaces
{
    public interface IComponentBase : IComponentType, IDocObject, ICloneable
    {
        IMember ParentMember { get; set; }
        IEnumerable<IMember> Members { get; }
        IEnumerable<GeometryBase> Geometry { get; }

        IMember this[string name] { get; set; }

        Guid ID { get; set; }
        int GroupIndex { get; }
        string Name { get; }
        string ShortName { get; }
        Plane Plane { get; }

        bool Transform(Transform xform);
    }
}
