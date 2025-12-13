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
        protected ILayerInfo _layerInfo;
        protected IEnumerable<T> _geometry;
        protected ObjectAttributes _attributes;
        protected Dictionary<string, IMember> _members;

        public IComponentBase Component { get; set; }
        public IMember ParentMember { get; set; }
        public IEnumerable<IMember> Members
        {
            get => _members.Values.Concat(FindMembers());
            set => _members = value.ToDictionary(m => m.Name, m => m);
        }

        public string Name { get; set; }

        public ILayerInfo LayerInfo
        {
            get => _layerInfo;
            set => _layerInfo = value;
        }
        public IEnumerable<T> Geometry
        {
            get => GetGeometry();
            set => _geometry = value;
        }
        IEnumerable<GeometryBase> IMember.Geometry
        {
            get => GetGeometry();
            set => _geometry = value as IEnumerable<T>;
        }

        public ObjectAttributes Attributes
        {
            get => _attributes;
            set => _attributes = value;
        }


        public MemberGeo(string name, IComponentBase component, IMember parent, ILayerInfo layerInfo)
        {
            Name = name;
            Component = component;
            ParentMember = parent;
            _layerInfo = layerInfo;
            _attributes = new ObjectAttributes();
            _members = new Dictionary<string, IMember>();
        }
        public MemberGeo(string name, IComponentBase component, IMember parent, string layerName, Color layerColor)
            : this(name, component, parent, new LayerInfo(layerName, layerColor)) { }
        protected MemberGeo(IMember other)
        {
            Name = other.Name;
            Component = other.Component;
            ParentMember = other.ParentMember;
            LayerInfo = other.LayerInfo;
            Geometry = other.Geometry.Select(g => g.Duplicate()).OfType<T>();
            Attributes = other.Attributes.Duplicate();
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
                .OfType<IMember>();
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
            Attributes.Name = Component.Name;
            var memberLayer = Layers.CreateLayer(this);
            Attributes.LayerIndex = memberLayer.Index;

            if (_geometry == null) return;

            Objects.DeleteObjects(this);
            foreach (var geometry in Geometry)
            {
                var id = Settings.ActiveDoc.Objects.Add(geometry, Attributes);
                Attributes.ObjectId = id; // TODO: Refactoring ? Only needed for label right now
            }
        }
        public void Delete()
        {
            throw new System.NotImplementedException();
        }

        public object Clone() => new MemberGeo<T>(this);
    }

    public class MemberGeo : MemberGeo<GeometryBase>
    {
        public MemberGeo(string name, IComponentBase component, IMember parent, ILayerInfo layerInfo)
            : base(name, component, parent, layerInfo)
        { }
    }
}
