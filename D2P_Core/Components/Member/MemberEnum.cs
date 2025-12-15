using D2P_Core.Interfaces;
using Rhino.Geometry;
using System;
using System.Drawing;
using System.Linq;

namespace D2P_Core.Components.Member
{
    public class MemberEnum<Q> : MemberGeo<TextDot> where Q : struct, Enum
    {
        public Q EnumValue { get => GetEnum(); set => SetEnum(value); }

        public MemberEnum(string name, IComponentBase component, IMember parent, string layerName, Color layerColor)
            : base(name, component, layerName, layerColor) { }

        private Q GetEnum()
        {
            var txtDot = Geometry.FirstOrDefault();
            if (txtDot == null) return default(Q);
            Enum.TryParse(txtDot.Text, false, out Q result);
            return result;
        }

        public void SetEnum(Q value)
        {
            SetGeometry(new TextDot(value.ToString(), Component.Plane.Origin));
        }
    }
}
