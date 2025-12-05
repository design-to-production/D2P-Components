using D2P_Core.Components;
using D2P_Core.Components.Grasshopper;
using D2P_Core.Enums;
using D2P_Core.Interfaces;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace D2P_Core.Utility
{
    public static class Objects
    {
        public static ComponentType ComponentTypeFromObject(RhinoObject obj, Settings settings = null)
        {
            settings = settings ?? new Settings();
            var typeID = ComponentTypeIDFromObject(obj, settings);
            var typeName = ComponentTypeNameFromObject(obj, settings);
            return new ComponentType(typeID, typeName, settings);
        }
        public static string ComponentTypeIDFromObject(RhinoObject obj, Settings settings) => obj.Name.Split(settings.TypeDelimiter).FirstOrDefault();
        public static string ComponentTypeNameFromObject(RhinoObject obj, Settings settings) => Layers.GetComponentTypeName(obj, settings);
        public static Color ComponentTypeLayerColorFromObject(RhinoObject obj, Settings settings) => Layers.GetComponentTypeRootLayer(obj, settings).Color;

        public static IEnumerable<T> ObjectsByLayer<T>(int layerIdx, GrasshopperComponent component, LayerScope layerScope) where T : GeometryBase
        {
            IEnumerable<T> objectsByLayer(int idx)
            {
                var objLayers = component.AttributeCollection.Where(keyVal => keyVal.Value.LayerIndex == idx).Select(kv => kv.Key);
                return component.GeometryCollection.Where(kv => objLayers.Contains(kv.Key) && (kv.Value != null)).Select(kv => kv.Value as T);
            }

            switch (layerScope)
            {
                case LayerScope.CurrentOnly:
                    return objectsByLayer(layerIdx);
                case LayerScope.IncludeChildren:
                    var objects = new List<T>();
                    var doc = component.ActiveDoc;
                    foreach (var idx in Layers.GetChildLayerIndices(layerIdx, doc))
                    {
                        objects.AddRange(objectsByLayer(idx));
                    }
                    return objects;
                default:
                    return Enumerable.Empty<T>();
            }
        }
        public static IEnumerable<T> ObjectsByLayer<T>(IComponentBase component, int layerIdx) where T : GeometryBase
        {
            return component.Members
                .FirstOrDefault(m => m.Attributes.LayerIndex == layerIdx)
                .Geometry.OfType<T>();
        }

        public static IEnumerable<GeometryBase> ObjectsByLayer(int layerIdx, IComponentBase component, LayerScope layerScope) => ObjectsByLayer<GeometryBase>(component, layerIdx);
        public static IEnumerable<RhinoObject> ObjectsByLayer(Layer layer, RhinoDoc doc = null)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            return doc.Objects.FindByLayer(layer);
        }
        public static IEnumerable<Guid> ObjectIDsByLayer(IComponentBase component, int layerIdx, RhinoDoc doc = null)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            var filter = new ObjectEnumeratorSettings()
            {
                LayerIndexFilter = layerIdx,
                HiddenObjects = true,
                LockedObjects = true,
                NameFilter = component.Name
            };
            return doc.Objects.FindByFilter(filter).Select(rhObj => rhObj.Id);
        }

        public static IEnumerable<RhinoObject> ObjectsByGroup(int grpIdx, RhinoDoc doc) => doc.Groups.GroupMembers(grpIdx);
        public static IEnumerable<RhinoObject> ObjectsByGroup(int grpIdx, Layer layer, RhinoDoc doc) => ObjectsByGroup(grpIdx, doc).Where(rh => rh.Attributes.LayerIndex == layer?.Index);

        public static int DeleteObjects(IComponentBase component, Layer layer)
        {
            if (!Group.GetGroupIndex(component, out int grpIdx))
                return 0;
            var rhObjects = ObjectsByGroup(grpIdx, layer, component.ActiveDoc);
            if (rhObjects == null)
                return 0;
            var objectIds = rhObjects.Select(rh => rh.Id);
            return component.ActiveDoc.Objects.Delete(objectIds, true);
        }
        public static int DeleteObjects(IComponentBase component)
        {
            if (!Group.GetGroupIndex(component, out int grpIdx))
                return 0;
            var objectIds = ObjectsByGroup(grpIdx, component.ActiveDoc)
                .Select(rh => rh.Id)
                .Where(id => id != component.ID);
            return component.ActiveDoc.Objects.Delete(objectIds, true);
        }

        public static int DeleteObjects(IMember member)
        {
            var layer = Layers.FindLayer(member);
            return DeleteObjects(member.Component, layer);

        }
        public static void ReplaceObjects(IMember member)
        {
            DeleteObjects(member);
            AddObjects(member);
        }
        public static void AddObjects(IMember member)
        {
            var layer = Layers.FindLayer(member);
        }

        public static int DeleteComponent(IComponentBase component)
        {
            if (!Group.GetGroupIndex(component, out int grpIdx)) return -1;
            var objectIds = ObjectsByGroup(grpIdx, component.ActiveDoc).Select(rh => rh.Id);
            return component.ActiveDoc.Objects.Delete(objectIds, true);
        }
        public static int DeleteComponents(IEnumerable<IComponentBase> components)
        {
            return components.Sum(comp => DeleteComponent(comp));
        }

        public static int ObjectGroupID(Guid objectID, RhinoDoc doc)
        {
            var rhinoObject = doc.Objects.Find(objectID);
            if (rhinoObject.GroupCount < 1) { return -1; }
            if (rhinoObject.GroupCount > 1) { return -2; }
            return rhinoObject.GetGroupList()[0];
        }

        public static IEnumerable<int> ObjectGroupIDs(Guid objectID, RhinoDoc doc)
        {
            var rhinoObject = doc.Objects.Find(objectID);
            if (rhinoObject == null || rhinoObject.GroupCount < 1) return new List<int>();
            return rhinoObject.GetGroupList();
        }

    }
}
