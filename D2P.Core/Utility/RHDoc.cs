using D2P.Core.Interfaces;
using Rhino;
using System.Collections.Generic;
using System.Linq;

namespace D2P.Core.Utility {
    public static class RHDoc {
        public static void Purge(RhinoDoc doc)
        {
            foreach (var layer in doc.Layers) {
                if (doc.Objects.FindByLayer(layer).Length == 0) {
                    doc.Layers.Delete(layer, true);
                }
            }
        }

        internal static RhinoDoc CreateHeadless(RhinoDoc doc)
        {
            var headlessDoc = RhinoDoc.CreateHeadless(doc.Name);
            headlessDoc.Layers.SetCurrentLayerIndex(0, true);
            foreach (var dimStyle in doc.DimStyles) {
                headlessDoc.DimStyles.Add(dimStyle, false);
            }
            foreach (var layer in doc.Layers) {
                headlessDoc.Layers.Add(layer);
            }
            headlessDoc.Layers.SetCurrentLayerIndex(0, true);
            return headlessDoc;
        }

        public static void UpdateComponentLayerColors(IEnumerable<IComponentBase> components)
        {
            foreach (var compGrp in components.GroupBy(c => c.TypeId)) {
                var component = compGrp.First();
                UpdateComponentTypeLayerColors(component);
                UpdateComponentSublayerColors(component);
            }
        }

        static void UpdateComponentTypeLayerColors(IComponentBase component)
        {
            var rhLayer = Layers.FindComponentTypeRootLayer(component);
            if (rhLayer == null || rhLayer.Color == component.LayerColor)
                return;
            rhLayer.Color = component.LayerColor;
        }

        static void UpdateComponentSublayerColors(IComponentBase component)
        {
            foreach (var member in component.AllMembers) {
                var rhLayer = Layers.FindLayer(member, out int _);
                if (rhLayer == null ||
                    member?.LayerInfo == null ||
                    rhLayer.Color == member.LayerInfo.LayerColor ||
                    Layers.IsComponentTypeRootLayer(rhLayer))
                    continue;
                rhLayer.Color = member.LayerInfo.LayerColor;
            }
        }
    }
}
