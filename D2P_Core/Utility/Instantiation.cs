using D2P_Core.Components;
using D2P_Core.Interfaces;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace D2P_Core.Utility
{
    public static class Instantiation
    {
        // Instance By Name
        public static IEnumerable<IComponentBase> InstancesByName(string name)
        {
            var objEnumSettings = Constants.ObjectEnumeratorSettings(name);
            var rhObjects = Settings.ActiveDoc.Objects
                .GetObjectList(objEnumSettings)
                .Where(rhObj => rhObj.Attributes.GroupCount > 0);
            return InstancesFromObjects(rhObjects);
        }
        public static IEnumerable<IComponentBase> InstancesByName(IComponentBase component)
        {
            return InstancesByName(component.Name);
        }

        // Instance By Type
        public static IEnumerable<IComponentBase> InstancesByType(string type, FilterOptions filterOptions)
        {
            var nameFilter = $"{type}{Settings.TypeDelimiter}*";
            var objEnumSettings = Constants.ObjectEnumeratorSettings(nameFilter);
            var reg = new Regex(filterOptions.RegexPattern);
            var rhObjects = Settings.ActiveDoc.Objects
                .GetObjectList(objEnumSettings)
                .Where(rhObj => reg.IsMatch(rhObj.Name) == !filterOptions.ReversePattern);
            return InstancesFromObjects(rhObjects);
        }

        // Instances From Objects
        public static IEnumerable<IComponentBase> InstancesFromObjects(IEnumerable<RhinoObject> objects)
        {
            return InstancesFromObjects(objects
                .Where(obj => obj != null)
                .Select(obj => obj.Id));
        }
        public static IEnumerable<IComponentBase> InstancesFromObjects(IEnumerable<Guid> objectIds)
        {
            return InstancesFromObjects<IComponentBase>(objectIds);
        }
        public static IEnumerable<T> InstancesFromObjects<T>(IEnumerable<Guid> objectIds) where T : class, IComponentBase
        {
            var components = new List<T>();
            var grpIndices = objectIds
                .SelectMany(id => Objects.GetObjectGroupIDs(id))
                .ToHashSet();

            foreach (var grpIdx in grpIndices)
            {
                var component = InstanceFromGroup<T>(grpIdx);
                components.Add(component);
            }

            components.Sort((x, y) => string.Compare(x.ShortName, y.ShortName));
            return components;
        }

        // Instance From Group
        public static IComponentBase InstanceFromGroup(int grpIdx)
        {
            return InstanceFromGroup<IComponentBase>(grpIdx);
        }
        public static T InstanceFromGroup<T>(int grpIdx) where T : class, IComponentBase
        {
            var grpObjects = Objects.ObjectsByGroup(grpIdx);
            foreach (var grpObj in grpObjects)
            {
                if (grpObj.GetType() != typeof(TextObject))
                    continue;
                if (!grpObj.Name.Contains((grpObj as TextObject).TextGeometry.PlainText))
                    continue;

                var componentType = Objects.GetComponentTypeFromObject(grpObj);
                if (!ComponentTable.TryGetValue(componentType.TypeId, out var type))
                    throw new Exception($"No class with type {componentType.TypeId} registered !");

                var component = (T)Activator.CreateInstance(type);
                component.ID = grpObj.Id;

                //if (component.Label)
                //{
                //    //var label = TextEntity.Create("", Plane.WorldXY, Settings.DimensionStyle, false, 0, 0);
                //    //label.TextHeight = LabelSize;
                //    //Label.SetGeometry(label);
                //}

                // TODO: Refactoring => Fill when exists !

                component.TypeId = componentType.TypeId;
                component.TypeName = componentType.TypeName;
                component.LayerColor = componentType.LayerColor;
                component.LabelSize = componentType.LabelSize;

                component.ShortName = (grpObj as TextObject).TextGeometry.PlainText;
                component.Plane = (grpObj as TextObject).TextGeometry.Plane;

                return component;
            }
            return null;
        }

        // Instance From Object
        public static IComponentBase InstanceFromObject(RhinoObject obj)
        {
            var grpIndices = Objects.GetObjectGroupIDs(obj.Id);
            foreach (var grpIdx in grpIndices)
            {
                var component = InstanceFromGroup(grpIdx);
                if (component == null) continue;
                return component;
            }
            return null;
        }
        public static T InstanceFromObject<T>(RhinoObject obj) where T : class, IComponentBase
        {
            var grpIndices = Objects.GetObjectGroupIDs(obj.Id);
            foreach (var grpIdx in grpIndices)
            {
                var component = InstanceFromGroup<T>(grpIdx);
                if (component == null) continue;
                return component;
            }
            return null;
        }
    }
}
