using Grasshopper.Kernel;
using Rhino.DocObjects;
using System;
using System.Drawing;

namespace D2P_GrasshopperTools.GH.Geometry
{
    public class GHCreateObjectAttributes : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_CreateObjectAttributes class.
        /// </summary>
        public GHCreateObjectAttributes()
          : base("CreateObjectAttributes", "ObjectAttributes",
              "A simple GH component to create basic RhinoObjectAttributes",
              "D2P", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("ObjectColor", "C", "The color of the object after baking the component to a Rhino document", GH_ParamAccess.item);
            pManager.AddNumberParameter("ObjectColorSource", "S", "ColorFromLayer=0  ColorFromObject=1 ColorFromMaterial=2 ColorFromParent=3", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ObjectAttributes", "A", "The RhinoObjectAttributes defined by this component", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var objectColor = Color.Black;
            double colorSourceDouble = 0.0;
            DA.GetData(0, ref objectColor);
            DA.GetData(1, ref colorSourceDouble);

            var colorSource = (ObjectColorSource)(int)colorSourceDouble;

            var objectAttributes = new ObjectAttributes()
            {
                ObjectColor = objectColor,
                ColorSource = colorSource,
            };

            DA.SetData(0, objectAttributes);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.GH_ObjectAttributes;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("AA51B317-EC23-4E00-80FA-AD45BEDD1724"); }
        }
    }
}