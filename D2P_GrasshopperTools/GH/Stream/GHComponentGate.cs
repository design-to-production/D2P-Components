using D2P_Core.Interfaces;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Linq;

namespace D2P_GrasshopperTools.GH.Stream
{
    public class GHComponentGate : GHVariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the Utility_RakeComponents class.
        /// </summary>
        public GHComponentGate()
          : base("ComponentGate", "Gate",
              "Sorts the input component-instances by their type-ids and automatically populates the output parameters",
              "D2P", "00 Stream")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Components", "C", "The in-memory representation of the component instances", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager) { }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (!DA.GetDataTree(0, out GH_Structure<IGH_Goo> componentTree))
                return;

            _components = componentTree.Select(x => new GH_ObjectWrapper(x).Value as IComponent).ToList();

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
                SetDataTrees(DA, componentTree);
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
                return Properties.Resources.GH_ExplodeComponentStream;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("C0E8C4D7-0B5F-49FF-9049-02FC0C5BC1B8"); }
        }
    }
}