using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;

namespace D2P_Core.Components.Member
{
    public class MemberGeo<T> : DynamicObject, IMember<T> where T : GeometryBase
    {
        private readonly Dictionary<string, object> _members = new Dictionary<string, object>();
        private readonly IComponentBase _component;
        private readonly ILayerInfo _layerInfo;

        protected IEnumerable<T> _geometry;
        protected ObjectAttributes _attributes;

        public IComponentBase Component { get => _component; }
        public IEnumerable<IMember> Children { get => _members.Values.Select(obj => (IMember)obj); }

        public ILayerInfo LayerInfo { get => _layerInfo; }
        public IEnumerable<T> Geometry { get => GetGeometry(); }
        public T FirstGeometry => Geometry.FirstOrDefault();
        public ObjectAttributes Attributes { get => _attributes; set => _attributes = value; }


        public MemberGeo(IComponentBase component, ILayerInfo layerInfo)
        {
            _component = component;
            _layerInfo = layerInfo;
        }
        public MemberGeo(IComponentBase component, string layerName, Color layerColor)
        {
            _component = component;
            _layerInfo = new LayerInfo(layerName, layerColor);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _members[binder.Name] = value;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _members.TryGetValue(binder.Name, out result);
        }

        public object this[string name]
        {
            get => _members.TryGetValue(name, out var v) ? v : null;
            set => _members[name] = value;
        }


        private IEnumerable<T> GetGeometry()
        {
            if (_geometry != null) return _geometry;
            var layer = Layers.FindLayer((IMember)this);
            return Objects.ObjectsByLayer<T>(Component, layer.Index);
        }
        public void SetGeometry(T geometry) => SetGeometry(new[] { geometry });
        public void SetGeometry(IEnumerable<T> geometry) => _geometry = geometry;
        public void SetGeometry(GeometryBase geometry) => SetGeometry(geometry);
        public void SetGeometry(IEnumerable<GeometryBase> geometry) => SetGeometry(geometry);

        public bool Exists()
        {
            throw new System.NotImplementedException();
        }
        public void Commit()
        {
            if (!Component.Exists())
                return;

            var attributes = Attributes;
            attributes.RemoveFromAllGroups();
            attributes.AddToGroup(Component.GroupIndex);

            var memberLayer = Layers.CreateLayer((IMember)this);
            attributes.LayerIndex = memberLayer.Index;

            Objects.DeleteObjects((IMember)this);
            var doc = Component.ActiveDoc;
            foreach (var geometry in Geometry)
            {
                doc.Objects.Add(geometry, attributes);
            }
        }
        public void Delete()
        {
            throw new System.NotImplementedException();
        }
    }

    public class MemberGeo : MemberGeo<GeometryBase>
    {
        public MemberGeo(IComponentBase component, ILayerInfo layerInfo, IEnumerable<GeometryBase> geometry, ObjectAttributes attributes = null)
            : base(component, layerInfo)
        {
            _geometry = geometry;
            Attributes = attributes;
        }
    }
}
