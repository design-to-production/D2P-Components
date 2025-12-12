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
        private readonly ILayerInfo _layerInfo;

        protected IEnumerable<T> _geometry;
        protected ObjectAttributes _attributes;

        public IComponentBase Component { get; set; }
        public IMember ParentMember { get; set; }
        public IEnumerable<IMember> Members
        {
            get => _members.Values.Concat(FindMembers());
        }

        public ILayerInfo LayerInfo { get => _layerInfo; }
        public IEnumerable<T> Geometry { get => GetGeometry(); }
        IEnumerable<GeometryBase> IMember.Geometry => Geometry;

        public T FirstGeometry => Geometry.FirstOrDefault();
        public ObjectAttributes Attributes { get => _attributes; set => _attributes = value; }


        public MemberGeo(IComponentBase component, string layerName, Color layerColor)
            : this(component, new LayerInfo(layerName, layerColor)) { }
        public MemberGeo(IComponentBase component, ILayerInfo layerInfo)
        {
            Component = component;
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
                value.ParentMember = this;
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
                    m.ParentMember = this;
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
            foreach (var childMember in Members)
            {
                childMember.Commit();
            }
        }
        private void UpdateDoc()
        {
            if (Component == null || !Component.Exists()) return;

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
        public MemberGeo(IComponentBase component, ILayerInfo layerInfo, IEnumerable<GeometryBase> geometry, ObjectAttributes attributes)
            : base(component, layerInfo)
        {
            _geometry = geometry;
            _attributes = attributes;
        }
    }
}
