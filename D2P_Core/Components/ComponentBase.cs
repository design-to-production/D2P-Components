using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace D2P_Core.Components
{
    public abstract class ComponentBase : IComponentBase
    {
        static RhinoDoc _doc = RhinoDoc.ActiveDoc;
        public RhinoDoc ActiveDoc { get => _doc; protected set => _doc = value; }
        public Settings Settings { get; } = Settings.Default;

        public Guid ID { get; set; }
        public int GroupIndex { get; protected set; }
        public string ShortName { get; protected set; }
        public string Name => TypeId + Settings.TypeDelimiter + ShortName;
        public Plane Plane { get; }

        public abstract string TypeId { get; }
        public abstract string TypeName { get; }
        public abstract Color LayerColor { get; }
        public abstract double LabelSize { get; }

        public ComponentBase() { Init(); }
        public ComponentBase(string name, Plane plane) : this()
        {
            ShortName = name;
            Plane = plane;
        }

        protected abstract void Init();

        //public abstract IComponentBase Parent { get; }
        //public abstract IEnumerable<IComponentBase> Children { get; }
        public IEnumerable<IMember> Members
        {
            get => GetType()
                .GetProperties()
                .Where(p => typeof(IMember).IsAssignableFrom(p.PropertyType))
                .Select(p => p.GetValue(this))
                .OfType<IMember>();
        }

        public virtual bool Exists() => ActiveDoc.Objects.FindId(ID) != null;
        public virtual void Delete() => Objects.DeleteComponent(this);
        public virtual void Commit()
        {
            if (!Exists())
                Create();

            foreach (var member in Members)
            {
                member.Commit();
            }
        }

        private void Create()
        {
            if (!Utility.Group.GetGroupIndex(this, out int grpIdx))
                grpIdx = Utility.Group.AddGroup(ActiveDoc);
            GroupIndex = grpIdx;

            var componentLayer = Layers.GetComponentTypeRootLayer(this);
            if (componentLayer == null || componentLayer.Index == 0)
                componentLayer = Layers.CreateComponentTypeLayer(this);

            var label = TextEntity.Create(ShortName, Plane, Settings.DimensionStyle, false, 0, 0);
            label.TextHeight = LabelSize;
            var attributes = new ObjectAttributes() { Name = Name, LayerIndex = componentLayer.Index };
            attributes.RemoveFromAllGroups();
            attributes.AddToGroup(GroupIndex);

            ID = ActiveDoc.Objects.AddText(label, attributes);
        }
    }
}
