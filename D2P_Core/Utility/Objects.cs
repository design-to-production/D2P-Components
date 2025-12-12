using D2P_Core.Components;
using D2P_Core.Interfaces;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P_Core.Utility
{
    public static class Objects
    {
        // Get Type Infos
        public static ComponentType GetComponentTypeFromObject(RhinoObject rhObj)
        {
            var typeLayer = Layers.FindComponentTypeRootLayer(rhObj);
            var typeID = rhObj.Name.Split(Settings.TypeDelimiter).FirstOrDefault();
            var typeName = Layers.GetComponentTypeName(typeLayer);
            var labelSize = Layers.GetComponentTypeLabelSize(typeLayer);
            var layerColor = Layers.FindComponentTypeRootLayer(rhObj)?.Color;
            return new ComponentType(typeID, typeName, labelSize, layerColor);
        }

        // Objects By Layer
        //public static IEnumerable<T> ObjectsByLayer<T>(IMember member)
        //{

        //}

        //public static IEnumerable<T> ObjectsByLayer<T>(IComponentBase component, int layerIdx, LayerScope layerScope) where T : GeometryBase
        //{
        //    IEnumerable<T> objectsByLayer(int idx)
        //    {
        //        var objLayers = component.AttributeCollection.Where(keyVal => keyVal.Value.LayerIndex == idx).Select(kv => kv.Key);
        //        return component.GeometryCollection.Where(kv => objLayers.Contains(kv.Key) && (kv.Value != null)).Select(kv => kv.Value as T);
        //    }

        //    switch (layerScope)
        //    {
        //        case LayerScope.CurrentOnly:
        //            return objectsByLayer(layerIdx);
        //        case LayerScope.IncludeChildren:
        //            var objects = new List<T>();
        //            foreach (var idx in Layers.GetChildLayerIndices(layerIdx))
        //            {
        //                objects.AddRange(objectsByLayer(idx));
        //            }
        //            return objects;
        //        default:
        //            return Enumerable.Empty<T>();
        //    }
        //}
        public static IEnumerable<T> ObjectsByLayer<T>(IComponentBase component, int layerIdx) where T : GeometryBase
        {
            return component.Members
                .FirstOrDefault(m => m.Attributes.LayerIndex == layerIdx)
                .Geometry
                .OfType<T>();
        }
        public static IEnumerable<GeometryBase> ObjectsByLayer(IComponentBase component, int layerIdx) => ObjectsByLayer<GeometryBase>(component, layerIdx);
        public static IEnumerable<RhinoObject> ObjectsByLayer(Layer layer)
        {
            return Settings.ActiveDoc.Objects.FindByLayer(layer);
        }

        // Objects By Group
        public static IEnumerable<RhinoObject> ObjectsByGroup(int grpIdx) => Settings.ActiveDoc.Groups.GroupMembers(grpIdx);
        public static IEnumerable<RhinoObject> ObjectsByGroup(int grpIdx, Layer layer) => ObjectsByGroup(grpIdx).Where(rh => rh.Attributes.LayerIndex == layer?.Index);

        // Delete Objects
        public static int DeleteObjects(IComponentBase component, Layer layer)
        {
            if (!Group.GetGroupIndex(component, out int grpIdx))
                return 0;
            var rhObjects = ObjectsByGroup(grpIdx, layer);
            if (rhObjects == null)
                return 0;
            var objectIds = rhObjects.Select(rh => rh.Id);
            return Settings.ActiveDoc.Objects.Delete(objectIds, true);
        }
        public static int DeleteObjects(IComponentBase component)
        {
            if (!Group.GetGroupIndex(component, out int grpIdx))
                return 0;
            var objectIds = ObjectsByGroup(grpIdx)
                .Select(rh => rh.Id)
                .Where(id => id != component.ID);
            return Settings.ActiveDoc.Objects.Delete(objectIds, true);
        }
        public static int DeleteObjects(IMember member)
        {
            var layer = Layers.FindLayer(member);
            return DeleteObjects(member.Component, layer);

        }

        // Delete Components
        public static int DeleteComponent(IComponentBase component)
        {
            if (!Group.GetGroupIndex(component, out int grpIdx)) return -1;
            var objectIds = ObjectsByGroup(grpIdx).Select(rh => rh.Id);
            return Settings.ActiveDoc.Objects.Delete(objectIds, true);
        }
        public static int DeleteComponents(IEnumerable<IComponentBase> components)
        {
            return components.Sum(comp => DeleteComponent(comp));
        }

        // Add Objects        
        public static void AddObjects(IMember member)
        {
            var layer = Layers.FindLayer(member);
        }

        // Get Object Group IDs
        public static int GetObjectGroupID(Guid objectID)
        {
            var rhinoObject = Settings.ActiveDoc.Objects.Find(objectID);
            if (rhinoObject.GroupCount < 1) { return -1; }
            if (rhinoObject.GroupCount > 1) { return -2; }
            return rhinoObject.GetGroupList()[0];
        }
        public static IEnumerable<int> GetObjectGroupIDs(Guid objectID)
        {
            var rhinoObject = Settings.ActiveDoc.Objects.Find(objectID);
            if (rhinoObject == null || rhinoObject.GroupCount < 1) return new List<int>();
            return rhinoObject.GetGroupList();
        }
    }
}
