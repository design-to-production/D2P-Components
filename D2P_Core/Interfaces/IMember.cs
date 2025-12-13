using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;

namespace D2P_Core.Interfaces
{
    public interface IMember : IDocObject
    {
        IComponentBase Component { get; set; }

        IMember ParentMember { get; set; }
        IEnumerable<IMember> Members { get; set; }

        string Name { get; set; }

        ILayerInfo LayerInfo { get; set; }
        IEnumerable<GeometryBase> Geometry { get; set; }
        ObjectAttributes Attributes { get; set; }

        void SetGeometry(GeometryBase geometry);
        void SetGeometry(IEnumerable<GeometryBase> geometry);
    }
    public interface IMember<T> : IMember where T : GeometryBase
    {
        new IEnumerable<T> Geometry { get; }
    }
}
