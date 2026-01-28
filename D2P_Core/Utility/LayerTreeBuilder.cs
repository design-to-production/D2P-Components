namespace D2P_Core.Utility {
    using D2P_Core.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    public sealed class LayerNode {
        private readonly Dictionary<string, LayerNode> _childIndex = new Dictionary<string, LayerNode>(StringComparer.Ordinal);

        public string Name { get; }
        public string FullPath { get; }
        public Color Color { get; set; }
        public LayerNode Parent { get; } = null;
        public List<LayerNode> Children { get; } = new List<LayerNode>();

        public LayerNode(string name, string fullPath, LayerNode parent)
        {
            Name = name;
            FullPath = fullPath;
            Parent = parent;
        }

        public LayerNode GetOrAddChild(string childName)
        {
            if (_childIndex.TryGetValue(childName, out var existing))
                return existing;

            var childFullPath = string.IsNullOrEmpty(FullPath)
                ? childName
                : $"{FullPath}::{childName}";

            var child = new LayerNode(childName, childFullPath, this);
            _childIndex[childName] = child;
            Children.Add(child);
            return child;
        }
    }

    public static class LayerTreeBuilder {
        public static LayerNode BuildTree(IComponentBase component)
        {
            var componentLayers = Layers.GetComponentLayers(component);
            var baseLayer = Layers.FindComponentTypeRootLayer(component);

            var layerSegments = componentLayers
                .Select(l => l.FullPath.Replace(baseLayer.FullPath, ""))
                .Where(s => !string.IsNullOrEmpty(s));

            if (layerSegments == null) throw new ArgumentNullException(nameof(layerSegments));

            var root = new LayerNode("__root__", fullPath: "", parent: null);
            foreach (var layer in componentLayers) {
                var raw = layer.FullPath
                    .Replace($"{baseLayer.FullPath}::", "")
                    .Replace($"{baseLayer.FullPath}", "")
                    .Replace($"{component.TypeId}_", "");

                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                var parts = raw
                    .Split(new[] { "::" }, StringSplitOptions.None)
                    .Where(p => !string.IsNullOrEmpty(p));

                if (parts.Count() == 0)
                    continue;

                var current = root;
                current.Color = layer.Color;
                foreach (var part in parts) {
                    current = current.GetOrAddChild(part);
                }

            }
            SortRecursive(root);
            return root;
        }

        private static void SortRecursive(LayerNode node)
        {
            node.Children.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            foreach (var c in node.Children)
                SortRecursive(c);
        }
    }

}
