using D2P_Core.Components.Grasshopper;
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

        internal static RhinoDoc CreateHeadless(RhinoDoc doc)
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

        internal static Guid ReplaceInRhinoDoc(IComponentBase component)
        {
            var label = TextEntity.Create(component.Name, component.Plane, component.Settings.DimensionStyle, false, 0, 0);
            label.TextHeight = component.LabelSize;
            return component.ActiveDoc.Objects.Add(label);
        }

        internal static Guid AddToRhinoDoc(GrasshopperComponent component, RhinoDoc doc = null, bool replaceExisting = true)
        {
            doc = doc ?? RhinoDoc.ActiveDoc;
            var grpExists = doc.Groups.FindIndex(component.GroupIndex) != null;
            var grpIdx = component.IsVirtual || !grpExists || !replaceExisting ? Group.AddGroup(doc) : component.GroupIndex;
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


        internal static void UpdateComponentTypeLayerColors(IComponentBase component, RhinoDoc doc)
        {
            var rhLayer = Layers.GetComponentTypeRootLayer(component);
            if (rhLayer == null || rhLayer.Color == component.LayerColor)
                return;
            rhLayer.Color = component.LayerColor;
        }

        internal static void UpdateComponentSublayerColors(GrasshopperComponent component)
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
