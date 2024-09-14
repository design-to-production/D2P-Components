using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace D2P_GrasshopperTools.GH.Geometry
{
    public class GHCreateBBox : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the Geometry_CreateBBox class.
        /// </summary>
        public GHCreateBBox()
          : base("CreateBBox", "BBox",
              "Creates an oriented Bounding Box based on the input geometry and plane",
              "D2P", "Geometry")
        { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "The geometry used to create the BoundingBox", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Plane", "P", "The base plane for the oriented BoundingBox", GH_ParamAccess.item);
            pManager.AddNumberParameter("ExtraLength", "xL", "Optional input to define an extra length for the BoundingBox", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("ExtraWidth", "xW", "Optional input to define an extra width for the BoundingBox", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("ExtraHeight", "xH", "Optional input to define an extra height for the BoundingBox", GH_ParamAccess.item, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBoxParameter("BBox", "B", "The BoundingBox created for the input geometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GeometryBase geometry = null;
            var plane = Plane.Unset;
            double xLength = 0;
            double xHeight = 0;
            double xWidth = 0;
            DA.GetData(0, ref geometry);
            DA.GetData(1, ref plane);
            DA.GetData(2, ref xLength);
            DA.GetData(3, ref xWidth);
            DA.GetData(4, ref xHeight);

            var box = new Box(plane, geometry);
            if (xLength > 0)
                box.X = new Interval(box.X.T0 - xLength, box.X.T1 + xLength);
            if (xWidth > 0)
                box.Y = new Interval(box.Y.T0 - xWidth, box.Y.T1 + xWidth);
            if (xHeight > 0)
                box.Z = new Interval(box.Z.T0 - xHeight, box.Z.T1 + xHeight);

            DA.SetData(0, box);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:                
                return Properties.Resources.GH_CreateBBox;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F75BED37-EB99-4CFD-9088-170797B638FA"); }
        }
    }
}