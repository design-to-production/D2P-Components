using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Rhino;
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

        public ComponentBase() { }
        public ComponentBase(string name, Plane plane)
        {
            ShortName = name;
            Plane = plane;
        }

        //public abstract IComponentBase Parent { get; }
        //public abstract IEnumerable<IComponentBase> Children { get; }
        public IEnumerable<IMember> Members
        {
            get => GetType()
                .GetProperties(BindingFlags.Default)
                .OfType<IMember>();
        }

        public virtual bool Exists() => ActiveDoc.Objects.FindId(ID) != null;
        public virtual void Delete() => Objects.DeleteComponent(this);
        public virtual void Commit()
        {
            if (!Exists())
            {
                var label = TextEntity.Create(Name, Plane, Settings.DimensionStyle, false, 0, 0);
                label.TextHeight = LabelSize;
                ID = ActiveDoc.Objects.Add(label);
            }

            if (!Group.GetGroupIndex(this, out int grpIdx))
                grpIdx = Group.AddGroup(ActiveDoc);
            GroupIndex = grpIdx;

            foreach (var member in Members)
            {
                member.Commit();
            }
        }
    }
}
