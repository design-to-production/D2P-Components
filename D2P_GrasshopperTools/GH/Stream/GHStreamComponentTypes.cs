using D2P_Core;
using Grasshopper.Kernel;
using Rhino;
using System;
using System.Linq;

namespace D2P_GrasshopperTools.GH.Stream
{
    public class GHStreamComponentTypes : GHVariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the GHStreamComponentTypes class.
        /// </summary>
        public GHStreamComponentTypes()
          : base("StreamComponentTypes", "StreamTypes",
              "Stream component-types from the Rhino document. Sorts them by their type-ids and automatically populates the output parameters",
              "D2P", "Stream")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Settings", "S", "The settings define the basic root-layer for all components being used and a collection of specific delimiters", GH_ParamAccess.item);
            pManager[0].Optional = true;
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
            Settings settings = null;
            DA.GetData(0, ref settings);
            settings = settings ?? new Settings(RhinoDoc.ActiveDoc);

            var componentTypes = D2P_Core.Utility.Instantiation.GetComponentTypes(settings);

            if (DA.Iteration == 0)
                _properties = componentTypes.ToDictionary(compType => compType.TypeID, compType => typeof(ComponentType));

            if (OutputMismatch() && DA.Iteration == 0)
                OnPingDocument().ScheduleSolution(5, d => CreateOutputParams(false));
            else
            {
                foreach (var compType in componentTypes)
                {
                    DA.SetData(compType.TypeID, compType);
                }
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.GH_StreamComponentTypes;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("EEEF3B66-9E44-40EC-B7E0-A97DF9ED483D"); }
        }
    }
}