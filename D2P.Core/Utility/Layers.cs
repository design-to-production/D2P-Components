using D2P.Core.Components;
using D2P.Core.Interfaces;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace D2P.Core.Utility {
    public static class Layers {
        // Create Layer
        public static Layer CreateLayer(IComponentBase component)
        {
            if (!FindRootLayer(out Layer rootLayer))
                rootLayer = CreateRootLayer();
            var layerName = ComposeComponentTypeLayerName(component);
            var componentLayer = new Layer() {
                Id = Guid.NewGuid(),
                ParentLayerId = rootLayer.Id,
                Name = layerName,
                Color = component.LayerColor
            };
            var layerIdx = Settings.ActiveDoc.Layers.Add(componentLayer);
            componentLayer.Index = layerIdx;
            return componentLayer;
        }
        public static Layer CreateLayer(IMember member)
        {
            var layerName = ComposeMemberLayerName(member);
            var layerSegments = new Queue<string>(layerName
                .Split(Settings.LayerNameDelimiter)
                .Where(s => !string.IsNullOrEmpty(s))
            );

            var componentLayer = FindComponentTypeRootLayer(member.Component);
            if (componentLayer == null || componentLayer.Index == 0)
                componentLayer = CreateComponentTypeLayer(member.Component);

            return TraverseLayers(member, ref layerSegments, componentLayer.Id);
        }
        public static Layer CreateRootLayer()
        {
            if (!FindRootLayer(out Layer rootLayer)) {
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
            var componentLayer = new Layer() {
                Id = Guid.NewGuid(),
                ParentLayerId = rootLayer.Id,
                Name = layerName,
                Color = component.LayerColor
            };
            var layerIdx = Settings.ActiveDoc.Layers.Add(componentLayer);
            componentLayer.Index = layerIdx;
            return componentLayer;
        }

        // Find Layers
        public static bool FindRootLayer(out Layer rootLayer)
        {
            rootLayer = FindLayerByName(Settings.RootLayerName);
            return rootLayer != null;
        }

        public static Layer FindLayer(IMember member)
        {
            var layer = FindLayer(member, out int layersFound);
            return layersFound != 1 ? null : layer;
        }
        public static Layer FindLayer(IMember member, out int layersFound)
        {
            if (member?.Component == null) {
                layersFound = 0;
                return null;
            }
            string layerName = string.Empty;
            if (string.IsNullOrEmpty(member.LayerInfo.RawLayerName))
                layerName = ComposeComponentTypeLayerName(member?.Component);
            else layerName = ComposeFullLayerPath(member);
            var componentLayers = GetComponentLayers(member.Component);
            var matchedLayers = componentLayers
                .Where(l => !l.IsReference && l.FullPath == layerName); // TODO: Compare FULL LayerName !!!
            layersFound = matchedLayers.Count();
            if (matchedLayers.Count() > 1 || !matchedLayers.Any())
                return null;
            return matchedLayers.First();
        }
        public static Layer FindLayer(int layerIndex)
        {
            return Settings.ActiveDoc.Layers.FindIndex(layerIndex);
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
                .FirstOrDefault(l => condition(l) &&
                l.Name == layerName &&
                l.FullPath.StartsWith(Settings.RootLayerName));
            if (layerFound == null)
                return null;
            return layerFound;
        }

        public static IEnumerable<Layer> FindComponentTypeRootLayers()
        {
            if (!FindRootLayer(out Layer rootLayer))
                return Enumerable.Empty<Layer>();
            var childLayers = GetChildLayers(rootLayer);
            return childLayers.Where(layer => IsComponentTypeRootLayer(layer));
        }
        public static Layer FindComponentTypeRootLayer(RhinoObject obj)
        {
            var objLayer = FindLayer(obj.Attributes.LayerIndex);
            if (IsComponentTypeRootLayer(objLayer))
                return objLayer;

            var componentTypeAncestorLayers = new List<Layer>();
            TraverseAncestorLayers(objLayer.Id, ref componentTypeAncestorLayers);

            return componentTypeAncestorLayers.Find(l => IsComponentTypeRootLayer(l));
        }
        public static Layer FindComponentTypeRootLayer(IComponentBase component)
        {
            var componentTypeRootLayerName = ComposeComponentTypeLayerName(component);
            return FindLayerByName(componentTypeRootLayerName);
        }

        // Layer Validation
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

        // Compose Layer Names
        public static string ComposeComponentLayerName(IComponentBase component, string rawLayerName)
        {
            return $"{component.TypeId}{Settings.LayerDelimiter}{rawLayerName.Split(Settings.LayerNameDelimiter).LastOrDefault()}";
        }

        public static string ComposeComponentTypeLayerName(IComponentType componentType)
        {
            return ComposeComponentTypeLayerName(componentType.TypeId, componentType.TypeName);
        }
        public static string ComposeComponentTypeLayerName(string type, string description)
        {
            return $"{type} {Settings.LayerDescriptionDelimiter} {description}";
        }

        public static string ComposeFullLayerPath(IMember member)
        {
            var layerPath = string.Empty;
            composeLayerPath(member, ref layerPath);
            var typeLayer = FindComponentTypeRootLayer(member.Component);
            var typeLayerName = typeLayer?.FullPath;
            if (typeLayer == null) {
                var composedTypeLayerName = ComposeComponentTypeLayerName(member.Component);
                typeLayerName = $"{Settings.RootLayerName}::{composedTypeLayerName}";
            }
            return $"{typeLayerName}::{layerPath}";
        }
        static void composeLayerPath(IMember member, ref string layerPath)
        {
            var layerNames = member.LayerInfo.RawLayerName.Split(':')
                .Where(name => !string.IsNullOrEmpty(name))
                .Select(name => name.Replace($"{member.Component.TypeId}{Settings.LayerDelimiter}", ""))
                .Select(name => $"{member.Component.TypeId}{Settings.LayerDelimiter}{name}");
            var result = string.Join("::", layerNames);

            layerPath = layerPath.Insert(0, result);
            if (member.ParentMember == null)
                return;
            layerPath = layerPath.Insert(0, "::");
            composeLayerPath(member.ParentMember, ref layerPath);
        }

        public static string DecomposeLayerName(IComponentBase component, string layerName)
        {
            if (IsComponentTypeRootLayer(component, layerName)) {
                var idx = layerName.IndexOf(Settings.LayerDescriptionDelimiter);
                return layerName.Substring(idx);
            }
            return layerName.Substring(layerName.IndexOf(Settings.LayerDelimiter));
        }

        //DROP !!
        public static string ComposeMemberLayerName(IMember member)
        {
            if (member == null)
                return string.Empty;

            var layerName = member.LayerInfo?.RawLayerName ?? string.Empty;
            if (member.ParentMember == null)
                return layerName;

            var parentLayerName = ComposeMemberLayerName(member.ParentMember);
            if (string.IsNullOrEmpty(parentLayerName))
                return layerName;

            var layerDelimiter = Settings.LayerNameDelimiter;
            return $"{parentLayerName}{layerDelimiter}{layerDelimiter}{layerName}";
        }

        // Get Layer Infos
        public static LayerInfo GetLayerInfo(Layer layer)
        {
            return new LayerInfo(GetRawLayerName(layer), layer.Color);
        }
        public static string GetRawLayerName(Layer layer)
        {
            return layer.Name.Split(Settings.LayerDelimiter).LastOrDefault();
        }

        // Get Component Infos
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
            var layer = FindComponentTypeRootLayer(rhObj);
            return GetComponentTypeName(layer);
        }
        public static double GetComponentTypeLabelSize(Layer componentLayer)
        {
            var compObj = Objects.ObjectsByLayer(componentLayer).FirstOrDefault();
            if (IsComponentTypeRootLayer(componentLayer) && compObj?.Geometry is TextEntity)
                return (compObj.Geometry as TextEntity).TextHeight;
            return Settings.DimensionStyle.TextHeight;
        }

        // Get Layers
        public static IEnumerable<Layer> GetComponentLayers(IComponentBase component)
        {
            var componentLayers = new List<Layer>();
            var componentTypeRootLayer = FindComponentTypeRootLayer(component);
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
            return childLayers.Where(l => l.Id != rootId);
        }
        public static IEnumerable<Layer> GetChildLayers(int layerIdx)
        {
            return GetChildLayers(Settings.ActiveDoc.Layers.FindIndex(layerIdx));
        }
        public static IEnumerable<int> GetChildLayerIndices(int layerIdx)
        {
            return GetChildLayers(layerIdx).Select(layer => layer.Index);
        }

        // Traverse Layers
        static Layer TraverseLayers(IMember member, ref Queue<string> layerQueue, Guid parentLayerId)
        {
            if (string.IsNullOrEmpty(member.LayerInfo.RawLayerName))
                return FindComponentTypeRootLayer(member.Component);

            var parentLayer = Settings.ActiveDoc.Layers.FindId(parentLayerId);
            if (parentLayer == null) return null;

            var layerName = ComposeComponentLayerName(member.Component, layerQueue.Dequeue());
            var layerPath = $"{parentLayer.FullPath}::{layerName}";
            var docLayerIdx = Settings.ActiveDoc.Layers.FindByFullPath(layerPath, -1);
            var docLayer = Settings.ActiveDoc.Layers.FindIndex(docLayerIdx);
            if (docLayer == null && member.LayerInfo != null) {
                docLayer = new Layer() {
                    Name = layerName,
                    Id = Guid.NewGuid(),
                    ParentLayerId = parentLayerId,
                    Color = member.LayerInfo.LayerColor
                };
                docLayer.Index = Settings.ActiveDoc.Layers.Add(docLayer);
            }
            if (docLayer == null || !layerQueue.Any())
                return docLayer;
            return TraverseLayers(member, ref layerQueue, docLayer.Id);
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
            foreach (var childLayer in children) {
                TraverseChildLayers(childLayer.Id, ref childLayers);
            }
        }
    }
}
