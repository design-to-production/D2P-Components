using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace D2P_Core.Components.Member {
    public class MemberGeo<T> : MemberCollection, IMember<T> where T : GeometryBase {
        protected IEnumerable<T> _geometry;

        public bool ReplaceExisting { get; set; } = true;
        public IComponentBase Component { get; set; }

        public ILayerInfo LayerInfo { get; set; }
        public ObjectAttributes Attributes { get; set; } = new ObjectAttributes();
        public IEnumerable<T> Geometry {
            get {
                if (_geometry != null)
                    return _geometry;
                var layer = Layers.FindLayer(this);
                if (layer == null)
                    return Enumerable.Empty<T>();
                return Objects.GeometryByLayer<T>(Component, layer.Index);
            }
            set => _geometry = value;
        }
        IEnumerable<GeometryBase> IMember.Geometry { get => Geometry; }

        public MemberGeo(IComponentBase component, ILayerInfo layerInfo)
        {
            Component = component;
            LayerInfo = layerInfo;
        }
        public MemberGeo(IComponentBase component, string rawLayerName, Color layerColor)
            : this(component, new LayerInfo(rawLayerName, layerColor))
        { }
        protected MemberGeo(IMember other)
        {
            ParentMember = other.ParentMember;
            Component = other.Component;
            LayerInfo = other.LayerInfo;
            SetGeometry(
                other.Geometry
                .Select(g => g.Duplicate())
                .OfType<T>()
            );
            DynamicMembers = other.DynamicMembers.Select(m => m.Duplicate());
        }

        public void SetGeometry(T geometry) => SetGeometry(new[] { geometry });
        public void SetGeometry(IEnumerable<T> geometry) => _geometry = geometry;
        void IMember.SetGeometry(GeometryBase geometry) => SetGeometry(geometry as T);
        void IMember.SetGeometry(IEnumerable<GeometryBase> geometry) => SetGeometry(geometry as IEnumerable<T>);


        public void Commit()
        {
            if (Component == null || !Component.Exists())
                return;

            //if (Members.IsComponentLabel(this)) {
            //    var layer = Layers.FindLayer(this);
            //    var rhinoObjects = Objects.ObjectsByLayer(Component, layer.Index);
            //    var label = rhinoObjects.FirstOrDefault(rhObj => rhObj.Geometry.GetType() == typeof(TextEntity));
            //    var newLabel = Geometry.FirstOrDefault() as TextEntity;
            //    Settings.ActiveDoc.Objects.Replace(label.Id, newLabel);
            //    Attributes = label.Attributes;
            //    Component.ID = Attributes.ObjectId;
            //}

            UpdateDoc();

            foreach (var childMember in AllMembers) {
                childMember.ParentMember = this;
                childMember.Component = Component;
                childMember.Commit();
            }
        }
        private void UpdateDoc()
        {
            Attributes.RemoveFromAllGroups();
            Attributes.AddToGroup(Component.GroupIndex);
            Attributes.Name = Component.Name;
            var memberLayer = Layers.CreateLayer(this);
            Attributes.LayerIndex = memberLayer.Index;

            //if (_geometry == null) return;
            // TODO: Replace Members of existing component instead of recreating it completely
            //if (ReplaceExisting)

            Objects.DeleteObjects(this);
            foreach (var geometry in Geometry) {
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

        public IMember<T> Duplicate()
        {
            return new MemberGeo<T>(this);
        }

        IMember IDocObject<IMember>.Duplicate()
        {
            return Duplicate();
        }
    }

    public class MemberGeo : MemberGeo<GeometryBase> {
        public MemberGeo(IComponentBase component, ILayerInfo layerInfo)
            : base(component, layerInfo) { }
    }
}
