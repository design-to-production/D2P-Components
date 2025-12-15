using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace D2P_Core.Components.Member
{
    public class MemberGeo<T> : MemberCollection, IMember<T> where T : GeometryBase
    {
        protected IEnumerable<T> _geometry;

        public string Name { get; set; }
        public IComponentBase Component { get; set; }

        public ILayerInfo LayerInfo { get; set; }
        public ObjectAttributes Attributes { get; set; } = new ObjectAttributes();
        public IEnumerable<T> Geometry
        {
            get
            {
                if (_geometry != null)
                    return _geometry;
                var layer = Layers.FindLayer(this);
                if (layer == null)
                    return Enumerable.Empty<T>();
                return Objects.ObjectsByLayer<T>(Component, layer.Index);
            }
            set => _geometry = value;
        }
        IEnumerable<GeometryBase> IMember.Geometry { get => Geometry; }

        public MemberGeo(string name, IComponentBase component, ILayerInfo layerInfo)
        {
            Name = name;
            Component = component;
            LayerInfo = layerInfo;
        }
        public MemberGeo(string name, IComponentBase component, string layerName, Color layerColor)
            : this(name, component, new LayerInfo(layerName, layerColor)) { }
        protected MemberGeo(IMember other, IComponentBase newComponent)
        {
            Name = other.Name;
            ParentMember = other.ParentMember;
            LayerInfo = other.LayerInfo;
            SetGeometry(
                other.Geometry
                .Select(g => g.Duplicate())
                .OfType<T>()
            );
            Attributes = other.Attributes.Duplicate();
            DynamicMembers = other.DynamicMembers.Select(m => m.Duplicate());
            Component = newComponent;  // TODO: Anti pattern ? 
        }

        public void SetGeometry(T geometry) => SetGeometry(new[] { geometry });
        public void SetGeometry(IEnumerable<T> geometry) => _geometry = geometry;
        void IMember.SetGeometry(GeometryBase geometry) => SetGeometry(geometry as T);
        void IMember.SetGeometry(IEnumerable<GeometryBase> geometry) => SetGeometry(geometry as IEnumerable<T>);


        public void Commit()
        {
            UpdateDoc();
            foreach (var childMember in AllMembers)
            {
                childMember.Commit();
            }
        }
        private void UpdateDoc()
        {
            if (Component == null || !Component.Exists()) return;

            Attributes.RemoveFromAllGroups();
            Attributes.AddToGroup(Component.GroupIndex);
            Attributes.Name = Component.Name;
            var memberLayer = Layers.CreateLayer(this);
            Attributes.LayerIndex = memberLayer.Index;

            //if (_geometry == null) return;
            // TODO: Replace Members of existing component instead of recreating it completely

            Objects.DeleteObjects(this);
            foreach (var geometry in Geometry)
            {
                var id = Settings.ActiveDoc.Objects.Add(geometry, Attributes);
                Attributes.ObjectId = id; // TODO: Refactoring ? Only needed for label right now
            }
        }

        public bool Exists()
        {
            throw new System.NotImplementedException();
        }
        public void Delete()
        {
            throw new System.NotImplementedException();
        }
        public IMember Duplicate()
        {
            return new MemberGeo<T>(this, Component);
        }
    }

    public class MemberGeo : MemberGeo<GeometryBase>
    {
        public MemberGeo(string name, IComponentBase component, ILayerInfo layerInfo)
            : base(name, component, layerInfo)
        { }
    }
}
