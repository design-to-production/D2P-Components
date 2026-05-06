using D2P.Core.Interfaces;
using D2P.Core.Utility;
using Rhino;
using Rhino.DocObjects;
using System.Drawing;

namespace D2P.Core.Components {
    public class ComponentType : IComponentType {
        public string TypeId { get; set; }
        public string TypeName { get; set; }
        public double LabelSize { get; set; }
        public Color LayerColor { get; set; }

        public ComponentType(string typeID, string typeName, double? labelSize = null, Color? layerColor = null)
        {
            TypeId = typeID;
            TypeName = typeName;
            LabelSize = labelSize ?? RhinoDoc.ActiveDoc.DimStyles.Current.TextHeight;
            LayerColor = layerColor ?? Color.Black;
        }

        public ComponentType(Layer layer)
        {
            TypeId = Layers.GetComponentTypeID(layer);
            TypeName = Layers.GetComponentTypeName(layer);
            LabelSize = Layers.GetComponentTypeLabelSize(layer);
            LayerColor = layer.Color;
        }
    }
}
