using D2P_Core.Components;
using D2P_Core.Interfaces;
using Rhino.Geometry;
using System.Drawing;


namespace D2P_GrasshopperTools.Core
{
    public class GHComponent : ComponentBase
    {
        public override string TypeId { get; }
        public override string TypeName { get; }
        public override Color LayerColor { get; }
        public override double LabelSize { get; }

        protected override void Init() { }
        public override object Clone()
        {
            return new GHComponent(this);
        }

        private GHComponent(IComponentBase component)
        {
            TypeId = component.TypeId;
            TypeName = component.TypeName;
            LayerColor = component.LayerColor;
            LabelSize = component.LabelSize;



            Members = component.Members.Clone();
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
