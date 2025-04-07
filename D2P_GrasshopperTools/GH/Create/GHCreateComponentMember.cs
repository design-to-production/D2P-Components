using D2P_Core;
using D2P_Core.Interfaces;
using Grasshopper.Kernel;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace D2P_GrasshopperTools.GH.Create
{
    public class GHCreateComponentMember : GHComponentBase
    {
        /// <summary>
        /// Initializes a new instance of the Geometry_AddToComponent class.
        /// </summary>
        public GHCreateComponentMember()
          : base("CreateComponentMember", "ComponentMember",
              "Creates a component-member composed of geometry and a layer-information",
              "D2P", "01 Create")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("LayerInfo", "L", "The layer-information used to create geometry on a specific layer of the component", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Geometry", "G", "The input geometry that will be added to a component under a specific layer", GH_ParamAccess.list);
            pManager.AddGenericParameter("ObjectAttributes", "A", "Optional input to define the RhinoObjectAttributes for the geometries", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ComponentMember", "M", "The ComponentMember which can be registered to a component-instance", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            ILayerInfo layerInfo = null;
            var geometry = new List<GeometryBase>();
            ObjectAttributes attributes = null;

            DA.GetData(0, ref layerInfo);
            DA.GetDataList(1, geometry);
            DA.GetData(2, ref attributes);

            var componentMembers = new ComponentMember(layerInfo, geometry, attributes);

            DA.SetData(0, componentMembers);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.GH_CreateComponentObjects;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("05FD312F-0797-4543-8605-90E42DB7F2E3"); }
        }
    }
}