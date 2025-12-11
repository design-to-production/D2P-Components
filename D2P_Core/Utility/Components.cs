using D2P_Core.Components;
using D2P_Core.Interfaces;
using Rhino.DocObjects;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace D2P_Core.Utility
{
    internal static class Components
    {
        // Get Parent Component
        public static IComponentBase GetParentComponent(IComponentBase component, out int parentsFound)
        {
            var parentNameSegments = component.ShortName.Split(Settings.NameDelimiter).ToList();
            if (parentNameSegments.Count <= 1)
            {
                parentsFound = 0;
                return null;
            }
            parentNameSegments.RemoveAt(parentNameSegments.Count - 1);
            var parentName = string.Join(Settings.NameDelimiter.ToString(), parentNameSegments);
            var namingCondition = $"*{Settings.TypeDelimiter}{parentName}";
            var objEnumSettings = new ObjectEnumeratorSettings()
            {
                HiddenObjects = true,
                LockedObjects = true,
                NameFilter = namingCondition,
                ObjectTypeFilter = ObjectType.Annotation
            };
            var rhObjects = Settings.ActiveDoc.Objects.GetObjectList(objEnumSettings);
            var parents = Instantiation.InstancesFromObjects(rhObjects);
            parentsFound = parents.Count();
            return parents.FirstOrDefault();
        }
        public static IEnumerable<IComponentBase> GetChildren(IComponentBase component, IEnumerable<string> filterTypes = null)
        {
            var namingCondition = $"*{Settings.TypeDelimiter}{component.ShortName}{Settings.NameDelimiter}*";
            var objEnumSettings = new ObjectEnumeratorSettings()
            {
                HiddenObjects = true,
                LockedObjects = true,
                NameFilter = namingCondition,
                ObjectTypeFilter = ObjectType.Annotation
            };
            var rhObjects = Settings.ActiveDoc.Objects.GetObjectList(objEnumSettings)
                .Where(rhObj => !rhObj.Name.Contains(Settings.JointDelimiter));
            if (filterTypes != null && filterTypes.Any())
                rhObjects = rhObjects.Where(rhObj => filterTypes.Contains(rhObj.Name.Split(Settings.TypeDelimiter)[0]));
            var children = Instantiation.InstancesFromObjects(rhObjects);
            return children;
        }
        public static IEnumerable<IComponentBase> GetJoints(IComponentBase component, IEnumerable<string> filterTypes = null)
        {
            var namingCondition = $"*{component.ShortName}*";
            var objEnumSettings = new ObjectEnumeratorSettings() { HiddenObjects = true, LockedObjects = true, NameFilter = namingCondition, ObjectTypeFilter = ObjectType.Annotation };
            var escapedString = $"(.*{Settings.TypeDelimiter}{component.ShortName}{Settings.JointDelimiter}.*)" +
                $"|(.*{Settings.JointDelimiter}{component.ShortName}{Settings.JointDelimiter}.*)" +
                $"|(.*{Settings.JointDelimiter}{component.ShortName}$)";
            escapedString = escapedString.Replace("+", "\\+");
            var reg = new Regex(escapedString);
            var rhObjects = Settings.ActiveDoc.Objects.GetObjectList(objEnumSettings)
                .Where(rhObj => reg.IsMatch(rhObj.Name));
            if (filterTypes != null && filterTypes.Any())
                rhObjects = rhObjects.Where(rhObj => filterTypes.Contains(rhObj.Name.Split(Settings.TypeDelimiter)[0]));
            var joints = Instantiation.InstancesFromObjects(rhObjects);
            return joints;
        }
        public static IEnumerable<IComponentBase> GetConnectedComponents(IComponentBase component, IEnumerable<string> typeFilter)
        {
            IEnumerable<IComponentBase> joints;
            if (component.ShortName.Contains(Settings.JointDelimiter))
                joints = component.ShortName.Split(Settings.JointDelimiter)
                    .SelectMany(x => Instantiation.InstancesByName($"*{Settings.TypeDelimiter}{x}"));
            else joints = GetJoints(component);
            var connectedComponentNames = joints
                .SelectMany(x => x.ShortName.Split(Settings.JointDelimiter)
                .Where(y => y != component.ShortName))
                .ToHashSet();
            var connectedComponents = connectedComponentNames
                .SelectMany(x => Instantiation.InstancesByName($"*{Settings.TypeDelimiter}{x}"));
            if (typeFilter.Any())
                connectedComponents = connectedComponents.Where(x => typeFilter.Contains(x.TypeId));
            return connectedComponents;
        }

        public static IEnumerable<IComponentType> GetComponentTypes()
        {
            var componentTypeLayers = Layers.FindComponentTypeRootLayers();
            return componentTypeLayers.Select(layer => new ComponentType(layer));
        }
    }
}
