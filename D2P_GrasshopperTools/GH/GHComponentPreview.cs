using D2P_Core.Interfaces;
using Grasshopper.Kernel;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace D2P_GrasshopperTools.GH
{
    public abstract class GHComponentPreview : GH_Component
    {

        protected BoundingBox _box;
        protected List<IComponent> _components = new List<IComponent>();

        protected GHComponentPreview(string name, string shortname, string description, string category, string subcategory)
        : base(name, shortname, description, category, subcategory)
        { }

        public override BoundingBox ClippingBox => _box;

        protected override void BeforeSolveInstance()
        {
            _components.Clear();
        }

        protected override void AfterSolveInstance()
        {
            _box = BoundingBox.Empty;
            foreach (var geo in _components.SelectMany(c => c?.Geometry))
            {
                if (geo == null)
                    continue;
                var bbox = geo.GetBoundingBox(false);
                _box.Union(bbox);
            }
            _box.Transform(Transform.Scale(_box.Center, 1.2));
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (Hidden || Locked)
                return;

            var selected = Attributes.Selected;
            var fill = selected ? args.WireColour_Selected : args.WireColour;
            var edge = fill.GetBrightness() < 0.3 ? Color.White : Color.Black;
            var renderTypes = new[] { ObjectType.Annotation, ObjectType.Curve, ObjectType.TextDot };
            var geometry = _components.Where(c => c != null).SelectMany(comp => comp.Geometry);
            geometry = geometry.Where(geo => geo != null && renderTypes.Contains(geo.ObjectType));
            foreach (var geo in geometry)
            {
                switch (geo.ObjectType)
                {
                    case ObjectType.Annotation:
                        args.Display.DrawAnnotation(geo as AnnotationBase, fill);
                        break;
                    case ObjectType.Curve:
                        args.Display.DrawCurve(geo as Curve, fill);
                        break;
                    case ObjectType.TextDot:
                        args.Display.DrawDot(geo as TextDot, fill, edge, edge);
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine(geo.ObjectType);
                        break;
                }
            }
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (Hidden || Locked)
                return;

            var selected = Attributes.Selected;
            var fill = selected ? args.ShadeMaterial_Selected : args.ShadeMaterial;
            var renderTypes = new[] { ObjectType.Brep, ObjectType.Extrusion };
            var geometry = _components.Where(c => c != null).SelectMany(comp => comp.Geometry);
            geometry = geometry.Where(geo => geo != null && renderTypes.Contains(geo.ObjectType));
            foreach (var geo in geometry)
            {
                switch (geo.ObjectType)
                {
                    case ObjectType.Brep:
                        args.Display.DrawBrepShaded(geo as Brep, fill);
                        break;
                    case ObjectType.Extrusion:
                        args.Display.DrawBrepShaded((geo as Extrusion).ToBrep(), fill);
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine(geo.ObjectType);
                        break;
                }
            }
        }
    }
}