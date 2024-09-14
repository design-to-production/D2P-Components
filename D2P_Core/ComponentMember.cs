using D2P_Core.Interfaces;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;

namespace D2P_Core
{
    public class ComponentMember
    {
        public ComponentMember(ILayerInfo layerInfo, IEnumerable<GeometryBase> geometryBases, ObjectAttributes attributes)
        {
            LayerInfo = layerInfo;
            GeometryBases = geometryBases;
            ObjectAttributes = attributes;
        }

        public ILayerInfo LayerInfo { get; set; }
        public IEnumerable<GeometryBase> GeometryBases { get; set; }
        public ObjectAttributes ObjectAttributes { get; set; }
    }
}
