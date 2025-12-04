using D2P_Core.Enums;
using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace D2P_Core.Components
{
    public class Member<T> : IMember<T> where T : GeometryBase
    {
        private readonly IComponent _component;
        private readonly ILayerInfo _layerInfo;
        protected IEnumerable<T> _geometry;
        protected ObjectAttributes _attributes;

        public ILayerInfo LayerInfo { get => _layerInfo; }
        public IEnumerable<T> Geometry { get => GetGeometry(); }
        public ObjectAttributes Attributes
        {
            get => _attributes;
            set => _attributes = value;
        }

        public Member(IComponent component, ILayerInfo layerInfo)
        {
            _component = component;
            _layerInfo = layerInfo;
        }
        public Member(IComponent component, string layerName, Color layerColor)
        {
            _component = component;
            _layerInfo = new LayerInfo(layerName, layerColor);
        }

        private IEnumerable<T> GetGeometry()
        {
            if (_geometry != null) return _geometry;
            var layer = Layers.FindLayer(_component, LayerInfo.RawLayerName, out int layersFound);
            return Objects.ObjectsByLayer(layer.Index, _component, LayerScope.CurrentOnly)
                .OfType<T>();
        }

        public void SetGeometry(T geometry) => SetGeometry(new[] { geometry });
        public void SetGeometry(IEnumerable<T> geometry)
        {
            _geometry = geometry;
            _component.ReplaceMember(this);
            // TODO: REFACTORING MEMBERSHIP & STAGINGCONCEPT (resp. caching) !!
        }

        //private Q GetEnum<Q>(string rawLayerName, LayerScope layerScope) where Q : struct, Enum
        //{
        //    var txtDot = GetGeometry<TextDot>(rawLayerName, layerScope).FirstOrDefault();
        //    if (txtDot == null) return default(T);
        //    Enum.TryParse(txtDot.Text, false, out T result);
        //    return result;
        //}

    }

    public class Member : Member<GeometryBase>
    {
        public Member(IComponent component, ILayerInfo layerInfo, IEnumerable<GeometryBase> geometry, ObjectAttributes attributes = null)
            : base(component, layerInfo)
        {
            _geometry = geometry;
            Attributes = attributes;
        }
    }
}
