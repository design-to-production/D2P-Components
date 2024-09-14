using D2P_Core.Interfaces;
using System.Collections.Generic;
using System.Drawing;

namespace D2P_Core
{
    public class LayerInfo : ILayerInfo
    {
        public string RawLayerName { get; } = string.Empty;
        public Color LayerColor { get; } = Color.Black;

        public LayerInfo() { }
        public LayerInfo(string rawLayerName, Color layerColor)
        {
            RawLayerName = rawLayerName;
            LayerColor = layerColor;
        }
    }

    public class LayerInfoComparer : IComparer<ILayerInfo>
    {
        readonly char _layerNameDelimiter;
        public LayerInfoComparer(IComponent component)
        {
            _layerNameDelimiter = component.Settings.LayerNameDelimiter;
        }

        public int Compare(ILayerInfo x, ILayerInfo y)
        {
            if (x == y) return 0;
            if (y == null) return 1;
            var n1 = x.RawLayerName.Split(_layerNameDelimiter).Length;
            var n2 = y.RawLayerName.Split(_layerNameDelimiter).Length;
            return n1 > n2 ? 1 : -1;
        }
    }
}
