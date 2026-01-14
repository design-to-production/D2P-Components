using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace D2P_Core.Interfaces {
    public interface IComponentBase : IMemberCollection, IComponentType, IDocObject<IComponentBase> {
        IMember<TextEntity> Label { get; }

        Guid ID { get; set; }
        int GroupIndex { get; set; }
        string Name { get; }
        string ShortName { get; set; }
        Plane Plane { get; set; }

        IEnumerable<GeometryBase> Geometry { get; }

        //IMember this[string name] { get; set; }

        bool Transform(Transform xform);
    }
}
