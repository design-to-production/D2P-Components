using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;

namespace D2P_Core.Interfaces
{
    public interface IMember<T> : IDocMember where T : GeometryBase
    {
        IComponentBase Component { get; }
        IEnumerable<IMember> Children { get; }

        ILayerInfo LayerInfo { get; }
        IEnumerable<T> Geometry { get; }
        T FirstGeometry { get; }
        ObjectAttributes Attributes { get; set; }

        void SetGeometry(T geometry);
        void SetGeometry(IEnumerable<T> geometry);
    }

    public interface IMember : IMember<GeometryBase> { }
}
