using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace D2P_Core
{
    using AttributeCollection = Dictionary<Guid, ObjectAttributes>;
    using GeometryCollection = Dictionary<Guid, GeometryBase>;
    using LayerCollection = Dictionary<ILayerInfo, Dictionary<Guid, int>>;

    public class Component : IComponent
    {

        GeometryCollection _geometryCollection = new GeometryCollection();
        AttributeCollection _attributeCollection = new AttributeCollection();
        LayerCollection _stagingLayerCollection = new LayerCollection();

        protected ComponentType _componentType;

        // Doc
        public RhinoDoc ActiveDoc { get; set; } = RhinoDoc.ActiveDoc;

        // IDs
        public Guid ID { get; set; }
        public int GroupIdx => Utility.Group.GetGroupIndex(ID, ActiveDoc);
        public string Name => TypeID + Settings.TypeDelimiter + ShortName;
        public string ShortName => Label.PlainText;

        // State
        public bool IsInitialized => ActiveDoc.Layers.FindName(Layers.ComposeComponentTypeLayerName(this)) != null;
        public bool IsVirtual => ActiveDoc.Objects.FindId(ID) == null;
        public bool IsVirtualClone { get; private set; }

        // Type
        public ComponentType ComponentType => _componentType;
        public string TypeID => _componentType.TypeID;
        public string TypeName => _componentType.TypeName;
        public Color LayerColor => _componentType.LayerColor;
        public Settings Settings => _componentType.Settings;

        // Geometry
        public double LabelSize => Label.TextHeight;
        public TextEntity Label
        {
            get
            {
                GeometryCollection.TryGetValue(ID, out var value);
                return value as TextEntity;
            }
        }
        public Plane Plane
        {
            get => Label.Plane;
            set => (GeometryCollection[ID] as TextEntity).Plane = value;

        }
        public GeometryCollection GeometryCollection
        {
            get => IsVirtualClone || IsVirtual || _geometryCollection.Any() ? _geometryCollection : RHObjects.ToDictionary(rh => rh.Id, rh => rh.Geometry);
            private set => _geometryCollection = value.ToDictionary(kv => kv.Key, kv => kv.Value.Duplicate());
        }
        public AttributeCollection AttributeCollection
        {
            get => IsVirtualClone || IsVirtual || _attributeCollection.Any() ? _attributeCollection : RHObjects.ToDictionary(rh => rh.Id, rh => rh.Attributes);
            private set => _attributeCollection = value.ToDictionary(kv => kv.Key, kv => kv.Value.Duplicate());
        }
        public LayerCollection StagingLayerCollection { get => _stagingLayerCollection; private set => _stagingLayerCollection = value; }
        public IEnumerable<RhinoObject> RHObjects => Objects.ObjectsByGroup(GroupIdx, ActiveDoc);
        public IEnumerable<GeometryBase> Geometry => GeometryCollection.Values;
        public IEnumerable<ObjectAttributes> Attributes => AttributeCollection.Values;

        public Component(ComponentType componentType, string name, Plane plane)
        {
            ID = Guid.NewGuid();
            _componentType = componentType;
            var label = TextEntity.Create(name, plane, Settings.DimensionStyle, false, 0, 0);
            label.TextHeight = componentType.LabelSize;
            GeometryCollection.Add(ID, label);
            StagingLayerCollection.Add(new LayerInfo(), new Dictionary<Guid, int>() { { ID, -1 } });
            AttributeCollection.Add(ID, new ObjectAttributes() { Name = Name, LayerIndex = 0 });
        }

        public Component(ComponentType componentType, Guid id)
        {
            ID = id;
            _componentType = componentType;
        }

        protected Component(IComponent component)
        {
            ID = component.ID;
            _componentType = new ComponentType(component);
            GeometryCollection = new GeometryCollection(component.GeometryCollection);
            var label = TextEntity.Create(ShortName, Plane, Settings.DimensionStyle, false, 0, 0);
            label.TextHeight = _componentType.LabelSize;
            GeometryCollection[ID] = label;
            AttributeCollection = new AttributeCollection(component.AttributeCollection);
            StagingLayerCollection = new LayerCollection(component.StagingLayerCollection);
            IsVirtualClone = component.IsVirtualClone;
        }

        public bool Transform(Transform xform) => Geometry.Select(geo => geo.Transform(xform)).All(r => r);
        public IComponent VirtualClone() => new Component(this) { IsVirtualClone = true };
        public IComponent Clone() => new Component(this);

        public Guid[] ReplaceGeometries(ComponentMember objects) => ReplaceGeometries(objects.LayerInfo, objects.GeometryBases, objects.ObjectAttributes);
        public Guid[] ReplaceGeometries(ILayerInfo layerInfo, IEnumerable<GeometryBase> geometries, ObjectAttributes objectAttributes = null)
        {
            if (!_geometryCollection.Any())
                CacheGeometry();
            if (!_attributeCollection.Any())
                CacheAttributes();

            var rawLayerName = layerInfo.RawLayerName;
            var layerIdx = Layers.FindLayerIndexByFullPath(this, rawLayerName);
            if (layerIdx <= 0)
            {
                layerIdx = Layers.FindLayerIndex(this, rawLayerName, out int layersFound);
                if (layersFound > 1)
                    return new Guid[0];
            }

            var ids = new List<Guid>();
            objectAttributes = objectAttributes ?? new ObjectAttributes();
            objectAttributes.Name = Name;

            if (!StagingLayerCollection.ContainsKey(layerInfo))
                StagingLayerCollection.Add(layerInfo, new Dictionary<Guid, int>());

            if (layerIdx > 0)
                objectAttributes.LayerIndex = layerIdx;

            foreach (var geometry in geometries)
            {
                if (geometry == null)
                    continue;
                var newID = Guid.NewGuid();
                if (layerIdx <= 0)
                    StagingLayerCollection[layerInfo][newID] = -1;
                GeometryCollection.Add(newID, geometry);
                AttributeCollection.Add(newID, objectAttributes);
            }

            return ids.ToArray();
        }
        public Guid ReplaceGeometry(GeometryBase geometry, ILayerInfo layerInfo, ObjectAttributes objectAttributes = null)
        {
            var geometries = new List<GeometryBase>() { geometry };
            return ReplaceGeometries(layerInfo, geometries, objectAttributes).FirstOrDefault();
        }

        void CacheGeometry() => _geometryCollection = GeometryCollection;
        void CacheAttributes() => _attributeCollection = AttributeCollection;
        public void ClearStagingLayerCollection()
        {
            foreach (var kv in StagingLayerCollection)
            {
                foreach (var objID in kv.Value.Keys.ToList())
                {
                    GeometryCollection.Remove(objID);
                    AttributeCollection.Remove(objID);
                }
            }
            StagingLayerCollection.Clear();
        }
    }
}