using D2P_Core;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P_GrasshopperTools.GH.Components
{
    public class GHCreateJoint : GHComponentPreview
    {
        /// <summary>
        /// Initializes a new instance of the GHCreateJoint class.
        /// </summary>
        public GHCreateJoint()
          : base("CreateJoint", "Joint",
              "Creates a joint-component instance based on a specific type and related components",
              "D2P", "Components")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Type", "T", "The type definition for this component instance. This will define where the component will be baked into the layer-tree of the Rhino document", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "P", "The plane used to create the text-label for the component after baking to the Rhino Document", GH_ParamAccess.item);
            pManager.AddGenericParameter("Components", "C", "The components used to create the joint-relation of this component", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Component", "C", "The in-memory representation of the defined component instance", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ComponentType componentType = null;
            var plane = Plane.Unset;
            var components = new List<Component>();

            DA.GetData(0, ref componentType);
            DA.GetData(1, ref plane);
            DA.GetDataList(2, components);

            var componentNames = components.Select(c => c?.ShortName ?? string.Empty);
            var name = String.Join(componentType.Settings.JointDelimiter.ToString(), componentNames);
            if (!componentNames.Any() || componentNames.Contains(string.Empty))
            {
                var msg = $"Invalid Joint Name {name} !";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                return;
            }
            else if (componentNames.Count() == 1)
            {
                var msg = $"Please provide more than one component to create a joint !";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                return;
            }

            var component = new Component(componentType, name, plane);
            _components.Add(component);

            DA.SetData(0, component);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.GH_CreateJoint;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("D7B9D83D-4C26-412D-A860-7B56A8C6A755"); }
        }
    }
}