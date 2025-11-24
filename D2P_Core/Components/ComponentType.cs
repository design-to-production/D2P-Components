using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Rhino.DocObjects;
using System.Drawing;

namespace D2P_Core.Components
{
    public class ComponentType : IComponentType
    {
        public string TypeID { get; set; }
        public string TypeName { get; set; }
        public double LabelSize { get; set; }
        public Color LayerColor { get; set; }
        public Settings Settings { get; set; }

        public ComponentType(string typeID, string typeName, Settings settings = null, double? labelSize = null, Color? layerColor = null)
        {
            TypeID = typeID;
            TypeName = typeName;
            LabelSize = labelSize ?? Rhino.RhinoDoc.ActiveDoc.DimStyles.Current.TextHeight;
            LayerColor = layerColor ?? Color.Black;
            Settings = settings ?? Settings.Default;
        }

        public ComponentType(Layer layer, Settings settings)
        {
            TypeID = Layers.GetComponentTypeID(layer, settings);
            TypeName = Layers.GetComponentTypeName(layer, settings);
            LabelSize = Layers.GetComponentTypeLabelSize(layer, settings);
            LayerColor = layer.Color;
            Settings = Layers.GetComponentTypeSettings(layer, settings);
        }
        private ComponentType(IComponentType type)
        {
            TypeID = type.TypeID;
            TypeName = type.TypeName;
            LabelSize = type.LabelSize;
            LayerColor = type.LayerColor;
            Settings = type.Settings;
        }

        public ComponentType Clone() => new ComponentType(this);
    }
}
