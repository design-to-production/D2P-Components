using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Rhino.DocObjects;
using System.Drawing;

namespace D2P_Core
{
    public class ComponentType : IComponentType
    {
        public ComponentType(string typeID, string typeName, Settings settings, double? labelSize, Color? layerColor)
        {
            TypeID = typeID;
            TypeName = typeName;
            LabelSize = labelSize ?? Rhino.RhinoDoc.ActiveDoc.DimStyles.Current.TextHeight;
            LayerColor = layerColor ?? Color.Black;
            Settings = settings;
        }

        public ComponentType(IComponent component)
        {
            TypeID = component.TypeID;
            TypeName = component.TypeName;
            LabelSize = component.LabelSize;
            LayerColor = component.LayerColor;
            Settings = component.Settings;
        }

        public ComponentType(Layer layer, Settings settings)
        {
            TypeID = Layers.GetComponentTypeID(layer, settings);
            TypeName = Layers.GetComponentTypeName(layer, settings);
            LabelSize = Layers.GetComponentTypeLabelSize(layer, settings);
            LayerColor = layer.Color;
            Settings = settings;
        }

        public ComponentType(TextObject textObj, Settings settings)
        {
            TypeID = Objects.ComponentTypeIDFromObject(textObj, settings);
            TypeName = Objects.ComponentTypeNameFromObject(textObj, settings);
            LabelSize = textObj.TextGeometry.TextHeight;
            LayerColor = Objects.ComponentTypeLayerColorFromObject(textObj, settings);
            Settings = settings;
        }

        public string TypeID { get; set; }
        public string TypeName { get; set; }
        public double LabelSize { get; set; }
        public Color LayerColor { get; set; }
        public Settings Settings { get; set; }
    }
}
