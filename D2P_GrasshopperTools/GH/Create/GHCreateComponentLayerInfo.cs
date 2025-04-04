using D2P_Core;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace D2P_GrasshopperTools.GH.Create
{
    public class GHCreateComponentLayerInfo : GHComponentBase
    {
        /// <summary>
        /// Initializes a new instance of the GH_CreateComponentLayerInfo class.
        /// </summary>
        public GHCreateComponentLayerInfo()
          : base("CreateLayerInfo", "LayerInfo",
              "Creates a layer-information which can be applied to a component together with geometry",
              "D2P", "01 Create")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("LayerName", "N", "The name of the layer below the root-layer of a component", GH_ParamAccess.item);
            pManager.AddColourParameter("LayerColor", "C", "The color of the layer", GH_ParamAccess.item, Color.Black);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("LayerInfo", "L", "The defined layer-information", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string layerName = string.Empty;
            Color layerColor = Color.Black;

            DA.GetData(0, ref layerName);
            DA.GetData(1, ref layerColor);

            var layerObject = new LayerInfo(layerName, layerColor);

            DA.SetData(0, layerObject);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:                
                return Properties.Resources.GH_CreateLayerObject;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("BA650D9F-3F81-4E2B-9500-4F403C6DD4B3"); }
        }
    }
}