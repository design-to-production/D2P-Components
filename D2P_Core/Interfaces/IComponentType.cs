using System.Drawing;

namespace D2P_Core.Interfaces
{
    public interface IComponentType
    {
        string TypeId { get; }
        string TypeName { get; }
        Color LayerColor { get; }
        double LabelSize { get; }
    }
}
