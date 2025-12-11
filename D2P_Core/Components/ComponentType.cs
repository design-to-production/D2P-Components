using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Rhino;
using Rhino.DocObjects;
using System.Drawing;

namespace D2P_Core.Components
{
    public class ComponentType : IComponentType
    {
        public Settings Settings { get; }

        public string TypeId { get; }
        public string TypeName { get; }
        public double LabelSize { get; }
        public Color LayerColor { get; }

        public ComponentType(string typeID, string typeName, Settings settings = null, double? labelSize = null, Color? layerColor = null)
        {
            TypeId = typeID;
            TypeName = typeName;
            LabelSize = labelSize ?? RhinoDoc.ActiveDoc.DimStyles.Current.TextHeight;
            LayerColor = layerColor ?? Color.Black;
            Settings = settings ?? Settings.Default;
        }

        public ComponentType(Layer layer, Settings settings)
        {
            TypeId = Layers.GetComponentTypeID(layer, settings);
            TypeName = Layers.GetComponentTypeName(layer, settings);
            LabelSize = Layers.GetComponentTypeLabelSize(layer, settings);
            LayerColor = layer.Color;
            Settings = Layers.GetComponentTypeSettings(layer, settings);
        }
    }
}
