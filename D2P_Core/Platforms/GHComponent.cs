using D2P_Core.Components;
using D2P_Core.Interfaces;
using Rhino.Geometry;
using System.Drawing;
using System.Linq;


namespace D2P_Core.Platforms {
    public class GHComponent : ComponentBase {
        public override string TypeId { get; set; }
        public override string TypeName { get; set; }
        public override Color LayerColor { get; set; }
        public override double LabelSize { get; set; }

        public GHComponent() : base() { }
        protected GHComponent(IComponentBase other) : base(other) { }
        public GHComponent(IComponentType type, string name, Plane plane)
            : base(name, plane)
        {
            TypeId = type.TypeId;
            TypeName = type.TypeName;
            LayerColor = type.LayerColor;
            LabelSize = type.LabelSize;
            Label.Geometry.First().TextHeight = LabelSize;
        }

        public override IComponentBase Duplicate()
        {
            return new GHComponent(this);
        }
    }
}
