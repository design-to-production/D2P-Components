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

        //public static ComponentMember<T> Get(string rawLayerName, IComponent component)
        //{
        //    var layer = Layers.FindLayer(component, rawLayerName, out int layersFound);
        //    if (layer == null || layersFound != 1) return null;

        //    var layerInfo = new LayerInfo(rawLayerName, layer.Color);
        //    var rhObjects = Objects.ObjectsByLayer(layer, component.ActiveDoc);
        //    var geometry = rhObjects.Select(obj => obj.Geometry as T).Where(geo => geo != null);
        //    var attribute = rhObjects.Select(obj => obj.Attributes).FirstOrDefault();

        //    return new ComponentMember<T>(layerInfo, geometry, attribute);
        //}

        //public static void Set(IComponent component, ComponentMember<T> member)
        //{
        //    component.ReplaceMember(member);
        //}
    }

    public class ComponentMember : ComponentMember<GeometryBase>
    {
        public ComponentMember(ILayerInfo layerInfo, IEnumerable<GeometryBase> geometry, ObjectAttributes attributes = null)
            : base(layerInfo, geometry, attributes)
        { }
    }
}
