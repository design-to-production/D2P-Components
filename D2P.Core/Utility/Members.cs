using D2P.Core.Components;
using D2P.Core.Components.Member;
using D2P.Core.Interfaces;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P.Core.Utility {
    public static class Members {
        public static IEnumerable<IMember> FindMembers(IComponentBase component)
        {
            IEnumerable<IMember> createMembers(LayerNode node, IMember parent = null)
            {
                foreach (var n in node.Children) {
                    var localPath = n.Name == "__root__" ? "" : n.Name;
                    var layerInfo = new LayerInfo(localPath, n.Color);
                    var member = new MemberGeo(component, layerInfo);
                    var childMembers = createMembers(n, member);
                    member.SetMembers(childMembers);
                    member.ParentMember = parent;
                    yield return member;
                }
            }
            var tree = LayerTreeBuilder.BuildTree(component);
            return createMembers(tree);
        }

        public static IEnumerable<GeometryBase> GetAllMemberGeometries(IComponentBase component)
        {
            return component.AllMembers.SelectMany(m => GetAllMemberGeometries(m));
        }
        public static IList<GeometryBase> GetAllMemberGeometries(IMember member)
        {
            var geometries = new List<GeometryBase>();
            geometries.AddRange(member.Geometry);
            foreach (var child in member.AllMembers) {
                if (!child.Geometry.Any()) continue;
                var childGeo = GetAllMemberGeometries(child);
                geometries.AddRange(childGeo);
            }
            return geometries;
        }

        public static IMember MemberFromLayer(IComponentBase component, Layer layer)
        {
            var name = Guid.NewGuid().ToString();
            var rawLayerName = Layers.GetRawLayerName(layer);
            var layerInfo = new LayerInfo(rawLayerName, layer.Color);
            var member = new MemberGeo(component, layerInfo);
            var geometry = Objects.GeometryByLayer(component, layer.Index);
            member.SetObjects(geometry);
            return member;
        }

        public static bool IsComponentLabel(IComponentBase component, IMember member)
        {
            var label = member.Geometry
                .OfType<TextEntity>()
                .FirstOrDefault();
            return label != null && label?.PlainText == component.ShortName;
        }
    }
}
