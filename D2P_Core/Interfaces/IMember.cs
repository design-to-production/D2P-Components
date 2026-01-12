using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;

namespace D2P_Core.Interfaces {
    public interface IMember : IMemberCollection, IDocObject<IMember> {
        bool ReplaceExisting { get; set; }
        IComponentBase Component { get; set; }

        ILayerInfo LayerInfo { get; set; }
        ObjectAttributes Attributes { get; set; }
        IEnumerable<GeometryBase> Geometry { get; }

        void SetGeometry(GeometryBase geometry);
        void SetGeometry(IEnumerable<GeometryBase> geometry);
    }

    public interface IMember<T> : IMember where T : GeometryBase {
        new IEnumerable<T> Geometry { get; }
        new IMember<T> Duplicate();
    }
}
