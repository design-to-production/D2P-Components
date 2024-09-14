using System.Drawing;

namespace D2P_Core.Interfaces
{
    public interface IComponentType
    {
        Settings Settings { get; }
        string TypeID { get; }
        string TypeName { get; }
        double LabelSize { get; }
        Color LayerColor { get; }
    }
}
