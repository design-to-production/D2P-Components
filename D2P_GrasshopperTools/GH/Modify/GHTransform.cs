using D2P_Core.Interfaces;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P_GrasshopperTools.GH.Modify
{
    public class GHTransform : GHComponentPreview
    {
        /// <summary>
        /// Initializes a new instance of the TransformComponent class.
        /// </summary>
        public GHTransform()
          : base("TransformComponent", "XformComp",
              "Transforms a component and all geometries inside",
              "D2P", "03 Modify")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Components", "C", "The in-memory representation of component instances to process", GH_ParamAccess.list);
            pManager.AddGenericParameter("SourcePlanes", "S", "Source planes for the transformation", GH_ParamAccess.list);
            pManager.AddGenericParameter("TargetPlanes", "T", "Target planes for the transformation", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Components", "C", "The in-memory representation of the transformed component instances", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var components = new List<IComponent>();
            var sourcePlanes = new List<Plane>();
            var targetPlanes = new List<Plane>();
            DA.GetDataList(0, components);
            DA.GetDataList(1, sourcePlanes);
            DA.GetDataList(2, targetPlanes);

            _components = components.Select(comp => comp.VirtualClone()).ToList();

            if (_components.Count != sourcePlanes.Count && sourcePlanes.Count != 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Less source planes than components provided");
                return;
            }
            if (_components.Count > targetPlanes.Count && targetPlanes.Count != 1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Less target planes than components provided");
                return;
            }

            for (var i = 0; i < _components.Count; ++i)
            {
                var plane0 = sourcePlanes.Count > 0 ? sourcePlanes[i] : _components[i].Plane;
                var planeIdx = targetPlanes.Count == 1 ? 0 : i;
                var xform = Transform.PlaneToPlane(plane0, targetPlanes[planeIdx]);
                _components[i].Transform(xform);
            }

            DA.SetDataList(0, _components);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:                
                return Properties.Resources.GH_Transform;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7CC23555-75E9-4846-B658-BD12FC4B0277"); }
        }
    }
}