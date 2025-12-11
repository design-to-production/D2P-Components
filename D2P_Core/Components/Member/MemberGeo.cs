using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace D2P_Core.Components.Member
{
    public class MemberGeo<T> : IMember<T> where T : GeometryBase
    {
        private readonly Dictionary<string, IMember> _members = new Dictionary<string, IMember>();

        private readonly IComponentBase _component;
        private readonly ILayerInfo _layerInfo;

        protected IEnumerable<T> _geometry;
        protected ObjectAttributes _attributes;

        public IComponentBase Component { get => _component; }
        public IMember Parent { get; set; }
        public IEnumerable<IMember> Children { get => _members.Values; }

        public ILayerInfo LayerInfo { get => _layerInfo; }
        public IEnumerable<T> Geometry { get => GetGeometry(); }
        IEnumerable<GeometryBase> IMember.Geometry => Geometry;

        public T FirstGeometry => Geometry.FirstOrDefault();
        public ObjectAttributes Attributes { get => _attributes; set => _attributes = value; }


        public MemberGeo(IComponentBase component, string layerName, Color layerColor)
            : this(component, new LayerInfo(layerName, layerColor)) { }
        public MemberGeo(IComponentBase component, ILayerInfo layerInfo)
        {
            _component = component;
            _layerInfo = layerInfo;
            _attributes = new ObjectAttributes();
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
                value.Parent = this;
                _members[name] = value;
            }
        }

        public IEnumerable<IMember> Members
        {
            get => GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p =>
                    p.CanRead &&
                    p.GetIndexParameters().Length == 0 &&
                    typeof(IMember).IsAssignableFrom(p.PropertyType) &&
                    p.Name != nameof(Parent) &&
                    p.Name != nameof(Children))
                .Select(p => p.GetValue(this))
                .OfType<IMember>().Select(m =>
                {
                    m.Parent = this;
                    return m;
                });
        }


        private IEnumerable<T> GetGeometry()
        {
            if (_geometry != null) return _geometry;
            var layer = Layers.FindLayer(this);
            return Objects.ObjectsByLayer<T>(Component, layer.Index);
        }
        public void SetGeometry(T geometry) => SetGeometry(new[] { geometry });
        public void SetGeometry(IEnumerable<T> geometry) => _geometry = geometry;
        void IMember.SetGeometry(GeometryBase geometry) => SetGeometry(geometry as T);
        void IMember.SetGeometry(IEnumerable<GeometryBase> geometry) => SetGeometry(geometry as IEnumerable<T>);

        public bool Exists()
        {
            throw new System.NotImplementedException();
        }
        public void Commit()
        {
            UpdateDoc();
            var members = Children.Concat(Members);
            foreach (var childMember in members)
            {
                childMember.Commit();
            }
        }
        private void UpdateDoc()
        {
            if (!Component.Exists()) return;

            Attributes.RemoveFromAllGroups();
            Attributes.AddToGroup(Component.GroupIndex);
            var memberLayer = Layers.CreateLayer(this);
            Attributes.LayerIndex = memberLayer.Index;

            if (_geometry == null) return;
            Objects.DeleteObjects(this);
            foreach (var geometry in Geometry)
            {
                Settings.ActiveDoc.Objects.Add(geometry, Attributes);
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
