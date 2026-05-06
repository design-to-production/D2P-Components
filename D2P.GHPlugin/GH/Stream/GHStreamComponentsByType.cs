using D2P.Core;
using D2P.Core.Interfaces;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace D2P.GHPlugin.GH.Stream {
    public class GHStreamComponentsByType : GHVariableParameterComponent {
        bool reverseRegex = false;

        /// <summary>
        /// Initializes a new instance of the GH_StreamTypes class.
        /// </summary>
        public GHStreamComponentsByType()
          : base("StreamComponentsByType", "StreamByType",
              "Stream component-instances from the Rhino document by providing a type-id. Sorts them by their type-ids and automatically populates the output parameters",
              "D2P", "00 Stream")
        { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("TypeId", "T", "The component-type or its type-id used to stream specific types", GH_ParamAccess.list);
            pManager.AddTextParameter("NameFilter", "N", "The regex pattern used to filter by specific component-names", GH_ParamAccess.list, string.Empty);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        { }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var componentTypes = new List<GH_ObjectWrapper>();
            var filterList = new List<string>();
            DA.GetDataList(0, componentTypes);
            DA.GetDataList(1, filterList);

            var componentTrees = new Dictionary<string, DataTree<IComponentBase>>();
            foreach (var componentType in componentTypes) {
                var typeID = (componentType?.Value as IComponentType)?.TypeId ?? componentType?.Value?.ToString();
                _properties.Add(typeID, typeof(Enumerable));
                componentTrees.Add(typeID, new DataTree<IComponentBase>());
                for (int i = 0; i < filterList.Count; i++) {
                    var filterOptions = new FilterOptions() { RegexPattern = filterList[i], ReversePattern = reverseRegex };
                    var components = D2P.Core.Utility.Instantiation.InstancesByType(typeID, filterOptions);
                    componentTrees[typeID].EnsurePath(i);
                    componentTrees[typeID].AddRange(components);
                    _components.AddRange(components); // only for visualization
                }
            }

            _components.Sort();

            if (OutputMismatch() && DA.Iteration == 0)
                OnPingDocument().ScheduleSolution(5, d => CreateOutputParams(false));
            else {
                foreach (var kv in componentTrees) {
                    var paramIdx = Params.Output.FindIndex(x => x.Name == kv.Key);
                    DA.SetDataTree(paramIdx, kv.Value);
                }
            }
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            var menuItem = Menu_AppendItem(menu, "NegateRegexFilter", NegateRegexFilterHandler, true, reverseRegex);
            menuItem.ToolTipText = "When true the name-filter will be negated";
        }

        private void NegateRegexFilterHandler(object sender, EventArgs e)
        {
            reverseRegex = !reverseRegex;
            ExpireSolution(true);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon {
            get {
                //You can add image files to your project resources and access them like this:
                return Properties.Resources.GH_TypeStream;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid {
            get { return new Guid("DF586BB3-8A71-4383-9B00-4B624876CE8D"); }
        }
    }
}