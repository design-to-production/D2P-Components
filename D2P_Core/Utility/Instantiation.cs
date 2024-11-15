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
        public static List<IComponent> InstancesByName(string name, Settings settings, RhinoDoc doc = null)
        {
            var objEnumSettings = new ObjectEnumeratorSettings() { HiddenObjects = true, LockedObjects = true, NameFilter = name, ObjectTypeFilter = ObjectType.Annotation };
            var rhObjects = doc.Objects.GetObjectList(objEnumSettings).Where(rhObj => rhObj.Attributes.GroupCount > 0);
            return InstancesFromObjects(rhObjects, settings, doc);
        }
        public static List<IComponent> InstancesByName(IComponent component, RhinoDoc doc = null) => InstancesByName(component.Name, component.Settings, doc);

        public static List<IComponent> InstancesByType(string type, Settings settings, RhinoDoc doc = null, char typeDelimiter = ':', string filter = "")
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            var nameFilter = string.IsNullOrEmpty(filter) ? "*" : $"{type}{typeDelimiter}{filter}";
            var objEnumSettings = new ObjectEnumeratorSettings() { HiddenObjects = true, LockedObjects = true, NameFilter = nameFilter, ObjectTypeFilter = ObjectType.Annotation };
            var reg = new Regex($"(^{type}{typeDelimiter}.*)");
            var rhObjects = doc.Objects.GetObjectList(objEnumSettings).Where(rhObj => reg.IsMatch(rhObj.Name));
            return InstancesFromObjects(rhObjects, settings, doc);
        }
        public static List<IComponent> InstancesFromObjects(IEnumerable<RhinoObject> objects, Settings settings, RhinoDoc doc = null)
        {
            return InstancesFromObjects(objects.Where(obj => obj != null).Select(obj => obj.Id), settings, doc);
        }
        public static List<IComponent> InstancesFromObjects(IEnumerable<Guid> objectIds, Settings settings, RhinoDoc doc = null)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            var allObjects = new List<TextObject>();
            var processedGroups = new List<int>();
            foreach (var objId in objectIds)
            {
                var grpIndices = Objects.ObjectGroupIDs(objId, doc);
                foreach (var grpIdx in grpIndices)
                {
                    if (processedGroups.Contains(grpIdx)) continue;
                    processedGroups.Add(grpIdx);
                    var idFound = false;
                    var grpObjects = Objects.ObjectsByGroup(grpIdx, doc);
                    foreach (var grpObj in grpObjects)
                    {
                        if (idFound)
                            break;
                        if (grpObj.GetType() != typeof(TextObject))
                            continue;
                        if (!grpObj.Name.Contains((grpObj as TextObject).TextGeometry.PlainText))
                            continue;
                        allObjects.Add(grpObj as TextObject);
                        idFound = true;
                    }
                }
            }

            var instances = new List<IComponent>();
            foreach (var obj in allObjects)
            {
                var componentType = new ComponentType(obj, settings);
                instances.Add(new Component(componentType, obj.Id));
            }
            instances.Sort((x, y) => string.Compare(x.ShortName, y.ShortName));
            return instances;

        }

        public static IComponent InstanceFromGroup(int groupIndex, Settings settings, RhinoDoc doc = null)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            var geometry = doc?.Objects.FindByGroup(groupIndex);
            return InstancesFromObjects(geometry, settings, doc).FirstOrDefault();
        }
        public static IEnumerable<IComponent> InstancesFromGroups(int[] groupIndices, Settings settings, RhinoDoc doc = null)
        {
            return groupIndices.Select(idx => InstanceFromGroup(idx, settings, doc));
        }

        public static IComponent GetParentComponent(IComponent component, out int parentsFound, RhinoDoc doc = null)
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
            parentsFound = parents.Count;
            return parents.FirstOrDefault();
        }
        public static IEnumerable<IComponent> GetChildren(IComponent component, IEnumerable<string> filterTypes = null, RhinoDoc doc = null)
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
        public static IEnumerable<IComponent> GetJoints(IComponent component, IEnumerable<string> filterTypes = null, RhinoDoc doc = null)
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
        public static IEnumerable<IComponent> GetConnectedComponents(IComponent component, IEnumerable<string> typeFilter, RhinoDoc doc = null)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            IEnumerable<IComponent> joints;
            if (component.ShortName.Contains(component.Settings.JointDelimiter))
                joints = component.ShortName.Split(component.Settings.JointDelimiter).SelectMany(x => InstancesByName($"*{component.Settings.TypeDelimiter}{x}", component.Settings, doc));
            else joints = GetJoints(component);
            var connectedComponentNames = joints
                .SelectMany(x => x.ShortName.Split(component.Settings.JointDelimiter)
                .Where(y => y != component.ShortName))
                .ToHashSet();
            var connectedComponents = connectedComponentNames.SelectMany(x => InstancesByName($"*{component.Settings.TypeDelimiter}{x}", component.Settings, doc));
            if (typeFilter.Any())
                connectedComponents = connectedComponents.Where(x => typeFilter.Contains(x.TypeID));
            return connectedComponents;
        }
        public static IEnumerable<IComponentType> GetComponentTypes(Settings settings, RhinoDoc doc = null)
        {
            var componentTypeLayers = Layers.FindAllExistentComponentTypeRootLayers(settings, doc);
            return componentTypeLayers.Select(layer => new ComponentType(layer, settings));
        }
    }
}
