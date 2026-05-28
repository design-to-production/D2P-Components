using D2P.Core.Interfaces;
using Grasshopper.Kernel;
using System;
using System.Linq;
using System.Windows.Forms;

namespace D2P.GHPlugin.GH.Retrieve {
    public class GHRetrieveGeometry : GHComponentBase {
        private bool _getGeometryRecursive = true;

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
            pManager.AddGenericParameter("GUID", "ID", "The geometry id of the defined layer and scope", GH_ParamAccess.list);
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

            var matchedMembers = component.FindMembers(component, layerName);
            if (!matchedMembers.Any()) {
                var msg = $"Member with LayerName '{layerName}' not found !";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                return;
            }
            else if (matchedMembers.Count() > 1 && !_getGeometryRecursive) {
                var msg = $"Found {matchedMembers.Count()} members with layerName {layerName}, specify full path !";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                return;
            }

            var geometry = matchedMembers.SelectMany(m => m.BaseObjects.Select(b => b.Geometry));
            var ids = matchedMembers.SelectMany(m => m.BaseObjects.Select(b => b.Id));
            DA.SetDataList(0, geometry);
            DA.SetDataList(1, ids);
        }


        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "Get geometry recursively", ClickOnGetGeometryRecursively, true, _getGeometryRecursive);
        }

        private void ClickOnGetGeometryRecursively(object sender, EventArgs e)
        {
            _getGeometryRecursive = !_getGeometryRecursive;
            ExpireSolution(true);
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