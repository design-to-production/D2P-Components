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

        public Guid ID { get; set; }
        public int GroupIndex { get; protected set; }
        public string ShortName { get; protected set; }
        public string Name => TypeId + Settings.TypeDelimiter + ShortName;
        public Plane Plane { get; }

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

        protected abstract void Init();
        public abstract object Clone();

        public ComponentBase() { Init(); }
        public ComponentBase(string name, Plane plane) : this()
        {
            ShortName = name;
            Plane = plane;
        }
        protected ComponentBase(IComponentBase other)
        {
            TypeId = other.TypeId;
            TypeName = other.TypeName;
            LayerColor = other.LayerColor;
            LabelSize = other.LabelSize;
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
                .OfType<IMember>()
                .Select(m =>
                {
                    return m;
                });
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
            if (!Exists()) Create();
            foreach (var member in Members)
                member.Commit();
        }
        void Create()
        {
            if (!Utility.Group.GetGroupIndex(this, out int grpIdx))
                grpIdx = Utility.Group.AddGroup();
            GroupIndex = grpIdx;

            var componentLayer = Layers.FindComponentTypeRootLayer(this);
            if (componentLayer == null || componentLayer.Index == 0)
                componentLayer = Layers.CreateComponentTypeLayer(this);

            var label = TextEntity.Create(ShortName, Plane, Settings.DimensionStyle, false, 0, 0);
            label.TextHeight = LabelSize;
            var attributes = new ObjectAttributes() { Name = Name, LayerIndex = componentLayer.Index };
            attributes.RemoveFromAllGroups();
            attributes.AddToGroup(GroupIndex);

            ID = Settings.ActiveDoc.Objects.AddText(label, attributes);
        }
    }
}
