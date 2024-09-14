using D2P_Core;
using Grasshopper.Kernel;
using System;

namespace D2P_GrasshopperTools.GH.Components
{
    public class GHDeconstructComponentType : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_DeconstructComponentType class.
        /// </summary>
        public GHDeconstructComponentType()
          : base("DeconstructComponentType", "DeType",
              "Deconstructs a component",
              "D2P", "Components")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("ComponentType", "T", "A component-type definition", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("TypeID", "T", "Each type has a unique type-id, typically 4 uppercase letters that abbreviate the type-name and are easy to memorize", GH_ParamAccess.item);
            pManager.AddTextParameter("TypeName", "N", "Each type has a type-name, a short human readable description of the type", GH_ParamAccess.item);
            pManager.AddNumberParameter("LabelSize", "L", "The label size defines the size of the text-entity representing a component-instance in a Rhino document", GH_ParamAccess.item);
            pManager.AddColourParameter("LayerColor", "C", "The layer-color defines the color of the component-type root-layer", GH_ParamAccess.item);
            pManager.AddGenericParameter("Settings", "S", "The settings define the basic root-layer for all components being used and a collection of specific delimiters", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ComponentType componentType = null;
            DA.GetData(0, ref componentType);

            DA.SetData(0, componentType?.TypeID);
            DA.SetData(1, componentType?.TypeName);
            DA.SetData(2, componentType?.LabelSize);
            DA.SetData(3, componentType?.LayerColor);
            DA.SetData(4, componentType?.Settings);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:                
                return Properties.Resources.GH_DeconstructComponentType;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7848642D-3659-42D2-912D-0C12683849E7"); }
        }
    }
}