using D2P_Core.Interfaces;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P_GrasshopperTools.GH.Components
{
    public class GHRetrieveConnectedComponents : GHComponentPreview
    {
        /// <summary>
        /// Initializes a new instance of the GH_RetrieveConnectedComponents class.
        /// </summary>
        public GHRetrieveConnectedComponents()
          : base("RetrieveConnectedComponents", "RetrieveConnected",
              "Retrieves all the components connected to the input components by a joint",
              "D2P", "Components")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Component", "C", "The in-memory representation of a component instance", GH_ParamAccess.item);
            pManager.AddTextParameter("TypeIDFilter", "F", "A list of type-ids to return only children of specific component-types", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ConnectedComponents", "C", "The in-memory representation of the components connected to the input components", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IComponent component = null;
            var filterTypes = new List<string>();
            DA.GetData(0, ref component);
            DA.GetDataList(1, filterTypes);

            if (component == null)
            {
                var msg = $"Component is null !";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                return;
            }

            var connectedComponents = D2P_Core.Utility.Instantiation.GetConnectedComponents(component, filterTypes);
            if (connectedComponents == null || !connectedComponents.Any())
            {
                var msg = $"Connected components of component {component.Name} not found !";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, msg);
                return;
            }

            foreach (var comp in connectedComponents)
            {
                if (!_components.Contains(comp))
                    _components.Add(comp);
            }

            DA.SetDataList(0, connectedComponents);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.GH_RetrieveConnectedComponents;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("320EC34C-DA24-4AFC-9491-E98CB4579FA1"); }
        }
    }
}