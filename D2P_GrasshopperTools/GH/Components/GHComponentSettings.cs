using D2P_Core;
using D2P_GrasshopperTools.GH.Eto;
using Grasshopper.Kernel;
using Rhino;
using Rhino.UI;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace D2P_GrasshopperTools.GH.Components
{
    public class GHComponentSettings : GHComponentBase
    {
        /// <summary>
        /// Initializes a new instance of the GH_Settings class.
        /// </summary>
        public GHComponentSettings()
          : base("ComponentSettings", "Settings",
              "Defines the delimiter settings being used for a component-instance",
              "D2P", "Components")
        { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("RootLayerName", "R", "Specifies the root layer for the components", GH_ParamAccess.item);
            pManager.AddColourParameter("RootLayerColor", "C", "Specifies the root layer color for the components", GH_ParamAccess.item);
            pManager.AddTextParameter("DimensionStyleName", "S", "Specifies the style used for the annotations of the components", GH_ParamAccess.item);
            pManager.AddTextParameter("TypeDelimiter", "T", "Specifies the delimiter used to seperate the component type", GH_ParamAccess.item);
            pManager.AddTextParameter("LayerDelimiter", "L", "Specifies the delimiter used to seperate the component layer", GH_ParamAccess.item);
            pManager.AddTextParameter("NameDelimiter", "N", "Specifies the delimiter used to seperate the component name", GH_ParamAccess.item);
            pManager.AddTextParameter("LayerDescriptionDelimiter", "D", "Specifies the delimiter used to seperate the component (layer) description", GH_ParamAccess.item);
            pManager.AddTextParameter("LayerNameDelimiter", "N", "Specifies the delimiter used to define nested layers", GH_ParamAccess.item);
            pManager.AddTextParameter("CountDelimiter", "C", "Specifies the delimiter used to seperate multiple joints of the same components", GH_ParamAccess.item);
            pManager.AddTextParameter("JointDelimiter", "J", "Specifies the delimiter used to seperate the components attached to a joint", GH_ParamAccess.item);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
            pManager[9].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ComponentSettings", "S", "Settings being used for the components", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var rootLayerName = string.Empty;
            var rootLayerColor = Color.Empty;
            var dimensionStyleName = string.Empty;
            var typeDelimiter = string.Empty;
            var layerDelimiter = string.Empty;
            var nameDelimiter = string.Empty;
            var layerDescriptionDelimiter = string.Empty;
            var layerNameDelimiter = string.Empty;
            var countDelimiter = string.Empty;
            var jointDelimiter = string.Empty;

            DA.GetData(0, ref rootLayerName);
            DA.GetData(1, ref rootLayerColor);
            DA.GetData(2, ref dimensionStyleName);
            DA.GetData(3, ref typeDelimiter);
            DA.GetData(4, ref layerDelimiter);
            DA.GetData(5, ref nameDelimiter);
            DA.GetData(6, ref layerDescriptionDelimiter);
            DA.GetData(7, ref layerNameDelimiter);
            DA.GetData(8, ref countDelimiter);
            DA.GetData(9, ref jointDelimiter);

            var settings = new Settings()
            {
                RootLayerName = !string.IsNullOrEmpty(rootLayerName) ? rootLayerName : Properties.Settings.Default.DefaultRootLayerName,
                RootLayerColor = rootLayerColor != Color.Empty ? rootLayerColor : Properties.Settings.Default.DefaultRootLayerColor,
                DimensionStyleName = !string.IsNullOrEmpty(dimensionStyleName) ? dimensionStyleName : Properties.Settings.Default.DefaultDimensionStyleName,
                TypeDelimiter = !string.IsNullOrEmpty(typeDelimiter) ? typeDelimiter.First() : Properties.Settings.Default.DefaultTypeDelimiter,
                LayerDelimiter = !string.IsNullOrEmpty(layerDelimiter) ? layerDelimiter.First() : Properties.Settings.Default.DefaultLayerDelimiter,
                NameDelimiter = !string.IsNullOrEmpty(nameDelimiter) ? nameDelimiter.First() : Properties.Settings.Default.DefaultNameDelimiter,
                LayerDescriptionDelimiter = !string.IsNullOrEmpty(layerDescriptionDelimiter) ? layerDescriptionDelimiter.First() : Properties.Settings.Default.DefaultLayerDescriptionDelimiter,
                LayerNameDelimiter = !string.IsNullOrEmpty(layerNameDelimiter) ? layerNameDelimiter.First() : Properties.Settings.Default.DefaultLayerNameDelimiter,
                CountDelimiter = !string.IsNullOrEmpty(countDelimiter) ? countDelimiter.First() : Properties.Settings.Default.DefaultCountDelimiter,
                JointDelimiter = !string.IsNullOrEmpty(jointDelimiter) ? jointDelimiter.First() : Properties.Settings.Default.DefaultJointDelimiter,
            };

            DA.SetData(0, settings);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            var menuItem = Menu_AppendItem(menu, "EditDefaultSettings", DefaultSettingsHandler, true, false);
            menuItem.ToolTipText = "In the default settings you can setup the standard settings being used when creating new settings for a component";
        }

        private static void DefaultSettingsHandler(object sender, EventArgs e)
        {
            var panel = new EtoDefaultSettingsPanel();
            panel.Show(RhinoDoc.ActiveDoc);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:                
                return Properties.Resources.GH_Settings;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("58EBE9AD-43FE-4F2D-A896-5FFEC1AFBE75"); }
        }
    }
}