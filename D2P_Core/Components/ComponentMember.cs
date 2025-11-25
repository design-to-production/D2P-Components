using D2P_Core.Interfaces;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;

namespace D2P_Core.Components
{
    public class ComponentMember<T> where T : GeometryBase
    {
        public ILayerInfo LayerInfo { get; set; }
        public IEnumerable<T> Geometry { get; set; }
        public ObjectAttributes Attributes { get; set; }

        public ComponentMember(ILayerInfo layerInfo, IEnumerable<T> geometry, ObjectAttributes attributes = null)
        {
            LayerInfo = layerInfo;
            Geometry = geometry;
            Attributes = attributes;
        }
    }

    public class ComponentMember : ComponentMember<GeometryBase>
    {
        public ComponentMember(ILayerInfo layerInfo, IEnumerable<GeometryBase> geometry, ObjectAttributes attributes = null)
            : base(layerInfo, geometry, attributes)
        { }
    }
}
