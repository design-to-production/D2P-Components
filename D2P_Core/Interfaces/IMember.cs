using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;

namespace D2P_Core.Interfaces
{
    public interface IMember<T> where T : GeometryBase
    {
        ILayerInfo LayerInfo { get; }
        IEnumerable<T> Geometry { get; }
        ObjectAttributes Attributes { get; set; }
    }
}
