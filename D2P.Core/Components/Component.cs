using D2P.Core.Interfaces;
using System;
using System.Drawing;

namespace D2P.Core.Components {
    internal class Component : ComponentBase {
        public override string TypeId { get; set; }
        public override string TypeName { get; set; }
        public override Color LayerColor { get; set; }
        public override double LabelSize { get; set; }

        public Component() : base() { }

        public override IComponentBase Duplicate()
        {
            throw new NotImplementedException();
        }
    }
}
