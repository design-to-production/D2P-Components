using Rhino;
using Rhino.DocObjects;
using System.Drawing;


namespace D2P.Core.Components {
    public static class Settings {
        // Docs
        public static RhinoDoc ActiveDoc { get; set; } = RhinoDoc.ActiveDoc;

        // Layer structure        
        public static string RootLayerName { get; set; } = "D2P";
        public static Color RootLayerColor { get; set; } = Color.FromArgb(220, 75, 58);

        // Style        
        public static string DimensionStyleName => ActiveDoc.DimStyles.Current.Name;
        public static DimensionStyle DimensionStyle => ActiveDoc.DimStyles.FindName(DimensionStyleName) ?? ActiveDoc.DimStyles.Current;

        // Tolerance
        public static double Tolerance => ActiveDoc.ModelAbsoluteTolerance;
        public static double AngleTolerance => ActiveDoc.ModelAngleToleranceDegrees;

        // Delimiter
        public static char TypeDelimiter { get; set; } = ':';
        public static char LayerDelimiter { get; set; } = '_';
        public static char NameDelimiter { get; set; } = '.';
        public static char LayerDescriptionDelimiter { get; set; } = '-';
        public static char LayerNameDelimiter { get; set; } = ':';
        public static char CountDelimiter { get; set; } = '#';
        public static char JointDelimiter { get; set; } = 'x';
    }
}
