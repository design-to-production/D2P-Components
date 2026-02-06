using D2P.Core.Components.Primitives;
using D2P.Core.Extensions;
using D2P.Core.Interfaces;
using D2P.Core.Utility;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace D2P.Core.Components.Member {
    public class MemberGeo<T> : MemberGeo, IMember<T> where T : GeometryBase {
        public new IEnumerable<T> Geometry => BaseObjects
            .Where(o => o.Geometry is T)
            .Select(o => (T)o.Geometry);

        public new IEnumerable<IBaseObject<T>> BaseObjects {
            get {
                var baseObjects = base.BaseObjects;
                var typedObjects = baseObjects.OfType<IBaseObject<T>>();
                var untypedObjects = baseObjects
                     .Where(o => o.Geometry is T && !(o is IBaseObject<T>))
                     .Select(o => new BaseObject<T>((T)o.Geometry, o.Attributes));
                return typedObjects.Concat(untypedObjects);
            }
            set => base.BaseObjects = value.Cast<IBaseObject>();
        }

        public MemberGeo(IComponentBase component, ILayerInfo layerInfo) : base(component, layerInfo) { }
        public MemberGeo(IComponentBase component, string rawLayerName, Color layerColor) : base(component, rawLayerName, layerColor) { }
        protected MemberGeo(IMember<T> other) : base(other) { }

        void IMember<T>.SetObject(IBaseObject<T> baseObject) => base.SetObject(baseObject);
        void IMember<T>.SetObject(T geometry) => base.SetObject(geometry);
        void IMember<T>.SetObjects(IEnumerable<IBaseObject<T>> baseObjects) => base.SetObjects(baseObjects.Cast<IBaseObject>());
        void IMember<T>.SetObjects(IEnumerable<T> geometries) => base.SetObjects(geometries.Cast<GeometryBase>());

        public new IMember<T> Duplicate() => new MemberGeo<T>(this);
    }

    public class MemberGeo : MemberCollection, IMember {
        protected IEnumerable<IBaseObject> _objects;

        public IComponentBase Component { get; set; }

        public ILayerInfo LayerInfo { get; set; }
        public IEnumerable<ObjectAttributes> Attributes => BaseObjects.Select(o => o.Attributes);
        public IEnumerable<GeometryBase> Geometry => BaseObjects.Select(o => o.Geometry);

        public IEnumerable<IBaseObject> BaseObjects {
            get {
                if (_objects != null) return _objects;
                var layer = Layers.FindLayer(this);
                if (layer == null)
                    return _objects = Enumerable.Empty<IBaseObject>();
                return _objects = Objects.ObjectsByLayer(Component, layer.Index)
                   .Select(obj => new BaseObject(obj))
                   .ToList();
            }
            set => _objects = value;
        }
        public MemberGeo(IComponentBase component, ILayerInfo layerInfo)
        {
            Component = component;
            LayerInfo = layerInfo;
        }
        public MemberGeo(IComponentBase component, string rawLayerName, Color layerColor)
            : this(component, new LayerInfo(rawLayerName, layerColor)) { }
        protected MemberGeo(IMember other)
        {
            ParentMember = other.ParentMember;
            Component = other.Component;
            LayerInfo = other.LayerInfo;
            SetObjects(other.BaseObjects.Select(baseObj => baseObj.Duplicate()).ToList());
            DynamicMembers = other.DynamicMembers.Duplicate();
        }

        public void SetObject(IBaseObject obj) => _objects = new[] { obj };
        public void SetObjects(IEnumerable<IBaseObject> objects) => _objects = objects;
        public void SetObject(GeometryBase geometry) => _objects = new[] { new BaseObject(geometry) };
        public void SetObjects(IEnumerable<GeometryBase> geometry) => _objects = geometry.Select(g => new BaseObject(g)).ToList();
        void IMember.SetObject(IBaseObject baseObject) => _objects = new[] { baseObject };
        void IMember.SetObjects(IEnumerable<IBaseObject> baseObjects) => _objects = baseObjects;


        public override void SetMember(IMember member)
        {
            member.ParentMember = this;
            base.SetMember(member);
        }

        public void Commit()
        {
            if (Component == null || !Component.Exists())
                return;

            UpdateDoc();

            foreach (var childMember in AllMembers) {
                childMember.ParentMember = this;
                childMember.Component = Component;
                childMember.Commit();
            }
        }
        private void UpdateDoc()
        {
            var memberLayer = Layers.CreateLayer(this);

            if (_objects == null) return;
            else Delete();

            foreach (var obj in BaseObjects) {
                obj.Attributes.RemoveFromAllGroups();
                obj.Attributes.AddToGroup(Component.GroupIndex);
                obj.Attributes.Name = Component.Name;
                obj.Attributes.LayerIndex = memberLayer.Index;

                var id = Settings.ActiveDoc.Objects.Add(obj.Geometry, obj.Attributes);
                obj.Attributes.ObjectId = id;
            }
        }

        public bool Exists() { return Geometry.Any(); }
        public void Delete()
        {
            var layer = Layers.FindLayer(this);
            Objects.DeleteObjects(Component, layer, true);
            foreach (var member in AllMembers) {
                member.Delete();
            }
        }

        public IMember Duplicate()
        {
            return new MemberGeo(this);
        }


    }
}
