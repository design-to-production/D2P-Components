using D2P_Core.Interfaces;
using Rhino;
using Rhino.Geometry;
using System;
using System.Linq;

namespace D2P_Core.Utility
{
    public static class RHDoc
    {
        public static void Purge(RhinoDoc doc)
        {
            foreach (var layer in doc.Layers)
            {
                if (doc.Objects.FindByLayer(layer).Length == 0)
                {
                    doc.Layers.Delete(layer, true);
                }
            }
        }

        public static RhinoDoc CreateHeadless(RhinoDoc doc)
        {
            var headlessDoc = RhinoDoc.CreateHeadless(doc.Name);
            headlessDoc.Layers.SetCurrentLayerIndex(0, true);
            foreach (var dimStyle in doc.DimStyles)
            {
                headlessDoc.DimStyles.Add(dimStyle, false);
            }
            foreach (var layer in doc.Layers)
            {
                headlessDoc.Layers.Add(layer);
            }
            headlessDoc.Layers.SetCurrentLayerIndex(0, true);
            return headlessDoc;
        }

        public static Guid AddToRhinoDoc(IComponent component, RhinoDoc doc = null, bool replaceExisting = false)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            var grpExists = doc.Groups.FindIndex(component.GroupIdx) != null;
            var grpIdx = component.IsVirtual || !grpExists || !replaceExisting ? Group.AddGroup(doc) : component.GroupIdx;
            Layers.CreateStagingLayers(component);

            if (replaceExisting)
                Objects.DeleteObjects(component);

            foreach (var keyVal in component.GeometryCollection)
            {
                var id = keyVal.Key;
                component.AttributeCollection.TryGetValue(id, out var attributes);
                attributes.RemoveFromAllGroups();
                attributes.AddToGroup(grpIdx);
                var layerIdx = component.StagingLayerCollection.Values
                    .Where(kv => kv.ContainsKey(id))
                    .Select(kv => kv[id])
                    .FirstOrDefault();
                if (layerIdx > 0)
                    attributes.LayerIndex = layerIdx;
                if (id == component.ID)
                {
                    if (!replaceExisting || doc.Objects.FindId(id) == null)
                        doc.Objects.AddText(keyVal.Value as TextEntity, attributes);
                    else doc.Objects.Replace(id, keyVal.Value as TextEntity);
                }
                else doc.Objects.Add(keyVal.Value, attributes);
            }
            UpdateComponentSublayerColors(component);
            return component.ID;
        }

        public static void UpdateComponentTypeLayerColors(IComponentType componentType, RhinoDoc doc)
        {
            var rhLayer = Layers.GetComponentTypeRootLayer(componentType, doc);
            if (rhLayer == null || rhLayer.Color == componentType.LayerColor)
                return;
            rhLayer.Color = componentType.LayerColor;
        }

        public static void UpdateComponentSublayerColors(IComponent component)
        {
            var layerInfos = component.StagingLayerCollection.Keys;
            foreach (var layerInfo in layerInfos)
            {
                var rhLayer = Layers.FindLayer(component, layerInfo.RawLayerName, out int _);
                if (rhLayer == null || rhLayer.Color == layerInfo.LayerColor || Layers.IsComponentTypeTopLayer(rhLayer, component.Settings))
                    continue;
                rhLayer.Color = layerInfo.LayerColor;
            }
        }
    }
}
