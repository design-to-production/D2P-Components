using D2P_Core.Components;
using D2P_Core.Components.Grasshopper;
using D2P_Core.Interfaces;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace D2P_Core.Utility
{
    using SortedLayerColletcion = SortedDictionary<ILayerInfo, Dictionary<Guid, int>>;

    public static class Layers
    {

        public static Layer CreateLayer(IComponentBase component, IMember member)
        {
            if (!GetRootLayerID(component, out Guid rootLayerId))
                rootLayerId = CreateRootLayer(component).Id;
            var layerName = ComposeComponentTypeLayerName(component);
            var componentLayer = new Layer() { Id = Guid.NewGuid(), ParentLayerId = rootLayerId, Name = layerName, Color = component.LayerColor };
            var layerIdx = component.ActiveDoc.Layers.Add(componentLayer);
            componentLayer.Index = layerIdx;
            return componentLayer;
        }

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
        public static Layer FindLayer(int layerIndex, RhinoDoc doc = null)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            return doc.Layers.FindIndex(layerIndex);
        }
        public static int FindLayerIndexByFullPath(IComponentBase component, string rawLayerName, char layerNameDeilimiter = ':') => FindLayerByFullPath(component, rawLayerName, layerNameDeilimiter)?.Index ?? -1;
        public static Layer FindLayerByFullPath(IComponentBase component, string rawLayerName, char layerNameDeilimiter = ':')
        {
            var layerNames = new Queue<string>(rawLayerName.Split(layerNameDeilimiter).Where(s => !string.IsNullOrEmpty(s)));
            if (!layerNames.Any())
                return null;

            var rootLayerID = GetComponentLayerID(component);
            return TraverseLayers(component, ref layerNames, rootLayerID);
        }

        public static IEnumerable<Layer> FindAllExistentComponentTypeRootLayers(Settings settings, RhinoDoc doc = null)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            var rootLayer = GetRootLayer(doc, settings.RootLayerName);
            var childLayers = GetChildLayers(rootLayer, doc);
            return childLayers.Where(layer => IsComponentTypeTopLayer(layer, settings));
        }
        public static Layer FindComponentLayerByType(string type, string rootLayerName)
        {
            var doc = RhinoDoc.ActiveDoc;
            var layerNames = doc.Layers.Where(l => !l.IsReference).Select(l => l.Name);
            var componentLayerName = ComposeComponentTypeLayerName(type, "");
            componentLayerName = layerNames.FirstOrDefault(name => name.StartsWith(componentLayerName));
            if (componentLayerName == null)
                return null;
            return FindLayerByName(doc, componentLayerName, rootLayerName);
        }
        public static Layer FindLayerByName(RhinoDoc doc, string layerName, string rootLayerName, bool includeReferenced = false)
        {
            bool condition(Layer layer) => includeReferenced || !layer.IsReference;
            var layerFound = doc.Layers.FirstOrDefault(l => condition(l) && l.Name == layerName && l.FullPath.StartsWith(rootLayerName));
            if (layerFound == null)
                return null;
            return layerFound;
        }

        public static Layer CreateLayer(IMember member)
        {
            var component = member.Component;
            var layerName = ComposeMemberLayerName(member);
            var layerSegments = new Queue<string>(layerName
                .Split(component.Settings.LayerNameDelimiter)
                .Where(s => !string.IsNullOrEmpty(s))
            );

            var componentLayer = GetComponentTypeRootLayer(component);
            if (componentLayer == null || componentLayer.Index == 0)
                componentLayer = CreateComponentTypeLayer(component);

            return TraverseLayers(component, ref layerSegments, componentLayer.Id, member.LayerInfo);
        }

        private static string ComposeMemberLayerName(IMember member)
        {
            if (member == null)
                return string.Empty;

            var layerName = member.LayerInfo?.RawLayerName ?? string.Empty;
            if (member.Parent == null)
                return layerName;

            var parentLayerName = ComposeMemberLayerName(member.Parent);
            var layerDelimiter = member.Component.Settings.LayerNameDelimiter;

            if (string.IsNullOrEmpty(parentLayerName))
                return layerName;

            return $"{parentLayerName}{layerDelimiter}{layerDelimiter}{layerName}";
        }


        public static int[] CreateStagingLayers(GrasshopperComponent component)
        {
            var createdLayerIndices = new List<int>();
            var componentLayer = GetComponentTypeRootLayer(component);
            if (componentLayer == null || componentLayer.Index == 0)
                componentLayer = CreateComponentTypeLayer(component);

            var sortedStagingLayers = new SortedLayerColletcion(component.StagingLayerCollection, new LayerInfoComparer(component));
            foreach (var kv in sortedStagingLayers)
            {
                var rawLayerName = kv.Key.RawLayerName;
                var layerNames = new Queue<string>(rawLayerName.Split(component.Settings.LayerNameDelimiter).Where(s => !string.IsNullOrEmpty(s)));
                var layer = layerNames.Any() && component.IsInitialized ? TraverseLayers(component, ref layerNames, componentLayer.Id, kv.Key) : componentLayer;

                var geometryIds = new List<Guid>(kv.Value.Keys);
                foreach (var geometryId in geometryIds)
                {
                    kv.Value[geometryId] = layer.Index;
                    createdLayerIndices.Add(layer.Index);
                }
            }
            return createdLayerIndices.ToArray();
        }

        public static bool IsComponentTypeTopLayer(IComponentBase component, string layerName)
        {
            return layerName.Split(component.Settings.LayerDescriptionDelimiter).FirstOrDefault() == component.TypeId;
        }
        public static bool IsComponentTypeTopLayer(Layer layer, Settings settings)
        {
            if (layer == null)
                return false;
            var regex = new Regex($".*(?<!/s){settings.LayerDescriptionDelimiter}(?<!/s).*");
            return regex.IsMatch(layer.Name);
        }
        public static string ComposeComponentLayerName(IComponentBase component, string rawLayerName)
        {
            return $"{component.TypeId}{component.Settings.LayerDelimiter}{rawLayerName.Split(component.Settings.LayerNameDelimiter).LastOrDefault()}";
        }
        public static string ComposeComponentLayerName(IComponentBase component, IMember member)
        {
            var typeId = component.TypeId;
            var layerDelimiter = component.Settings.LayerDelimiter;
            var layerName = member.LayerInfo.RawLayerName
                .Split(component.Settings.LayerNameDelimiter)
                .LastOrDefault();
            return $"{typeId}{layerDelimiter}{layerName}";
        }
        public static string ComposeComponentTypeLayerName(IComponentType componentType) => ComposeComponentTypeLayerName(componentType.TypeId, componentType.TypeName, componentType.Settings.LayerDescriptionDelimiter);
        public static string ComposeComponentTypeLayerName(string type, string description, char layerDescriptionDelimiter = '-')
        {
            return $"{type} {layerDescriptionDelimiter} {description}";
        }
        public static string ComposeFullLayerPath(string layerName, Guid parentLayerId, RhinoDoc doc)
        {
            var parentLayerPath = doc.Layers.FindId(parentLayerId)?.FullPath;
            return parentLayerPath != null ? $"{parentLayerPath}::{layerName}" : string.Empty;
        }
        public static string DecomposeLayerName(IComponentBase component, string layerName)
        {
            if (IsComponentTypeTopLayer(component, layerName))
            {
                var idx = layerName.IndexOf(component.Settings.LayerDescriptionDelimiter);
                return layerName.Substring(idx);
            }
            return layerName.Substring(layerName.IndexOf(component.Settings.LayerDelimiter));
        }

        public static Layer GetRootLayer(RhinoDoc doc, string rootLayerName) => FindLayerByName(doc, rootLayerName, rootLayerName);
        public static bool GetRootLayerID(IComponentBase component, out Guid rootLayerId)
        {
            var rootLayer = GetRootLayer(component.ActiveDoc, component.Settings.RootLayerName);
            rootLayerId = rootLayer?.Id ?? Guid.Empty;
            return rootLayer != null;
        }
        public static Layer GetComponentTypeRootLayer(RhinoObject obj, Settings settings, RhinoDoc doc = null)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            var objLayer = FindLayer(obj.Attributes.LayerIndex);
            if (IsComponentTypeTopLayer(objLayer, settings))
                return objLayer;

            var componentTypeAncestorLayers = new List<Layer>();
            TraverseAncestorLayers(objLayer.Id, doc, ref componentTypeAncestorLayers);

            return componentTypeAncestorLayers.Find(l => IsComponentTypeTopLayer(l, settings));
        }
        public static Layer GetComponentTypeRootLayer(IComponentBase component)
        {
            var componentTypeRootLayerName = ComposeComponentTypeLayerName(component);
            return FindLayerByName(component.ActiveDoc, componentTypeRootLayerName, component.Settings.RootLayerName);
        }

        public static Guid GetComponentLayerID(IComponentBase component) => GetComponentTypeRootLayer(component)?.Id ?? Guid.Empty;

        public static string GetComponentTypeID(Layer componentLayer, Settings settings)
        {
            if (!IsComponentTypeTopLayer(componentLayer, settings))
                return string.Empty;
            var substringStartIdx = componentLayer.Name.IndexOf(settings.LayerDescriptionDelimiter);
            return componentLayer.Name.Substring(0, substringStartIdx - 1);
        }
        public static string GetComponentTypeName(Layer componentLayer, Settings settings)
        {
            if (!IsComponentTypeTopLayer(componentLayer, settings))
                return string.Empty;
            var substringStartIdx = componentLayer.Name.IndexOf(settings.LayerDescriptionDelimiter);
            return componentLayer.Name.Substring(substringStartIdx + 2);
        }
        public static string GetComponentTypeName(RhinoObject rhObj, Settings settings)
        {
            var layer = GetComponentTypeRootLayer(rhObj, settings);
            return GetComponentTypeName(layer, settings);
        }
        public static double GetComponentTypeLabelSize(Layer componentLayer, Settings settings)
        {
            var compObj = Objects.ObjectsByLayer(componentLayer).FirstOrDefault();
            if (IsComponentTypeTopLayer(componentLayer, settings) && compObj?.Geometry is TextEntity)
                return (compObj.Geometry as TextEntity).TextHeight;
            return settings.DimensionStyle.TextHeight;
        }
        public static Settings GetComponentTypeSettings(Layer componentLayer, Settings settings)
        {
            var compObj = Objects.ObjectsByLayer(componentLayer).FirstOrDefault();
            return GetComponentTypeSettings(compObj, settings);
        }
        public static Settings GetComponentTypeSettings(RhinoObject rhObj, Settings settings)
        {
            var componentLayer = GetComponentTypeRootLayer(rhObj, settings);
            var compTypeSettings = settings.ShallowCopy();
            if (IsComponentTypeTopLayer(componentLayer, settings) && rhObj?.Geometry is TextEntity)
            {
                var txtObj = rhObj.Geometry as TextEntity;
                var dimStyleName = txtObj.DimensionStyle.Name ?? txtObj.ParentDimensionStyle.Name;
                compTypeSettings.DimensionStyleName = dimStyleName;
            }
            return compTypeSettings;
        }

        public static IEnumerable<Layer> GetComponentLayers(IComponentBase component)
        {
            var doc = component.ActiveDoc;
            var componentLayers = new List<Layer>();
            var componentTypeRootLayer = GetComponentTypeRootLayer(component);
            if (componentTypeRootLayer == null)
                return componentLayers;
            TraverseChildLayers(componentTypeRootLayer.Id, doc, ref componentLayers);
            return componentLayers;
        }
        public static IEnumerable<Layer> GetAncestorLayers(Layer layer, RhinoDoc doc = null, bool includeRoot = false)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            var parentId = layer.ParentLayerId;
            var ancestorLayers = new List<Layer>();
            if (includeRoot)
                ancestorLayers.Add(layer);
            TraverseAncestorLayers(parentId, doc, ref ancestorLayers);
            return ancestorLayers;
        }
        public static IEnumerable<Layer> GetChildLayers(Layer layer, RhinoDoc doc = null)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            var rootId = layer?.Id ?? Guid.Empty;
            if (rootId == Guid.Empty)
                return Enumerable.Empty<Layer>();
            var childLayers = new List<Layer>();
            TraverseChildLayers(rootId, doc, ref childLayers);
            return childLayers;
        }
        public static IEnumerable<Layer> GetChildLayers(int layerIdx, RhinoDoc doc = null) => GetChildLayers(doc.Layers.FindIndex(layerIdx), doc);
        public static IEnumerable<int> GetChildLayerIndices(int layerIdx, RhinoDoc doc = null) => GetChildLayers(layerIdx, doc).Select(layer => layer.Index);
        public static Layer CreateRootLayer(IComponentBase component) => CreateRootLayer(component.Settings.RootLayerName, component.Settings.RootLayerColor, component.ActiveDoc);
        public static Layer CreateRootLayer(string rootLayerName, Color rootLayerColor, RhinoDoc doc = null)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            var rootLayer = GetRootLayer(doc, rootLayerName);
            if (rootLayer == null)
            {
                var rootLayerIdx = doc.Layers.Add(rootLayerName, rootLayerColor);
                rootLayer = doc.Layers.FindIndex(rootLayerIdx);
            }
            return rootLayer;
        }
        public static Layer CreateComponentTypeLayer(IComponentBase component)
        {
            if (!GetRootLayerID(component, out Guid rootLayerId))
                rootLayerId = CreateRootLayer(component).Id;
            var layerName = ComposeComponentTypeLayerName(component);
            var componentLayer = new Layer() { Id = Guid.NewGuid(), ParentLayerId = rootLayerId, Name = layerName, Color = component.LayerColor };
            var layerIdx = component.ActiveDoc.Layers.Add(componentLayer);
            componentLayer.Index = layerIdx;
            return componentLayer;
        }

        static Layer TraverseLayers(IComponentBase component, ref Queue<string> layerQueue, Guid parentLayerId, ILayerInfo layerInfo = null)
        {
            var doc = component.ActiveDoc;
            var layerName = ComposeComponentLayerName(component, layerQueue.Dequeue());
            var layerPath = ComposeFullLayerPath(layerName, parentLayerId, doc);
            var docLayerIdx = doc.Layers.FindByFullPath(layerPath, -1);
            var docLayer = doc.Layers.FindIndex(docLayerIdx);
            if (docLayer == null && layerInfo != null)
            {
                docLayer = new Layer()
                {
                    Name = layerName,
                    Id = Guid.NewGuid(),
                    ParentLayerId = parentLayerId,
                    Color = layerInfo.LayerColor
                };
                docLayer.Index = doc.Layers.Add(docLayer);
            }
            if (docLayer == null || !layerQueue.Any())
                return docLayer;
            return TraverseLayers(component, ref layerQueue, docLayer.Id, layerInfo);
        }
        static void TraverseAncestorLayers(Guid parentLayerId, RhinoDoc doc, ref List<Layer> ancestorLayers)
        {
            if (parentLayerId == Guid.Empty) return;
            var parentLayer = doc.Layers.FindId(parentLayerId);
            ancestorLayers.Add(parentLayer);
            TraverseAncestorLayers(parentLayer.ParentLayerId, doc, ref ancestorLayers);
        }
        static void TraverseChildLayers(Guid layerId, RhinoDoc doc, ref List<Layer> childLayers)
        {
            if (layerId == Guid.Empty) return;
            var layer = doc.Layers.FindId(layerId);
            childLayers.Add(layer);
            var children = layer.GetChildren();
            if (children == null) return;
            foreach (var childLayer in children)
            {
                TraverseChildLayers(childLayer.Id, doc, ref childLayers);
            }
        }
    }
}
