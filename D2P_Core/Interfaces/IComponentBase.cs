using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace D2P_Core.Interfaces
{
    public interface IComponentBase : IComponentType, IDocObject
    {
        IMember<TextEntity> Label { get; }

        Guid ID { get; set; }
        int GroupIndex { get; }
        string Name { get; }
        string ShortName { get; set; }
        Plane Plane { get; set; }

        IMember ParentMember { get; set; }
        IEnumerable<IMember> Members { get; }
        IEnumerable<GeometryBase> Geometry { get; }

        IMember this[string name] { get; set; }

        bool Transform(Transform xform);
    }
}
