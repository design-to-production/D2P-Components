using D2P_Core.Components.Member;
using D2P_Core.Extensions;
using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace D2P_Core.Components
{
    public abstract class ComponentBase : MemberCollection, IComponentBase
    {
        public Guid ID { get; set; }
        public int GroupIndex { get; protected set; }
        public string Name => TypeId + Settings.TypeDelimiter + ShortName;
        public string ShortName
        {
            get => Label.Geometry.FirstOrDefault().PlainText;
            set => Label.Geometry.FirstOrDefault().PlainText = value;
        }
        public Plane Plane
        {
            get => Label.Geometry.FirstOrDefault().Plane;
            set => Label.Geometry.FirstOrDefault().Plane = value;
        }

        public abstract string TypeId { get; set; }
        public abstract string TypeName { get; set; }
        public abstract Color LayerColor { get; set; }
        public abstract double LabelSize { get; set; }

        public IEnumerable<GeometryBase> Geometry => AllMembers.SelectMany(m => m.Geometry);
        public IMember<TextEntity> Label { get; private set; }

        public abstract IComponentBase Duplicate();
        protected virtual void Init()
        {
            Label = new MemberGeo<TextEntity>(nameof(Label), this, new LayerInfo("", LayerColor));
        }

        public ComponentBase() { Init(); }
        public ComponentBase(string name, Plane plane) : this()
        {
            ShortName = name;
            Plane = plane;
        }
        protected ComponentBase(IComponentBase other) : this()
        {
            TypeId = other.TypeId;
            TypeName = other.TypeName;
            LayerColor = other.LayerColor;
            LabelSize = other.LabelSize;
            DynamicMembers = other.DynamicMembers.Duplicate();
        }

        public bool Transform(Transform xform)
        {
            var result = true;
            foreach (var geometry in Geometry)
            {
                if (!geometry.Transform(xform))
                    result = false;
            }
            return result;
        }

        public virtual bool Exists() => Settings.ActiveDoc.Objects.FindId(ID) != null;
        public virtual void Delete() => Objects.DeleteComponent(this);
        public virtual void Commit()
        {
            if (!Exists())
            {
                if (!Utility.Group.GetGroupIndex(this, out int grpIdx))
                    grpIdx = Utility.Group.AddGroup();
                GroupIndex = grpIdx;

                var componentLayer = Layers.FindComponentTypeRootLayer(this);
                if (componentLayer == null || componentLayer.Index == 0)
                    componentLayer = Layers.CreateComponentTypeLayer(this);

                var attributes = new ObjectAttributes() { Name = Name, LayerIndex = componentLayer.Index };
                attributes.RemoveFromAllGroups();
                attributes.AddToGroup(GroupIndex);

                var label = Label.Geometry.FirstOrDefault();
                ID = Settings.ActiveDoc.Objects.AddText(label, attributes);
            }

            foreach (var member in AllMembers)
            {
                member.Commit();
            }
        }
    }
}
