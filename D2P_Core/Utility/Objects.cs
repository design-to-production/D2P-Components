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
        public enum LayerScope
        {
            CurrentOnly,
            IncludeChildren
        }

        public static string ComponentTypeIDFromObject(RhinoObject obj, Settings settings) => obj.Name.Split(settings.TypeDelimiter).FirstOrDefault();
        public static string ComponentTypeNameFromObject(RhinoObject obj, Settings settings) => Layers.GetComponentTypeName(obj, settings);
        public static Color ComponentTypeLayerColorFromObject(RhinoObject obj, Settings settings) => Layers.GetComponentTypeRootLayer(obj, settings).Color;

        public static IEnumerable<T> ObjectsByLayer<T>(int layerIdx, IComponent component, LayerScope layerScope) where T : GeometryBase
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
        public static IEnumerable<GeometryBase> ObjectsByLayer(int layerIdx, IComponent component, LayerScope layerScope) => ObjectsByLayer<GeometryBase>(layerIdx, component, layerScope);
        public static IEnumerable<RhinoObject> ObjectsByLayer(Layer layer, RhinoDoc doc = null)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            return doc.Objects.FindByLayer(layer);
        }
        public static IEnumerable<RhinoObject> ObjectsByGroup(int grpIdx, RhinoDoc doc) => doc.Groups.GroupMembers(grpIdx);
        public static IEnumerable<RhinoObject> ObjectsByGroup(int grpIdx, Layer layer, RhinoDoc doc) => ObjectsByGroup(grpIdx, doc).Where(rh => rh.Attributes.LayerIndex == layer.Index);

        public static int DeleteObjects(IComponent component, Layer layer)
        {
            var rhObjects = ObjectsByGroup(component.GroupIdx, layer, component.ActiveDoc);
            if (rhObjects == null)
                return 0;
            var objectIds = rhObjects.Select(rh => rh.Id);
            return component.ActiveDoc.Objects.Delete(objectIds, true);
        }
        public static int DeleteObjects(IComponent component)
        {
            var objectIds = ObjectsByGroup(component.GroupIdx, component.ActiveDoc)
                .Select(rh => rh.Id)
                .Where(id => id != component.ID);
            return component.ActiveDoc.Objects.Delete(objectIds, true);
        }

        public static int DeleteComponent(IComponent component)
        {
            var objectIds = ObjectsByGroup(component.GroupIdx, component.ActiveDoc).Select(rh => rh.Id);
            return component.ActiveDoc.Objects.Delete(objectIds, true);
        }
        public static int DeleteComponents(IEnumerable<IComponent> components)
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
