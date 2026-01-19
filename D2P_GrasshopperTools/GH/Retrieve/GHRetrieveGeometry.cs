using D2P_Core.Interfaces;
using D2P_Core.Utility;
using Grasshopper.Kernel;
using System;

namespace D2P_GrasshopperTools.GH.Retrieve {
    public class GHRetrieveGeometry : GHComponentBase {
        /// <summary>
        /// Initializes a new instance of the Component_LayerObjects class.
        /// </summary>
        public GHRetrieveGeometry()
          : base("RetrieveGeometry", "RetrieveGeo",
              "Retrieves geometry of a component-instance",
              "D2P", "02 Retrieve")
        { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Component", "C", "The in-memory representation of a component instance", GH_ParamAccess.item);
            pManager.AddTextParameter("LayerName", "L", "The name of the layer below the root-layer of a component", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "The geometry of the defined layer and scope", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IComponentBase component = null;
            var layerName = string.Empty;

            DA.GetData(0, ref component);
            DA.GetData(1, ref layerName);

            if (component == null) {
                var msg = $"Component is null !";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                return;
            }

            var member = component.FindMember(layerName, out int membersFound);
            if (member == null) {
                var msg = $"Member with LayerName '{layerName}' not found !";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                return;
            }
            else if (membersFound > 1) {
                var msg = $"Found {membersFound} members with layerName {layerName}, specify full path !";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                return;
            }

            var layer = Layers.FindLayer(member, out int layersFound);
            var layerIdx = layer?.Index ?? -1;
            if (layerIdx < 0) {
                if (layerIdx < 0) {
                    var msg = $"Layer {layerName} not found !";
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                    return;
                }
                if (layersFound > 1) {
                    var msg = $"Found {layersFound} layers with name {layerName}, specify full path !";
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                    return;
                }
            }
            DA.SetDataList(0, member.Geometry);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon {
            get {
                //You can add image files to your project resources and access them like this:                
                return Properties.Resources.GH_RetrieveComponentObjects;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid {
            get { return new Guid("33B9B431-3939-4574-BD76-D54E872B1558"); }
        }
    }
}