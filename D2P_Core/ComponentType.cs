using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Rhino.DocObjects;
using System.Drawing;

namespace D2P_Core
{
    public class ComponentType : IComponentType
    {
        public string TypeID { get; set; }
        public string TypeName { get; set; }
        public double LabelSize { get; set; } = 50;
        public Color LayerColor { get; set; } = Color.Black;
        public Settings Settings { get; set; } = Settings.Default;

        public ComponentType(string typeID, string typeName)
        {
            TypeID = typeID;
            TypeName = typeName;
        }

        public ComponentType(Layer layer, Settings settings)
        {
            TypeID = Layers.GetComponentTypeID(layer, settings);
            TypeName = Layers.GetComponentTypeName(layer, settings);
            LabelSize = Layers.GetComponentTypeLabelSize(layer, settings);
            LayerColor = layer.Color;
            Settings = Layers.GetComponentTypeSettings(layer, settings);
        }

        public ComponentType(TextObject textObj, Settings settings)
        {
            TypeID = Objects.ComponentTypeIDFromObject(textObj, settings);
            TypeName = Objects.ComponentTypeNameFromObject(textObj, settings);
            LabelSize = textObj.TextGeometry.TextHeight;
            LayerColor = Objects.ComponentTypeLayerColorFromObject(textObj, settings);
            Settings = Layers.GetComponentTypeSettings(textObj, settings);
        }

        public ComponentType(IComponent component)
        {
            TypeID = component.TypeID;
            TypeName = component.TypeName;
            LabelSize = component.LabelSize;
            LayerColor = component.LayerColor;
            Settings = component.Settings;
        }
    }
}
