using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Grasshopper.Kernel;
using System;

namespace D2P_GrasshopperTools.GH.Retrieve
{
    public class GHRetrieveParent : GHComponentPreview
    {
        /// <summary>
        /// Initializes a new instance of the Component_RetrieveParentComponent class.
        /// </summary>
        public GHRetrieveParent()
          : base("RetrieveParentComponent", "Parent",
              "Retrieves the parent component of a given input component. E.g. If the component-instance is named “aa.01”, “aa.02”, “aa.03”, ... the parent-instance is named “aa”",
              "D2P", "02 Retrieve")
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
            pManager.AddGenericParameter("ParentComponent", "C", "The in-memory representation of the component-parent instance", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IComponent component = null;
            DA.GetData(0, ref component);

            if (component == null)
            {
                var msg = $"Component is null !";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                return;
            }

            var parent = Instantiation.GetParentComponent(component, out int parentsFound);
            if (parent == null)
            {
                var msg = $"Parent of component {component.Name} not found !";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, msg);
                return;
            }
            if (parentsFound > 1)
            {
                var msg = $"Found {parentsFound} parents for component {component.Name} !";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                return;
            }

            _components.Add(parent);
            DA.SetData(0, parent);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:                
                return Properties.Resources.GH_RetrieveParent;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("92F56E9E-C2DA-4ADE-9D04-061A6F88C739"); }
        }
    }
}