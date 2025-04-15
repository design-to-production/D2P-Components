using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P_GrasshopperTools.GH.Retrieve
{
    public class GHRetrieveJoints : GHComponentPreview
    {
        /// <summary>
        /// Initializes a new instance of the GH_RetrieveJoints class.
        /// </summary>
        public GHRetrieveJoints()
          : base("RetrieveJoints", "Joints",
              "Retrieves all joint-components of a given input component",
              "D2P", "02 Retrieve")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Component", "C", "The in-memory representation of a component instance", GH_ParamAccess.item);
            pManager.AddTextParameter("TypeIDFilter", "F", "A list of type-ids to return only children of a specific component-type", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("JointComponents", "C", "The in-memory representation of the component-joint instances", GH_ParamAccess.list);
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

            var joints = Instantiation.GetJoints(component, filterTypes);
            if (joints == null || !joints.Any())
            {
                var msg = $"Joints of component {component.Name} not found !";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, msg);
                return;
            }

            foreach (var joint in joints)
            {
                if (!_components.Contains(joint))
                    _components.Add(joint);
            }

            DA.SetDataList(0, joints);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:                                
                return Properties.Resources.GH_RetrieveJoints;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0D9BB504-81E8-439E-8F3F-74904EE834E1"); }
        }
    }
}