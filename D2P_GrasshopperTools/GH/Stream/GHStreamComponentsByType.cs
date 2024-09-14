using D2P_Core;
using D2P_GrasshopperTools.Utility;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P_GrasshopperTools.GH.Stream
{
    public class GHStreamComponentsByType : GHVariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the GH_StreamTypes class.
        /// </summary>
        public GHStreamComponentsByType()
          : base("StreamComponentsByType", "StreamByType",
              "Stream component-instances from the Rhino document by providing a type-id. Sorts them by their type-ids and automatically populates the output parameters",
              "D2P", "Stream")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("TypeID", "T", "The type-id of the component-type", GH_ParamAccess.item);
            pManager.AddTextParameter("NameFilter", "F", "The regex pattern used to filter by specific component-names", GH_ParamAccess.item, string.Empty);
            pManager.AddGenericParameter("Settings", "S", "The settings define the basic root-layer for all components being streamed and a collection of specific delimiters", GH_ParamAccess.item);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var type = string.Empty;
            var filter = string.Empty;
            Settings settings = null;
            DA.GetData(0, ref type);
            DA.GetData(1, ref filter);
            DA.GetData(2, ref settings);

            settings = settings ?? DefaultSettings.Create();
            _components = D2P_Core.Utility.Instantiation.InstancesByType(type, settings, null, ':', filter);

            if (DA.Iteration == 0)
                _properties = new Dictionary<string, Type>() { { type, typeof(Enumerable) } };

            if (OutputMismatch() && DA.Iteration == 0)
                OnPingDocument().ScheduleSolution(5, d => CreateOutputParams(false));
            else
                DA.SetDataList(type, _components);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.GH_StreamByType;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("DF586BB3-8A71-4383-9B00-4B624876CE8D"); }
        }
    }
}