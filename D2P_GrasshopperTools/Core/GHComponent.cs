using D2P_Core.Interfaces;
using Rhino.Geometry;
using System.Drawing;


namespace D2P_Core.Components.Grasshopper
{
    public class GHComponent : ComponentBase
    {
        public override string TypeId { get; }
        public override string TypeName { get; }
        public override Color LayerColor { get; }
        public override double LabelSize { get; }

        protected override void Init()
        {
            throw new System.NotImplementedException();
        }

        public GHComponent() : base() { }
        public GHComponent(IComponentType type, string name, Plane plane)
            : base(name, plane)
        {
            TypeId = type.TypeId;
            TypeName = type.TypeName;
            LayerColor = type.LayerColor;
            LabelSize = type.LabelSize;
        }
    }
}
