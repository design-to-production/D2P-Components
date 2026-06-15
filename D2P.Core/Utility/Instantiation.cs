using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using D2P.Core.Components;
using D2P.Core.Interfaces;
using D2P.Core.Platforms;

using Rhino.DocObjects;

namespace D2P.Core.Utility {
    public static class Instantiation {
        // Instance By Name
        public static IEnumerable<IComponentBase> InstancesByName(string name)
        {
            var objEnumSettings = Constants.ObjectEnumeratorSettings(name);
            var rhObjects = Settings.ActiveDoc.Objects
                .GetObjectList(objEnumSettings)
                .Where(rhObj => rhObj.Attributes.GroupCount > 0);
            return InstancesFromObjects(rhObjects);
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
            return InstancesFromObjects<IComponentBase>(objects);
        }
        public static IEnumerable<IComponentBase> InstancesFromObjects(IEnumerable<Guid> objectIds)
        {
            return InstancesFromObjects<IComponentBase>(objectIds);
        }
        public static IEnumerable<T> InstancesFromObjects<T>(IEnumerable<Guid> objectIds) where T : class, IComponentBase
        {
            return InstancesFromObjects<T>(objectIds);
        }
        public static IEnumerable<T> InstancesFromObjects<T>(IEnumerable<RhinoObject> rhObjects) where T : class, IComponentBase
        {
            var components = new List<T>();
            var grpIndices = rhObjects
                .SelectMany(obj => obj.GetGroupList())
                .ToHashSet();
            foreach (var grpIdx in grpIndices) {
                var component = InstanceFromGroup<T>(grpIdx);
                if (component != null)
                    components.Add(component);
            }
            return components;
        }


        // Instance From Group
        public static IComponentBase InstanceFromGroup(int grpIdx)
        {
            return InstanceFromGroup<IComponentBase>(grpIdx);
        }
        public static T InstanceFromGroup<T>(int grpIdx) where T : class, IComponentBase
        {
            var grpObjects = Objects.ObjectsByGroup(grpIdx); // 14% (277)
            foreach (var txtLabel in grpObjects.OfType<TextObject>()) {
                if (!txtLabel.Name.Contains(txtLabel.TextGeometry.PlainText))
                    continue;

                var componentType = Objects.GetComponentTypeFromObject(txtLabel); // 14% (270)
                ComponentTable.TryGetValue(componentType.TypeId, out var type);

                if (type == null) type = typeof(Component);
                T component;
                try {
                    component = (T)Activator.CreateInstance(type);
                }
                catch {
                    return null;
                }

                // TODO: Refactoring initialization
                component.ID = txtLabel.Id;
                component.GroupIndex = grpIdx;

                component.TypeId = componentType.TypeId;
                component.TypeName = componentType.TypeName;
                component.LayerColor = componentType.LayerColor;
                component.LabelSize = componentType.LabelSize;

                var label = txtLabel.TextGeometry;
                component.Label.SetObject(label);

                var genericTypes = new HashSet<Type> {
                    typeof(Component),
                    typeof(GHComponent),
                };

                if (genericTypes.Contains(type)) {
                    var members = Members.FindMembers(component); // 7% (130)
                    component.SetMembers(members); // 34% (640)
                }

                return component;
            }
            return null;
        }

        // Instance From Object
        public static IComponentBase InstanceFromObject(RhinoObject obj)
        {
            var grpIndices = Objects.GetObjectGroupIDs(obj.Id);
            foreach (var grpIdx in grpIndices) {
                var component = InstanceFromGroup(grpIdx);
                if (component == null) continue;
                return component;
            }
            return null;
        }
        public static T InstanceFromObject<T>(RhinoObject obj) where T : class, IComponentBase
        {
            var grpIndices = Objects.GetObjectGroupIDs(obj.Id);
            foreach (var grpIdx in grpIndices) {
                var component = InstanceFromGroup<T>(grpIdx);
                if (component == null) continue;
                return component;
            }
            return null;
        }
    }
}
