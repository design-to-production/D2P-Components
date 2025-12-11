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
        public Guid ID { get; set; }
        public int GroupIndex { get; protected set; }
        public string ShortName { get; protected set; }
        public string Name => TypeId + Settings.TypeDelimiter + ShortName;
        public Plane Plane { get; }

        public abstract string TypeId { get; }
        public abstract string TypeName { get; }
        public abstract Color LayerColor { get; }
        public abstract double LabelSize { get; }

        protected abstract void Init();

        public ComponentBase() { Init(); }
        public ComponentBase(string name, Plane plane) : this()
        {
            ShortName = name;
            Plane = plane;
        }

        //public abstract IComponentBase Parent { get; }
        //public abstract IEnumerable<IComponentBase> Children { get; }
        public IEnumerable<IMember> Members
        {
            get => GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => typeof(IMember).IsAssignableFrom(p.PropertyType))
                .Select(p => p.GetValue(this))
                .OfType<IMember>();
        }

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

            var componentLayer = Layers.GetComponentTypeRootLayer(this);
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
