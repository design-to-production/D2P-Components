using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;

namespace D2P_Core.Interfaces
{
    public interface IMember : IDocMember
    {
        IComponentBase Component { get; }
        IMember Parent { get; set; }
        IEnumerable<IMember> Children { get; }

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
