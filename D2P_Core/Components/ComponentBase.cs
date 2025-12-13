using D2P_Core.Components.Member;
using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace D2P_Core.Components
{
    public abstract class ComponentBase : IComponentBase
    {
        protected Dictionary<string, IMember> _members = new Dictionary<string, IMember>();

        public IMember<TextEntity> Label { get; set; }

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

        public IMember ParentMember { get; set; }
        public IEnumerable<IMember> Members
        {
            get => _members.Values.Concat(FindMembers());
            set => _members = value.ToDictionary(m => m.Name, m => m);
        }
        public IEnumerable<GeometryBase> Geometry
        {
            get => Members.SelectMany(m => m.Geometry);
        }

        public abstract object Clone();
        protected virtual void Init()
        {
            Label = new MemberGeo<TextEntity>(nameof(Label), this, null, new LayerInfo("", LayerColor));
            var label = TextEntity.Create("", Plane.WorldXY, Settings.DimensionStyle, false, 0, 0);
            label.TextHeight = LabelSize;
            Label.SetGeometry(label);
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
            Label = other.Label.Clone() as IMember<TextEntity>;
            Members = other.Members.Select(m => m.Clone() as IMember);
        }

        public IMember this[string name]
        {
            get
            {
                _members.TryGetValue(name, out var v);
                return v ?? null;
            }
            set
            {
                if (value == null) return;
                _members[name] = value;
            }
        }
        private IEnumerable<IMember> FindMembers()
        {
            return GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p =>
                    p.CanRead &&
                    p.GetIndexParameters().Length == 0 &&
                    typeof(IMember).IsAssignableFrom(p.PropertyType) &&
                    p.Name != nameof(ParentMember) &&
                    p.Name != nameof(Members))
                .Select(p => p.GetValue(this))
                .OfType<IMember>();
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

        //public abstract IComponentBase ParentMember { get; }
        //public abstract IEnumerable<IComponentBase> ChildMembers { get; }

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

            foreach (var member in Members)
            {
                if (member.Name == nameof(Label)) continue;
                member.Commit();
            }
        }
    }
}
