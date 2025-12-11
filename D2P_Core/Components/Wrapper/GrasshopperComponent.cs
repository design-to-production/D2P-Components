using D2P_Core.Components.Member;
using D2P_Core.Enums;
using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;


namespace D2P_Core.Components.Grasshopper
{
    using AttributeCollection = Dictionary<Guid, ObjectAttributes>;
    using GeometryCollection = Dictionary<Guid, GeometryBase>;
    using LayerCollection = Dictionary<ILayerInfo, Dictionary<Guid, int>>;

    public class GrasshopperComponent : IComponentBase
    {
        #region Properties
        GeometryCollection _geometryCollection = new GeometryCollection();
        AttributeCollection _attributeCollection = new AttributeCollection();
        LayerCollection _stagingLayerCollection = new LayerCollection();

        protected ComponentType _componentType;

        // Doc
        public RhinoDoc ActiveDoc { get; set; } = RhinoDoc.ActiveDoc;

        // IDs
        public Guid ID { get; set; }
        public int GroupIndex { get { Utility.Group.GetGroupIndex(this, out int grpIdx); return grpIdx; } }
        public string Name => TypeId + Settings.TypeDelimiter + ShortName;
        public string ShortName => Label.PlainText;

        // State
        public bool IsInitialized => ActiveDoc.Layers.FindName(Layers.ComposeComponentTypeLayerName(this)) != null;
        public bool IsVirtual => ActiveDoc.Objects.FindId(ID) == null;
        public bool IsVirtualClone { get; protected set; }

        // Type
        public IComponentType ComponentType => _componentType;
        public string TypeId => _componentType.TypeId;
        public string TypeName => _componentType.TypeName;
        public Color LayerColor => _componentType.LayerColor;

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
            protected set => _geometryCollection = value.ToDictionary(kv => kv.Key, kv => kv.Value.Duplicate());
        }
        public AttributeCollection AttributeCollection
        {
            get => IsVirtualClone || IsVirtual || _attributeCollection.Any() ? _attributeCollection : RHObjects.ToDictionary(rh => rh.Id, rh => rh.Attributes);
            protected set => _attributeCollection = value.ToDictionary(kv => kv.Key, kv => kv.Value.Duplicate());
        }
        public LayerCollection StagingLayerCollection
        {
            get => _stagingLayerCollection;
            protected set => _stagingLayerCollection = value;
        }
        public IEnumerable<RhinoObject> RHObjects => Objects.ObjectsByGroup(GroupIndex);
        public IEnumerable<GeometryBase> Geometry => GeometryCollection.Values;
        public IEnumerable<ObjectAttributes> Attributes => AttributeCollection.Values;

        public IEnumerable<IMember> Members => throw new NotImplementedException();
        #endregion

        #region Constructors
        public GrasshopperComponent() { }
        public GrasshopperComponent(ComponentType componentType, string name, Plane plane)
        {
            ID = Guid.NewGuid();
            _componentType = componentType;
            var label = TextEntity.Create(name, plane, Settings.DimensionStyle, false, 0, 0);
            label.TextHeight = componentType.LabelSize;
            GeometryCollection.Add(ID, label);
            StagingLayerCollection.Add(new LayerInfo(), new Dictionary<Guid, int>() { { ID, -1 } });
            AttributeCollection.Add(ID, new ObjectAttributes() { Name = Name, LayerIndex = 0 });
        }
        //public GrasshopperComponent(IComponent component)
        //{
        //    ID = component.ID;
        //    _componentType = component.ComponentType.Clone();
        //    GeometryCollection = new GeometryCollection(component.GeometryCollection);
        //    var label = TextEntity.Create(ShortName, Plane, Settings.DimensionStyle, false, 0, 0);
        //    label.TextHeight = _componentType.LabelSize;
        //    GeometryCollection[ID] = label;
        //    AttributeCollection = new AttributeCollection(component.AttributeCollection);
        //    StagingLayerCollection = new LayerCollection(component.StagingLayerCollection);
        //    IsVirtualClone = component.IsVirtualClone;
        //}

        public virtual void Init(RhinoObject obj)
        {
            ID = obj.Id;
            _componentType = Objects.ComponentTypeFromObject(obj);
        }

        #endregion

        public bool Transform(Transform _) => Geometry.Select(geo => geo.Transform(_)).All(r => r);
        public virtual IComponentBase Clone(bool isVirtual) => new GrasshopperComponent() { IsVirtualClone = isVirtual };

        #region Members
        public IEnumerable<T> GetGeometry<T>(string rawLayerName, LayerScope layerScope) where T : GeometryBase
        {
            var layer = Layers.FindLayer(this, rawLayerName, out int layersFound);
            return Objects.ObjectsByLayer(layer.Index, this, layerScope)
                .OfType<T>();
        }
        public T GetEnum<T>(string rawLayerName, LayerScope layerScope) where T : struct, Enum
        {
            var txtDot = GetGeometry<TextDot>(rawLayerName, layerScope).FirstOrDefault();
            if (txtDot == null) return default;
            Enum.TryParse(txtDot.Text, false, out T result);
            return result;
        }

        public IList<Guid> AddMember<T>(IMember<T> member) where T : GeometryBase
        {
            if (member == null) return new List<Guid>();

            CacheCollections();

            if (!StagingLayerCollection.ContainsKey(member.LayerInfo))
                StagingLayerCollection.Add(member.LayerInfo, new Dictionary<Guid, int>());

            return AddObjectToCollections(member);
        }

        public IList<Guid> ReplaceMember(string rawLayerName, Color layerColor, IEnumerable<GeometryBase> geometry)
        {
            var layerInfo = new LayerInfo(rawLayerName, layerColor);
            var member = new MemberGeo(this, layerInfo, geometry);
            return ReplaceMember(member);
        }
        public IList<Guid> ReplaceMember<T>(IMember<T> member) where T : GeometryBase
        {
            if (member == null) return new List<Guid>();

            CacheCollections();

            if (StagingLayerCollection.TryGetValue(member.LayerInfo, out Dictionary<Guid, int> value))
            {
                foreach (var objID in value.Select(kv => kv.Key))
                {
                    RemoveObjectFromCollections(objID);
                }
            }
            else StagingLayerCollection.Add(member.LayerInfo, new Dictionary<Guid, int>());

            var layerIdx = Layers.FindLayerIndexByFullPath(this, member.LayerInfo.RawLayerName);
            if (layerIdx > 0)
            {
                foreach (var objID in Objects.ObjectIDsByLayer(this, layerIdx))
                {
                    if (Guid.Empty == objID) continue;
                    RemoveObjectFromCollections(objID);
                }
            }

            return AddObjectToCollections(member);
        }
        #endregion

        #region Collections
        IList<Guid> AddObjectToCollections<T>(IMember<T> member) where T : GeometryBase
        {
            var layerIdx = Layers.FindLayerIndexByFullPath(this, member.LayerInfo.RawLayerName);
            var objectAttributes = member.Attributes ?? new ObjectAttributes();
            objectAttributes.Name = Name;
            if (layerIdx > 0)
                objectAttributes.LayerIndex = layerIdx;

            var ids = new List<Guid>();
            foreach (var geometry in member.Geometry)
            {
                if (geometry == null)
                    continue;
                var newID = Guid.NewGuid();
                ids.Add(newID);
                if (layerIdx <= 0)
                    StagingLayerCollection[member.LayerInfo][newID] = -1;
                GeometryCollection.Add(newID, geometry);
                AttributeCollection.Add(newID, objectAttributes);
            }

            return ids;
        }

        void CacheCollections()
        {
            _geometryCollection = GeometryCollection;
            _attributeCollection = AttributeCollection;
        }
        void RemoveObjectFromCollections(Guid objID)
        {
            _geometryCollection.Remove(objID);
            _attributeCollection.Remove(objID);
        }

        public void ClearStagingLayerCollection()
        {
            foreach (var objID in StagingLayerCollection.Values.SelectMany(x => x.Keys))
            {
                RemoveObjectFromCollections(objID);
            }
            StagingLayerCollection.Clear();
        }

        public virtual void Commit()
        {
            RHDoc.AddToRhinoDoc(this, ActiveDoc, true);
        }
        public bool Exists()
        {
            return !IsVirtual;
        }
        public void Delete()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
