using D2P_Core.Components;
using System.Drawing;

namespace D2P_Core.Interfaces
{
    public interface IComponentType
    {
        Settings Settings { get; }

        string TypeId { get; }
        string TypeName { get; }
        Color LayerColor { get; }
        double LabelSize { get; }
    }
}
