using System.Drawing;

namespace D2P_Core.Interfaces
{
    public interface IComponentType
    {
        string TypeId { get; set; }
        string TypeName { get; set; }
        Color LayerColor { get; set; }
        double LabelSize { get; set; }
    }
}
