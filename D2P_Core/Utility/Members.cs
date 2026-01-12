using D2P_Core.Components;
using D2P_Core.Components.Member;
using D2P_Core.Interfaces;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P_Core.Utility {
    public static class Members {
        public static IList<IMember> FindMembers(IComponentBase component)
        {
            var componentLayers = Layers.GetComponentLayers(component);
            return componentLayers.Select(layer => MemberFromLayer(component, layer)).ToList();
        }

        public static IMember MemberFromLayer(IComponentBase component, Layer layer)
        {
            var name = Guid.NewGuid().ToString();
            var rawLayerName = Layers.GetRawLayerName(layer);
            var layerInfo = new LayerInfo(rawLayerName, layer.Color);
            var member = new MemberGeo(component, layerInfo);
            var geometry = Objects.GeometryByLayer(component, layer.Index);
            member.SetGeometry(geometry);
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
