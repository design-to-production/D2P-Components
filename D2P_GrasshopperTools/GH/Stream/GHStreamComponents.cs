using D2P_Core.Utility;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P_GrasshopperTools.GH.Stream
{
    public class GHStreamComponents : GHComponentPreview
    {
        public GHStreamComponents()
          : base("StreamComponents", "Stream",
              "Stream component-instances from rhino by providing their GUIDs",
              "D2P", "00 Stream")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            var guidParam = new Grasshopper.Kernel.Parameters.Param_Guid();
            pManager.AddParameter(guidParam, "ComponentIDs", "IDs", "The GUIDs of Rhino component-instances", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Components", "C", "The in-memory representation of component-instances", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var ids = new List<Guid>();
            DA.GetDataList(0, ids);

            _components = Instantiation.InstancesFromObjects(ids).ToList();

            DA.SetDataList(0, _components);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:                
                return Properties.Resources.GH_Stream;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1BA23CA4-39F5-4BC7-B7F9-54FFD973BBA5"); }
        }
    }
}