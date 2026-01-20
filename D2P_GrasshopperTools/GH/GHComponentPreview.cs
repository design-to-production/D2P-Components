using D2P_Core.Interfaces;
using Grasshopper.Kernel;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace D2P_GrasshopperTools.GH {
    public abstract class GHComponentPreview : GHComponentBase {

        protected BoundingBox _box;
        protected List<IComponentBase> _components = new List<IComponentBase>();
        private List<GeometryBase> _geometries = new List<GeometryBase>();

        public override BoundingBox ClippingBox => _box;

        protected GHComponentPreview(string name, string shortname, string description, string category, string subcategory)
        : base(name, shortname, description, category, subcategory)
        { }

        protected override void BeforeSolveInstance()
        {
            base.BeforeSolveInstance();
            _components.Clear();
            _geometries.Clear();
            _box = BoundingBox.Empty;
        }

        protected override void AfterSolveInstance()
        {
            base.AfterSolveInstance();
            _geometries = _components
              .Where(c => c != null)
              .SelectMany(comp => comp.Geometry)
              .Where(geo => geo != null)
              .ToList();
            _box = ComputeClippingBox(_geometries);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (Hidden || Locked)
                return;

            var selected = Attributes.Selected;
            var fill = selected ? args.WireColour_Selected : args.WireColour;
            var edge = fill.GetBrightness() < 0.3 ? Color.White : Color.Black;
            //var renderTypes = new[] { ObjectType.Annotation, ObjectType.Curve, ObjectType.TextDot };

            foreach (var geo in _geometries) {
                switch (geo.ObjectType) {
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
            //var renderTypes = new[] { ObjectType.Brep, ObjectType.Extrusion };

            foreach (var geo in _geometries) {
                switch (geo.ObjectType) {
                    case ObjectType.Brep:
                        args.Display.DrawBrepShaded(geo as Brep, fill);
                        break;
                    case ObjectType.Extrusion:
                        args.Display.DrawBrepShaded((geo as Extrusion).ToBrep(), fill);
                        break;
                    default:
                        break;
                }
            }
        }

        static BoundingBox ComputeClippingBox(IEnumerable<GeometryBase> geometry)
        {
            var box = BoundingBox.Empty;
            foreach (var geo in geometry) {
                if (geo == null)
                    continue;
                var bb = geo.GetBoundingBox(false);
                if (!bb.IsValid)
                    continue;
                box.Union(bb);
            }

            if (box.IsValid) {
                box.Inflate(1000.0);
            }

            return box;
        }
    }
}