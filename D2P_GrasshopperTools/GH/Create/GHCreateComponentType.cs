using D2P_Core;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace D2P_GrasshopperTools.GH.Create
{
    public class GHCreateComponentType : GHComponentBase
    {
        /// <summary>
        /// Initializes a new instance of the Layers_CreateLayer class.
        /// </summary>
        public GHCreateComponentType()
          : base("CreateComponentType", "ComponentType",
              "Creates a component type which can be used to define components of that specific type",
              "D2P", "01 Create")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TypeID", "T", "Each type has a unique type-id, typically 4 uppercase letters that abbreviate the type-name and are easy to memorize", GH_ParamAccess.item);
            pManager.AddTextParameter("TypeName", "N", "Each type has a type-name, a short human readable description of the type", GH_ParamAccess.item);
            pManager.AddNumberParameter("LabelSize", "L", "The label size defines the size of the text-entity representing a component-instance in a Rhino document", GH_ParamAccess.item);
            pManager.AddColourParameter("LayerColor", "C", "The layer-color defines the color of the component-type root-layer", GH_ParamAccess.item);
            pManager.AddGenericParameter("Settings", "S", "The settings define the basic root-layer for all components being used and a collection of specific delimiters", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ComponentType", "T", "The component-type definition that can be used to create component-instances", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var typeID = string.Empty;
            var typeName = string.Empty;
            double labelSize = 0;
            Color layerColor = Color.Black;
            Settings settings = null;

            DA.GetData(0, ref typeID);
            DA.GetData(1, ref typeName);
            DA.GetData(2, ref labelSize);
            DA.GetData(3, ref layerColor);
            DA.GetData(4, ref settings);

            if (settings == null)
                settings = new Settings();
            if (string.IsNullOrEmpty(typeName))
            {
                var componentLayer = D2P_Core.Utility.Layers.FindComponentLayerByType(typeID, settings.RootLayerName);
                if (componentLayer == null)
                {
                    var msg = $"Layer of type {typeID} does not exist in the RhinoDoc yet. Cannot auto-generate layer description !";
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                    return;
                }
                typeName = D2P_Core.Utility.Layers.GetComponentTypeName(componentLayer, settings);
            }

            if (labelSize <= 0)
                labelSize = Rhino.RhinoDoc.ActiveDoc.DimStyles.Current.TextHeight;

            var cls = new ComponentType(typeID, typeName, settings, labelSize, layerColor);
            DA.SetData(0, cls);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:                
                return Properties.Resources.GH_CreateComponentType;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("574EBA51-8F4D-4B0C-B4C9-A5AD7476876F"); }
        }
    }
}