using D2P.Core.Interfaces;
using System.Collections.Generic;
using System.Drawing;

namespace D2P.Core.Components
{
    public class LayerInfo : ILayerInfo
    {
        public string RawLayerName { get; } = string.Empty;
        public Color LayerColor { get; } = Color.Black;

        public LayerInfo() { }
        public LayerInfo(string rawLayerName, Color layerColor)
        {
            RawLayerName = rawLayerName ?? string.Empty;
            LayerColor = layerColor;
        }
    }

    public class LayerInfoComparer : IComparer<ILayerInfo>
    {
        public int Compare(ILayerInfo x, ILayerInfo y)
        {
            if (x == y) return 0;
            if (y == null) return 1;
            var n1 = x.RawLayerName.Split(Settings.LayerNameDelimiter).Length;
            var n2 = y.RawLayerName.Split(Settings.LayerNameDelimiter).Length;
            return n1 > n2 ? 1 : -1;
        }
    }
}
