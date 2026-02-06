using D2P.Core.Components.Member;
using D2P.Core.Extensions;
using D2P.Core.Interfaces;
using D2P.Core.Utility;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace D2P.Core.Components {
    public abstract class ComponentBase : MemberCollection, IComponentBase {
        public Guid ID { get; set; } = Guid.Empty;
        public int GroupIndex { get; set; }
        public string Name => TypeId + Settings.TypeDelimiter + ShortName;
        public string ShortName {
            get => Label.Geometry.FirstOrDefault().PlainText;
            set => Label.Geometry.FirstOrDefault().PlainText = value;
        }
        public Plane Plane {
            get => Label.Geometry.FirstOrDefault().Plane;
            set => Label.Geometry.FirstOrDefault().Plane = value;
        }

        public abstract string TypeId { get; set; }
        public abstract string TypeName { get; set; }
        public abstract Color LayerColor { get; set; }
        public abstract double LabelSize { get; set; }

        public IEnumerable<GeometryBase> Geometry => AllMembers.SelectMany(m => m.Geometry);
        public IMember<TextEntity> Label { get; private set; }

        protected virtual void Init()
        {
            Label = new MemberGeo<TextEntity>(this, "", LayerColor);
        }
        public abstract IComponentBase Duplicate();

        public ComponentBase()
        {
            Init();
            var label = TextEntity.Create("", Plane.WorldXY, Settings.DimensionStyle, false, 0, 0);
            label.TextHeight = LabelSize;
            Label.SetObject(label);
        }
        public ComponentBase(string name, Plane plane) : this()
        {
            Label.Geometry.First().PlainText = name;
            Label.Geometry.First().Plane = plane;
        }
        protected ComponentBase(IComponentBase other) : this()
        {
            TypeId = other.TypeId;
            TypeName = other.TypeName;
            LayerColor = other.LayerColor;
            LabelSize = other.LabelSize;
            Label = other.Label.Duplicate();
            DynamicMembers = other.DynamicMembers.Duplicate();
        }
        public bool Transform(Transform xform)
        {
            var result = true;
            foreach (var geometry in Geometry) {
                if (!geometry.Transform(xform))
                    result = false;
            }
            return result;
        }

        public virtual bool Exists() => Settings.ActiveDoc.Objects.FindId(ID) != null;
        public virtual void Delete() => Objects.DeleteComponent(this);
        public virtual void Commit()
        {
            var existing = Instantiation.InstancesByName(Name);
            Objects.DeleteComponents(existing.Where(c => c.ID != ID));

            if (!Exists()) {
                create();
            }

            AllMembers.SetComponent(this);
            foreach (var member in AllMembers.Where(m => !Members.IsComponentLabel(this, m))) {
                member.Commit();
            }
        }
        void create()
        {
            if (!Utility.Group.GetGroupIndex(this, out int grpIdx))
                grpIdx = Utility.Group.AddGroup();
            GroupIndex = grpIdx;

            var componentLayer = Layers.FindComponentTypeRootLayer(this);
            if (componentLayer == null || componentLayer.Index == 0)
                componentLayer = Layers.CreateComponentTypeLayer(this);

            var attributes = new ObjectAttributes() { Name = Name, LayerIndex = componentLayer.Index };
            attributes.AddToGroup(GroupIndex);

            var label = Label.Geometry.FirstOrDefault();
            ID = Settings.ActiveDoc.Objects.AddText(label, attributes);
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            var other = obj as IComponentBase;
            if (other != null)
                return this.ShortName.CompareTo(other.ShortName);
            else throw new ArgumentException("Object is not an IComponentBase");
        }
    }
}
