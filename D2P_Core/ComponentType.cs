using D2P_Core.Interfaces;
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

        public string TypeID { get; set; }
        public string TypeName { get; set; }
        public double LabelSize { get; set; }
        public Color LayerColor { get; set; }
        public Settings Settings { get; set; }
    }
}
