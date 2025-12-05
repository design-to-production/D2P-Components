using D2P_Core.Components;
using D2P_Core.Interfaces;
using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace D2P_Core.Utility
{
    public static class Instantiation
    {
        public static IEnumerable<IComponentBase> InstancesByName(string name, Settings settings = null, RhinoDoc doc = null)
        {
            var objEnumSettings = new ObjectEnumeratorSettings() { HiddenObjects = true, LockedObjects = true, NameFilter = name, ObjectTypeFilter = ObjectType.Annotation };
            var rhObjects = doc.Objects.GetObjectList(objEnumSettings).Where(rhObj => rhObj.Attributes.GroupCount > 0);
            return InstancesFromObjects(rhObjects, settings, doc);
        }
        public static IEnumerable<IComponentBase> InstancesByName(IComponent component, RhinoDoc doc = null) => InstancesByName(component.Name, component.Settings, doc);
        public static IEnumerable<IComponentBase> InstancesByType(string type, Settings settings, FilterOptions filterOptions, RhinoDoc doc = null)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            var nameFilter = $"{type}{settings.TypeDelimiter}*";
            var objEnumSettings = new ObjectEnumeratorSettings() { HiddenObjects = true, LockedObjects = true, NameFilter = nameFilter, ObjectTypeFilter = ObjectType.Annotation };
            var reg = new Regex(filterOptions.RegexPattern);
            var rhObjects = doc.Objects.GetObjectList(objEnumSettings).Where(rhObj => reg.IsMatch(rhObj.Name) == !filterOptions.ReversePattern);
            return InstancesFromObjects(rhObjects, settings, doc);
        }
        public static IEnumerable<IComponentBase> InstancesFromObjects(IEnumerable<RhinoObject> objects, Settings settings = null, RhinoDoc doc = null)
        {
            return InstancesFromObjects(objects.Where(obj => obj != null).Select(obj => obj.Id), settings, doc);
        }
        public static IEnumerable<IComponentBase> InstancesFromObjects(IEnumerable<Guid> objectIds, Settings settings = null, RhinoDoc doc = null)
        {
            return InstancesFromObjects<IComponentBase>(objectIds, settings, doc);
        }
        public static IEnumerable<T> InstancesFromObjects<T>(IEnumerable<Guid> objectIds, Settings settings = null, RhinoDoc doc = null) where T : class, IComponentBase
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            settings = settings ?? new Settings();

            var components = new List<T>();
            var grpIndices = objectIds
                .SelectMany(id => Objects.ObjectGroupIDs(id, doc))
                .ToHashSet();

            foreach (var grpIdx in grpIndices)
            {
                var component = InstanceFromGroup<T>(grpIdx, settings, doc);
                components.Add(component);
            }

            components.Sort((x, y) => string.Compare(x.ShortName, y.ShortName));
            return components;
        }

        public static IComponentBase InstanceFromGroup(int grpIdx, Settings settings = null, RhinoDoc doc = null) => InstanceFromGroup<IComponentBase>(grpIdx, settings, doc);

        public static T InstanceFromGroup<T>(int grpIdx, Settings settings = null, RhinoDoc doc = null) where T : class, IComponentBase
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            settings = settings ?? new Settings();
            var grpObjects = Objects.ObjectsByGroup(grpIdx, doc);
            foreach (var grpObj in grpObjects)
            {
                if (grpObj.GetType() != typeof(TextObject))
                    continue;
                if (!grpObj.Name.Contains((grpObj as TextObject).TextGeometry.PlainText))
                    continue;

                var typeId = Objects.ComponentTypeIDFromObject(grpObj, settings);
                if (!ComponentTable.TryGetValue(typeId, out var type))
                    throw new Exception($"No class with type {typeId} registered !");

                var component = (T)Activator.CreateInstance(type);
                component.ID = grpObj.Id;

                var compType = Objects.ComponentTypeFromObject(grpObj);

                return component;
            }
            return null;
        }

        public static IComponentBase InstanceFromObject(RhinoObject obj, Settings settings = null, RhinoDoc doc = null)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            settings = settings ?? new Settings();
            var grpIndices = Objects.ObjectGroupIDs(obj.Id, doc);
            foreach (var grpIdx in grpIndices)
            {
                var component = InstanceFromGroup(grpIdx, settings, doc);
                if (component == null) continue;
                return component;
            }
            return null;
        }
        public static T InstanceFromObject<T>(RhinoObject obj, Settings settings = null, RhinoDoc doc = null) where T : class, IComponentBase
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            settings = settings ?? new Settings();
            var grpIndices = Objects.ObjectGroupIDs(obj.Id, doc);
            foreach (var grpIdx in grpIndices)
            {
                var component = InstanceFromGroup<T>(grpIdx, settings, doc);
                if (component == null) continue;
                return component;
            }
            return null;
        }
        public static IComponentBase InstanceFromObject(Guid objId, Settings settings = null, RhinoDoc doc = null)
        {
            return InstanceFromObject(doc.Objects.FindId(objId), settings, doc);
        }

        public static IComponentBase GetParentComponent(IComponentBase component, out int parentsFound, RhinoDoc doc = null)
        {
            doc = doc ?? component.ActiveDoc;
            var parentNameSegments = component.ShortName.Split(component.Settings.NameDelimiter).ToList();
            if (parentNameSegments.Count <= 1)
            {
                parentsFound = 0;
                return null;
            }
            parentNameSegments.RemoveAt(parentNameSegments.Count - 1);
            var parentName = string.Join(component.Settings.NameDelimiter.ToString(), parentNameSegments);
            var namingCondition = $"*{component.Settings.TypeDelimiter}{parentName}";
            var objEnumSettings = new ObjectEnumeratorSettings() { HiddenObjects = true, LockedObjects = true, NameFilter = namingCondition, ObjectTypeFilter = ObjectType.Annotation };
            var rhObjects = doc.Objects.GetObjectList(objEnumSettings);
            var parents = InstancesFromObjects(rhObjects, component.Settings, doc);
            parentsFound = parents.Count();
            return parents.FirstOrDefault();
        }
        public static IEnumerable<IComponentBase> GetChildren(IComponentBase component, IEnumerable<string> filterTypes = null, RhinoDoc doc = null)
        {
            var namingCondition = $"*{component.Settings.TypeDelimiter}{component.ShortName}{component.Settings.NameDelimiter}*";
            var objEnumSettings = new ObjectEnumeratorSettings() { HiddenObjects = true, LockedObjects = true, NameFilter = namingCondition, ObjectTypeFilter = ObjectType.Annotation };
            var rhObjects = component.ActiveDoc.Objects.GetObjectList(objEnumSettings)
                .Where(rhObj => !rhObj.Name.Contains(component.Settings.JointDelimiter));
            if (filterTypes != null && filterTypes.Any())
                rhObjects = rhObjects.Where(rhObj => filterTypes.Contains(rhObj.Name.Split(component.Settings.TypeDelimiter)[0]));
            var children = InstancesFromObjects(rhObjects, component.Settings, doc);
            return children;
        }
        public static IEnumerable<IComponentBase> GetJoints(IComponentBase component, IEnumerable<string> filterTypes = null, RhinoDoc doc = null)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            var namingCondition = $"*{component.ShortName}*";
            var objEnumSettings = new ObjectEnumeratorSettings() { HiddenObjects = true, LockedObjects = true, NameFilter = namingCondition, ObjectTypeFilter = ObjectType.Annotation };
            var escapedString = $"(.*{component.Settings.TypeDelimiter}{component.ShortName}{component.Settings.JointDelimiter}.*)" +
                $"|(.*{component.Settings.JointDelimiter}{component.ShortName}{component.Settings.JointDelimiter}.*)" +
                $"|(.*{component.Settings.JointDelimiter}{component.ShortName}$)";
            escapedString = escapedString.Replace("+", "\\+");
            var reg = new Regex(escapedString);
            var rhObjects = doc.Objects.GetObjectList(objEnumSettings).Where(rhObj => reg.IsMatch(rhObj.Name));
            if (filterTypes != null && filterTypes.Any())
                rhObjects = rhObjects.Where(rhObj => filterTypes.Contains(rhObj.Name.Split(component.Settings.TypeDelimiter)[0]));
            var joints = InstancesFromObjects(rhObjects, component.Settings, doc);
            return joints;
        }
        public static IEnumerable<IComponentBase> GetConnectedComponents(IComponentBase component, IEnumerable<string> typeFilter, RhinoDoc doc = null)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            IEnumerable<IComponentBase> joints;
            if (component.ShortName.Contains(component.Settings.JointDelimiter))
                joints = component.ShortName.Split(component.Settings.JointDelimiter).SelectMany(x => InstancesByName($"*{component.Settings.TypeDelimiter}{x}", component.Settings, doc));
            else joints = GetJoints(component);
            var connectedComponentNames = joints
                .SelectMany(x => x.ShortName.Split(component.Settings.JointDelimiter)
                .Where(y => y != component.ShortName))
                .ToHashSet();
            var connectedComponents = connectedComponentNames.SelectMany(x => InstancesByName($"*{component.Settings.TypeDelimiter}{x}", component.Settings, doc));
            if (typeFilter.Any())
                connectedComponents = connectedComponents.Where(x => typeFilter.Contains(x.TypeId));
            return connectedComponents;
        }
        public static IEnumerable<IComponentType> GetComponentTypes(Settings settings, RhinoDoc doc = null)
        {
            var componentTypeLayers = Layers.FindAllExistentComponentTypeRootLayers(settings, doc);
            return componentTypeLayers.Select(layer => new ComponentType(layer, settings));
        }
    }
}
