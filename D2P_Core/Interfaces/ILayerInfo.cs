using System.Drawing;

namespace D2P_Core.Interfaces
{
    public interface ILayerInfo
    {
        string RawLayerName { get; }
        Color LayerColor { get; }
    }
}
