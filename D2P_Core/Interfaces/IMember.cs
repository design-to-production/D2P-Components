using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;

namespace D2P_Core.Interfaces
{
    public interface IMember : IDocObject
    {
        IComponentBase Component { get; set; }

        IMember ParentMember { get; set; }
        IEnumerable<IMember> Members { get; }

        ILayerInfo LayerInfo { get; }
        IEnumerable<GeometryBase> Geometry { get; }
        ObjectAttributes Attributes { get; set; }

        void SetGeometry(GeometryBase geometry);
        void SetGeometry(IEnumerable<GeometryBase> geometry);
    }
    public interface IMember<T> : IMember where T : GeometryBase
    {
        new IEnumerable<T> Geometry { get; }
    }
}
