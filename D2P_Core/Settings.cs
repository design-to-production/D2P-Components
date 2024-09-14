using Rhino;
using Rhino.DocObjects;
using System.Collections.Generic;
using System.Drawing;

namespace D2P_Core
{
    public class Settings
    {
        // Layerstructure
        public string RootLayerName { get; set; } = "D2P";
        public Color RootLayerColor { get; set; } = Color.FromArgb(220, 75, 58);

        // Style        
        public string DimensionStyleName { get; set; } = RhinoDoc.ActiveDoc.DimStyles.Current.Name;
        public DimensionStyle DimensionStyle => RhinoDoc.ActiveDoc.DimStyles.FindName(DimensionStyleName) ?? RhinoDoc.ActiveDoc.DimStyles.Current;

        // Delimiter
        public char TypeDelimiter { get; set; } = ':';
        public char LayerDelimiter { get; set; } = '_';
        public char NameDelimiter { get; set; } = '.';
        public char LayerDescriptionDelimiter { get; set; } = '-';
        public char LayerNameDelimiter { get; set; } = ':';
        public char CountDelimiter { get; set; } = '#';
        public char JointDelimiter { get; set; } = '+';

        public Dictionary<string, string> ToDictionary()
        {
            var settings = new Dictionary<string, string>();
            foreach (var propertyInfo in typeof(Settings).GetProperties())
            {
                var value = propertyInfo?.GetValue(this);
                settings.Add(propertyInfo?.Name, value?.ToString());
            }
            return settings;
        }
    }
}
