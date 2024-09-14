using D2P_Core;
using D2P_Core.Interfaces;

using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace D2P_GrasshopperTools.GH.Components
{
    public class GHRegisterComponentMembers : GHComponentPreview
    {
        /// <summary>
        /// Initializes a new instance of the Component_AddGeometryWithLayer class.
        /// </summary>
        public GHRegisterComponentMembers()
          : base("RegisterComponentMembers", "RegisterMembers",
              "Registers geometry and attributes to an in-memory representation of a component instance",
              "D2P", "Components")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Component", "C", "The in-memory representation of a component instance to process", GH_ParamAccess.item);
            pManager.AddGenericParameter("ComponentMembers", "M", "The component-members to add to the input component", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Components", "C", "The in-memory representation of the component instances extended by geometries and attributes", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            IComponent component = null;
            var componentMembers = new List<ComponentMember>();
            DA.GetData(0, ref component);
            DA.GetDataList(1, componentMembers);

            if (component == null)
            {
                var msg = $"Component is null !";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                return;
            }

            var componentClone = component.Clone();
            if (!componentClone.IsVirtual)
                componentClone.ClearStagingLayerCollection();

            foreach (var obj in componentMembers)
            {
                var rawLayerName = obj.LayerInfo.RawLayerName;
                if (D2P_Core.Utility.Layers.FindLayerIndexByFullPath(componentClone, rawLayerName) < 0)
                {
                    D2P_Core.Utility.Layers.FindLayerIndex(componentClone, rawLayerName, out int layersFound);
                    if (layersFound > 1)
                    {
                        var msg = $"Found {layersFound} layers with name {rawLayerName}, specify full path !";
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, msg);
                        return;
                    }
                }
                componentClone.ReplaceGeometries(obj);
            }

            _components.Add(componentClone);
            DA.SetData(0, componentClone);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:                
                return Properties.Resources.GH_RegisterComponentObjects;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("D373D60F-4EDB-40D7-92F9-E2C446B8CC95"); }
        }
    }
}