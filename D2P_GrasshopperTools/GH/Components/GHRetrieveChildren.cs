using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P_GrasshopperTools.GH.Components
{
    public class GHRetrieveChildren : GHComponentPreview
    {
        /// <summary>
        /// Initializes a new instance of the Component_RetrieveParentComponent class.
        /// </summary>
        public GHRetrieveChildren()
          : base("RetrieveChildComponents", "Children",
              "Retrieves child components of a given input component. E.g. If the parent-instance is named “aa” all child-instances are named “aa.01”, “aa.02”, “aa.03”, etc.",
              "D2P", "Components")
        { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Component", "C", "The in-memory representation of a component instance to process", GH_ParamAccess.item);
            pManager.AddTextParameter("TypeIDFilter", "F", "A list of type-ids to return only children of specific component-types", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ChildComponents", "C", "The in-memory representation of the component-child instances", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IComponent component = null;
            List<string> filterTypes = new List<string>();
            DA.GetData(0, ref component);
            DA.GetDataList(1, filterTypes);

            if (component == null)
            {
                var msg = $"Component is null !";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                return;
            }

            var children = Instantiation.GetChildren(component, filterTypes);
            if (children == null || !children.Any())
            {
                var msg = $"Children of component {component.Name} not found !";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                return;
            }

            _components.AddRange(children);
            DA.SetDataList(0, children);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:                
                return Properties.Resources.GH_RetrieveChildren;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("730EF6B1-3B10-4A7E-B963-EF11988700DB"); }
        }
    }
}