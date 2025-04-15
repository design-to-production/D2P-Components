using D2P_Core;
using D2P_GrasshopperTools.Utility;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P_GrasshopperTools.GH.Stream
{
    public class GHStreamComponentsGate : GHVariableParameterComponent
    {
        public GHStreamComponentsGate()
          : base("StreamComponentsGate", "StreamGate",
              "Stream component-instances from the Rhino document by providing their GUIDs. Sorts them by their type-ids and automatically populates the output parameters",
              "D2P", "00 Stream")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            var guidParam = new Grasshopper.Kernel.Parameters.Param_Guid();
            pManager.AddParameter(guidParam, "ComponentIDs", "IDs", "The GUIDs of Rhino component-instances", GH_ParamAccess.list);
            pManager.AddGenericParameter("Settings", "S", "The settings define the basic root-layer for all components being streamed and a collection of specific delimiters", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        { }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var ids = new List<Guid>();
            Settings settings = null;
            DA.GetDataList(0, ids);
            DA.GetData(1, ref settings);

            settings = settings ?? DefaultSettings.Create();
            _components = D2P_Core.Utility.Instantiation.InstancesFromObjects(ids, settings);
            var componentGroups = _components.GroupBy(comp => comp.TypeID);

            if (DA.Iteration == 0)
            {
                _properties = componentGroups.ToDictionary(grp => grp.First().TypeID, c => typeof(Enumerable));
            }

            if (OutputMismatch() && DA.Iteration == 0)
            {
                OnPingDocument().ScheduleSolution(5, d =>
                {
                    CreateOutputParams(false);
                });
            }
            else
            {
                foreach (var group in componentGroups)
                {
                    var typeName = group.First().TypeID;
                    DA.SetDataList(typeName, group);
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
                return Properties.Resources.GH_StreamGate;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F1B5318B-1C06-4DA3-9E97-C4A3E16366D3"); }
        }
    }
}