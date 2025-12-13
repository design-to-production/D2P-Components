using D2P_Core.Components;
using D2P_Core.Interfaces;
using Rhino.Geometry;
using System.Drawing;


namespace D2P_GrasshopperTools.Core
{
    public class GHComponent : ComponentBase
    {
        public override string TypeId { get; set; }
        public override string TypeName { get; set; }
        public override Color LayerColor { get; set; }
        public override double LabelSize { get; set; }

        protected override void Init() { base.Init(); }
        public override object Clone() => new GHComponent(this);

        private GHComponent(IComponentBase other) : base(other) { }
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
