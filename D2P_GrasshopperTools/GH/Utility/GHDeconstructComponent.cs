using D2P_Core.Interfaces;
using Grasshopper.Kernel;
using System;

namespace D2P_GrasshopperTools.GH.Utility
{
    public class GHDeconstructComponent : GHComponentPreview
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructComponent class.
        /// </summary>
        public GHDeconstructComponent()
          : base("DeconstructComponent", "DeComp",
              "Deconstructs a component",
              "D2P", "04 Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Component", "C", "The in-memory representation of a component instance", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            var guidParam = new Grasshopper.Kernel.Parameters.Param_Guid();
            pManager.AddParameter(guidParam, "GUID", "ID", "The GUID of the component-instance", GH_ParamAccess.item);
            pManager.AddGenericParameter("Type", "T", "The type definition for this component instance", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "The name of the component instance", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "P", "The plane of the component instance", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IComponent component = null;
            DA.GetData(0, ref component);

            DA.SetData(0, component?.ID);
            DA.SetData(1, component?.ComponentType);
            DA.SetData(2, component?.ShortName);
            DA.SetData(3, component?.Plane);

            _components.Add(component);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:                
                return Properties.Resources.GH_DeconstructComponent;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E11BE526-496C-45EA-8A8F-12A313858EE7"); }
        }
    }
}