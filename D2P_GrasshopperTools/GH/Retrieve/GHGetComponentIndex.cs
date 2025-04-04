using D2P_Core;
using D2P_Core.Interfaces;
using Grasshopper.Kernel;
using System;

namespace D2P_GrasshopperTools.GH.Retrieve
{
    public class GHGetComponentIndex : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHGetComponentIndex class.
        /// </summary>
        public GHGetComponentIndex()
          : base("GetComponentIndex", "IndexOf",
              "Gets the index of a component within a joint name",
              "D2P", "02 Retrieve")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Joint Component", "J", "The in-memory representation of the defined joint component instance", GH_ParamAccess.item);
            pManager.AddGenericParameter("Component", "C", "The in-memory representation of the defined component instance", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Index", "I", "The index of the component within the joint name", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Component joint = null;
            Component component = null;

            DA.GetData(0, ref joint);
            DA.GetData(1, ref component);

            var jointDelimiter = joint?.Settings.JointDelimiter;
            if (jointDelimiter == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid Joint Component !");
                return;
            }
            var connectedComponents = joint?.ShortName.Split(jointDelimiter.Value);
            var index = Array.IndexOf(connectedComponents, (component as IComponent)?.ShortName);

            if (index >= 0)
                DA.SetData(0, index);
            else DA.SetData(0, null);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:                
                return Properties.Resources.GH_GetComponentIndex;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2EC3FD54-CC42-40C0-BAE4-9EC8ECB0C761"); }
        }
    }
}