using D2P_Core.Components;
using D2P_Core.Interfaces;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P_Core.Utility {
    public static class Objects {
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
        public static IEnumerable<RhinoObject> ObjectsByLayer(IComponentBase component, int layerIdx)
        {
            return ObjectsByGroup(component.GroupIndex)
                .Where(rh => rh.Attributes.LayerIndex == layerIdx);
        }
        public static IEnumerable<RhinoObject> ObjectsByLayer(Layer layer)
        {
            return Settings.ActiveDoc.Objects.FindByLayer(layer);
        }

        // Geometry By Layer
        public static IEnumerable<T> GeometryByLayer<T>(IComponentBase component, int layerIdx) where T : GeometryBase
        {
            return ObjectsByLayer(component, layerIdx)
                .Select(rhObj => rhObj.Geometry)
                .OfType<T>();
        }
        public static IEnumerable<GeometryBase> GeometryByLayer(IComponentBase component, int layerIdx)
        {
            return GeometryByLayer<GeometryBase>(component, layerIdx);
        }
        public static IEnumerable<T> GeometryByLayer<T>(IEnumerable<IMember> members, int layerIdx) where T : GeometryBase
        {
            // TODO: Refactor and make SURE that components without initialized members return objects !!
            return members
                .FirstOrDefault(m => m.Attributes.LayerIndex == layerIdx)
                .Geometry
                .OfType<T>();
        }
        public static IEnumerable<GeometryBase> GeometryByLayer(IEnumerable<IMember> members, int layerIdx)
        {
            return GeometryByLayer<GeometryBase>(members, layerIdx);
        }


        // Objects By Group
        public static IEnumerable<RhinoObject> ObjectsByGroup(int grpIdx) => Settings.ActiveDoc.Groups.GroupMembers(grpIdx);


        // Delete Objects
        public static int DeleteObjects(IComponentBase component, Layer layer)
        {
            //TODO: Handle layer == null
            if (component == null || layer == null)
                return 0;
            if (!Group.GetGroupIndex(component, out int grpIdx))
                return 0;
            var rhObjects = ObjectsByLayer(component, layer.Index);
            if (rhObjects == null)
                return 0;
            var objectIds = rhObjects.Select(rh => rh.Id);
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
