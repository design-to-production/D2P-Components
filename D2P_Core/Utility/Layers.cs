using D2P_Core.Components;
using D2P_Core.Components.Grasshopper;
using D2P_Core.Interfaces;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace D2P_Core.Utility
{
    using SortedLayerColletcion = SortedDictionary<ILayerInfo, Dictionary<Guid, int>>;

    public static class Layers
    {
        // Create Layer
        public static Layer CreateLayer(IComponentBase component, IMember member)
        {
            if (!FindRootLayer(out Layer rootLayer))
                rootLayer = CreateRootLayer();
            var layerName = ComposeComponentTypeLayerName(component);
            var componentLayer = new Layer()
            {
                Id = Guid.NewGuid(),
                ParentLayerId = rootLayer.Id,
                Name = layerName,
                Color = component.LayerColor
            };
            var layerIdx = Settings.ActiveDoc.Layers.Add(componentLayer);
            componentLayer.Index = layerIdx;
            return componentLayer;
        }

        // Find Root Layer
        public static bool FindRootLayer(out Layer rootLayer)
        {
            rootLayer = FindLayerByName(Settings.RootLayerName);
            return rootLayer != null;
        }

        // Find Layer
        public static Layer FindLayer(IMember member)
        {
            var component = member.Component;
            var layerName = ComposeComponentLayerName(component, member);
            var componentLayers = GetComponentLayers(component);
            return componentLayers.FirstOrDefault(l => !l.IsReference && l.Name == layerName);
        }
        public static Layer FindLayer(IComponentBase component, string rawLayerName, out int layersFound)
        {
            string layerName = string.Empty;
            if (string.IsNullOrEmpty(rawLayerName))
                layerName = ComposeComponentTypeLayerName(component);
            else layerName = ComposeComponentLayerName(component, rawLayerName);
            var componentLayers = GetComponentLayers(component);
            var matchedLayers = componentLayers.Where(l => !l.IsReference && l.Name == layerName);
            layersFound = matchedLayers.Count();
            if (matchedLayers.Count() > 1 || !matchedLayers.Any())
                return null;
            return matchedLayers.First();
        }
        public static Layer FindLayer(int layerIndex)
        {
            return Settings.ActiveDoc.Layers.FindIndex(layerIndex);
        }

        // Find Layer/Index By Full Path
        public static int FindLayerIndexByFullPath(IComponentBase component, string rawLayerName)
        {
            return FindLayerByFullPath(component, rawLayerName)?.Index ?? -1;
        }
        public static Layer FindLayerByFullPath(IComponentBase component, string rawLayerName)
        {
            var layerNames = new Queue<string>(rawLayerName.Split(Settings.LayerNameDelimiter)
                .Where(s => !string.IsNullOrEmpty(s)));
            if (!layerNames.Any())
                return null;

            var rootLayerID = GetComponentLayerID(component);
            return TraverseLayers(component, ref layerNames, rootLayerID);
        }

        // Find ComponentType Root Layers
        public static IEnumerable<Layer> FindComponentTypeRootLayers()
        {
            if (!FindRootLayer(out Layer rootLayer))
                return Enumerable.Empty<Layer>();
            var childLayers = GetChildLayers(rootLayer);
            return childLayers.Where(layer => IsComponentTypeRootLayer(layer));
        }

        public static Layer FindComponentLayerByType(string type)
        {
            var layerNames = Settings.ActiveDoc.Layers.Where(l => !l.IsReference).Select(l => l.Name);
            var componentLayerName = ComposeComponentTypeLayerName(type, "");
            componentLayerName = layerNames.FirstOrDefault(name => name.StartsWith(componentLayerName));
            if (componentLayerName == null)
                return null;
            return FindLayerByName(componentLayerName);
        }
        public static Layer FindLayerByName(string layerName, bool includeReferenced = false)
        {
            bool condition(Layer layer) => includeReferenced || !layer.IsReference;
            var layerFound = Settings.ActiveDoc.Layers
                .FirstOrDefault(l => condition(l) && l.Name == layerName && l.FullPath.StartsWith(Settings.RootLayerName));
            if (layerFound == null)
                return null;
            return layerFound;
        }

        public static Layer CreateLayer(IMember member)
        {
            var component = member.Component;
            var layerName = ComposeMemberLayerName(member);
            var layerSegments = new Queue<string>(layerName
                .Split(Settings.LayerNameDelimiter)
                .Where(s => !string.IsNullOrEmpty(s))
            );

            var componentLayer = GetComponentTypeRootLayer(component);
            if (componentLayer == null || componentLayer.Index == 0)
                componentLayer = CreateComponentTypeLayer(component);

            return TraverseLayers(component, ref layerSegments, componentLayer.Id, member.LayerInfo);
        }

        public static string ComposeMemberLayerName(IMember member)
        {
            if (member == null)
                return string.Empty;

            var layerName = member.LayerInfo?.RawLayerName ?? string.Empty;
            if (member.Parent == null)
                return layerName;

            var parentLayerName = ComposeMemberLayerName(member.Parent);
            if (string.IsNullOrEmpty(parentLayerName))
                return layerName;

            var layerDelimiter = Settings.LayerNameDelimiter;
            return $"{parentLayerName}{layerDelimiter}{layerDelimiter}{layerName}";
        }

        // Create Staging Layers
        public static int[] CreateStagingLayers(GrasshopperComponent component)
        {
            var createdLayerIndices = new List<int>();
            var componentLayer = GetComponentTypeRootLayer(component);
            if (componentLayer == null || componentLayer.Index == 0)
                componentLayer = CreateComponentTypeLayer(component);

            var sortedStagingLayers = new SortedLayerColletcion(component.StagingLayerCollection, new LayerInfoComparer());
            foreach (var kv in sortedStagingLayers)
            {
                var rawLayerName = kv.Key.RawLayerName;
                var layerNames = new Queue<string>(rawLayerName.Split(Settings.LayerNameDelimiter).Where(s => !string.IsNullOrEmpty(s)));
                var layer = layerNames.Any() && component.IsInitialized ?
                    TraverseLayers(component, ref layerNames, componentLayer.Id, kv.Key)
                    : componentLayer;

                var geometryIds = new List<Guid>(kv.Value.Keys);
                foreach (var geometryId in geometryIds)
                {
                    kv.Value[geometryId] = layer.Index;
                    createdLayerIndices.Add(layer.Index);
                }
            }
            return createdLayerIndices.ToArray();
        }

        public static bool IsComponentTypeRootLayer(IComponentBase component, string layerName)
        {
            return layerName.Split(Settings.LayerDescriptionDelimiter).FirstOrDefault() == component.TypeId;
        }
        public static bool IsComponentTypeRootLayer(Layer layer)
        {
            if (layer == null)
                return false;
            var regex = new Regex($".*(?<!/s){Settings.LayerDescriptionDelimiter}(?<!/s).*");
            return regex.IsMatch(layer.Name);
        }

        // Compose Component Layer Name
        public static string ComposeComponentLayerName(IComponentBase component, string rawLayerName)
        {
            return $"{component.TypeId}{Settings.LayerDelimiter}{rawLayerName.Split(Settings.LayerNameDelimiter).LastOrDefault()}";
        }
        public static string ComposeComponentLayerName(IComponentBase component, IMember member)
        {
            var typeId = component.TypeId;
            var layerDelimiter = Settings.LayerDelimiter;
            var layerName = member.LayerInfo.RawLayerName
                .Split(Settings.LayerNameDelimiter)
                .LastOrDefault();
            return $"{typeId}{layerDelimiter}{layerName}";
        }

        // Compose Component Type Layer Name
        public static string ComposeComponentTypeLayerName(IComponentType componentType)
        {
            return ComposeComponentTypeLayerName(componentType.TypeId, componentType.TypeName);
        }
        public static string ComposeComponentTypeLayerName(string type, string description)
        {
            return $"{type} {Settings.LayerDescriptionDelimiter} {description}";
        }

        public static string ComposeFullLayerPath(string layerName, Guid parentLayerId)
        {
            var parentLayerPath = Settings.ActiveDoc.Layers.FindId(parentLayerId)?.FullPath;
            return parentLayerPath != null ? $"{parentLayerPath}::{layerName}" : string.Empty;
        }
        public static string DecomposeLayerName(IComponentBase component, string layerName)
        {
            if (IsComponentTypeRootLayer(component, layerName))
            {
                var idx = layerName.IndexOf(Settings.LayerDescriptionDelimiter);
                return layerName.Substring(idx);
            }
            return layerName.Substring(layerName.IndexOf(Settings.LayerDelimiter));
        }




        public static Layer GetComponentTypeRootLayer(RhinoObject obj)
        {
            var objLayer = FindLayer(obj.Attributes.LayerIndex);
            if (IsComponentTypeRootLayer(objLayer))
                return objLayer;

            var componentTypeAncestorLayers = new List<Layer>();
            TraverseAncestorLayers(objLayer.Id, ref componentTypeAncestorLayers);

            return componentTypeAncestorLayers.Find(l => IsComponentTypeRootLayer(l));
        }
        public static Layer GetComponentTypeRootLayer(IComponentBase component)
        {
            var componentTypeRootLayerName = ComposeComponentTypeLayerName(component);
            return FindLayerByName(componentTypeRootLayerName);
        }

        public static Guid GetComponentLayerID(IComponentBase component)
        {
            return GetComponentTypeRootLayer(component)?.Id ?? Guid.Empty;
        }

        public static string GetComponentTypeID(Layer layer)
        {
            if (!IsComponentTypeRootLayer(layer))
                return string.Empty;
            var substringStartIdx = layer.Name.IndexOf(Settings.LayerDescriptionDelimiter);
            return layer.Name.Substring(0, substringStartIdx - 1);
        }
        public static string GetComponentTypeName(Layer layer)
        {
            if (!IsComponentTypeRootLayer(layer))
                return string.Empty;
            var substringStartIdx = layer.Name.IndexOf(Settings.LayerDescriptionDelimiter);
            return layer.Name.Substring(substringStartIdx + 2);
        }
        public static string GetComponentTypeName(RhinoObject rhObj)
        {
            var layer = GetComponentTypeRootLayer(rhObj);
            return GetComponentTypeName(layer);
        }
        public static double GetComponentTypeLabelSize(Layer componentLayer)
        {
            var compObj = Objects.ObjectsByLayer(componentLayer).FirstOrDefault();
            if (IsComponentTypeRootLayer(componentLayer) && compObj?.Geometry is TextEntity)
                return (compObj.Geometry as TextEntity).TextHeight;
            return Settings.DimensionStyle.TextHeight;
        }
        //public static Settings GetComponentTypeSettings(Layer componentLayer, Settings settings)
        //{
        //    var compObj = Objects.ObjectsByLayer(componentLayer).FirstOrDefault();
        //    return GetComponentTypeSettings(compObj, settings);
        //}

        //public static Settings GetComponentTypeSettings(RhinoObject rhObj)
        //{
        //    var componentLayer = GetComponentTypeRootLayer(rhObj);
        //    var compTypeSettings = settings.ShallowCopy();
        //    if (IsComponentTypeRootLayer(componentLayer) && rhObj?.Geometry is TextEntity)
        //    {
        //        var txtObj = rhObj.Geometry as TextEntity;
        //        var dimStyleName = txtObj.DimensionStyle.Name ?? txtObj.ParentDimensionStyle.Name;
        //        compTypeSettings.DimensionStyleName = dimStyleName;
        //    }
        //    return compTypeSettings;
        //}

        public static IEnumerable<Layer> GetComponentLayers(IComponentBase component)
        {
            var componentLayers = new List<Layer>();
            var componentTypeRootLayer = GetComponentTypeRootLayer(component);
            if (componentTypeRootLayer == null)
                return componentLayers;
            TraverseChildLayers(componentTypeRootLayer.Id, ref componentLayers);
            return componentLayers;
        }
        public static IEnumerable<Layer> GetAncestorLayers(Layer layer, bool includeRoot = false)
        {
            var parentId = layer.ParentLayerId;
            var ancestorLayers = new List<Layer>();
            if (includeRoot)
                ancestorLayers.Add(layer);
            TraverseAncestorLayers(parentId, ref ancestorLayers);
            return ancestorLayers;
        }
        public static IEnumerable<Layer> GetChildLayers(Layer layer)
        {
            var rootId = layer?.Id ?? Guid.Empty;
            if (rootId == Guid.Empty)
                return Enumerable.Empty<Layer>();
            var childLayers = new List<Layer>();
            TraverseChildLayers(rootId, ref childLayers);
            return childLayers;
        }
        public static IEnumerable<Layer> GetChildLayers(int layerIdx)
        {
            return GetChildLayers(Settings.ActiveDoc.Layers.FindIndex(layerIdx));
        }
        public static IEnumerable<int> GetChildLayerIndices(int layerIdx)
        {
            return GetChildLayers(layerIdx).Select(layer => layer.Index);
        }

        public static Layer CreateRootLayer()
        {
            if (!FindRootLayer(out Layer rootLayer))
            {
                var rootLayerIdx = Settings.ActiveDoc.Layers.Add(Settings.RootLayerName, Settings.RootLayerColor);
                rootLayer = Settings.ActiveDoc.Layers.FindIndex(rootLayerIdx);
            }
            return rootLayer;
        }

        public static Layer CreateComponentTypeLayer(IComponentBase component)
        {

            if (!FindRootLayer(out Layer rootLayer))
                rootLayer = CreateRootLayer();
            var layerName = ComposeComponentTypeLayerName(component);
            var componentLayer = new Layer()
            {
                Id = Guid.NewGuid(),
                ParentLayerId = rootLayer.Id,
                Name = layerName,
                Color = component.LayerColor
            };
            var layerIdx = Settings.ActiveDoc.Layers.Add(componentLayer);
            componentLayer.Index = layerIdx;
            return componentLayer;
        }

        // Traverse Layers
        static Layer TraverseLayers(IComponentBase component, ref Queue<string> layerQueue, Guid parentLayerId, ILayerInfo layerInfo = null)
        {
            var layerName = ComposeComponentLayerName(component, layerQueue.Dequeue());
            var layerPath = ComposeFullLayerPath(layerName, parentLayerId);
            var docLayerIdx = Settings.ActiveDoc.Layers.FindByFullPath(layerPath, -1);
            var docLayer = Settings.ActiveDoc.Layers.FindIndex(docLayerIdx);
            if (docLayer == null && layerInfo != null)
            {
                docLayer = new Layer()
                {
                    Name = layerName,
                    Id = Guid.NewGuid(),
                    ParentLayerId = parentLayerId,
                    Color = layerInfo.LayerColor
                };
                docLayer.Index = Settings.ActiveDoc.Layers.Add(docLayer);
            }
            if (docLayer == null || !layerQueue.Any())
                return docLayer;
            return TraverseLayers(component, ref layerQueue, docLayer.Id, layerInfo);
        }
        static void TraverseAncestorLayers(Guid parentLayerId, ref List<Layer> ancestorLayers)
        {
            if (parentLayerId == Guid.Empty) return;
            var parentLayer = Settings.ActiveDoc.Layers.FindId(parentLayerId);
            ancestorLayers.Add(parentLayer);
            TraverseAncestorLayers(parentLayer.ParentLayerId, ref ancestorLayers);
        }
        static void TraverseChildLayers(Guid layerId, ref List<Layer> childLayers)
        {
            if (layerId == Guid.Empty) return;
            var layer = Settings.ActiveDoc.Layers.FindId(layerId);
            childLayers.Add(layer);
            var children = layer.GetChildren();
            if (children == null) return;
            foreach (var childLayer in children)
            {
                TraverseChildLayers(childLayer.Id, ref childLayers);
            }
        }
    }
}
