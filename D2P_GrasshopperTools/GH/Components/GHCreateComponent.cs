using D2P_Core;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;

namespace D2P_GrasshopperTools.GH.Components
{
    public class GHCreateComponent : GHComponentPreview
    {
        /// <summary>
        /// Initializes a new instance of the CreateComponent class.
        /// </summary>
        public GHCreateComponent()
          : base("CreateComponent", "Component",
              "Creates a component instance based on a specific type",
              "D2P", "Components")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>  
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Type", "T", "The type definition for this component instance. This will define where the component will be baked into the layer-tree of the Rhino document", GH_ParamAccess.item);
            pManager.AddTextParameter("Name", "N", "The name of the component instance. This will define the name of all objects within this component after baking it to the Rhino document", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "P", "The plane used to create the text-label for the component after baking to the Rhino Document", GH_ParamAccess.item);
            pManager.AddGenericParameter("Parent", "C", "The parent component or name used to create the inherent name of this component", GH_ParamAccess.item);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
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
            var name = string.Empty;
            var plane = Plane.Unset;
            Component parent = null;

            DA.GetData(0, ref componentType);
            DA.GetData(1, ref name);
            DA.GetData(2, ref plane);
            DA.GetData(3, ref parent);

            var parentName = parent?.ShortName ?? parent?.ToString();
            name = string.IsNullOrEmpty(parentName) ? name : $"{parentName}{componentType.Settings.NameDelimiter}{name}";

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
                return Properties.Resources.GH_CreateComponent;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9ABC0992-CC25-47B8-8CAC-BC3D84AA0FD4"); }
        }
    }
}