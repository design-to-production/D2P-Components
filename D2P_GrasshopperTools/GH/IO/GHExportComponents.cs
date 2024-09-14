using Grasshopper.Kernel;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace D2P_GrasshopperTools.GH.IO
{
    public class GHExportComponents : GHComponentPreview
    {
        bool _run = false;
        bool _exportOneFile = true;

        /// <summary>
        /// Initializes a new instance of the Component_Export class.
        /// </summary>
        public GHExportComponents()
          : base("ExportComponents", "Export",
              "Exports component-instances to another Rhino document. You can either export all component-instances to a single file or automatically export each component in a seperate file",
              "D2P", "IO")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Components", "C", "The in-memory representation of the component instances", GH_ParamAccess.list);
            pManager.AddTextParameter("Directory", "D", "The directory used for the export of a single file or a file for each component", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Export", "E", "Executes the export process", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string directoryPath = string.Empty;

            DA.GetDataList(0, _components);
            DA.GetData(1, ref directoryPath);
            DA.GetData(2, ref _run);

            if (!_run)
                return;

            var directory = Directory.CreateDirectory(directoryPath);
            if (!directory.Exists)
                return;

            if (_exportOneFile)
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "3dm files (*.3dm)|*3dm",
                    InitialDirectory = directoryPath
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var fileName = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                    D2P_Core.Utility.IO.ExportWithHeadless(_components, directoryPath, fileName);
                }
                else return;
            }
            else
            {
                foreach (var component in _components)
                {
                    D2P_Core.Utility.IO.ExportWithHeadless(component, directory.FullName);
                }
            }

            Process.Start("explorer.exe", @directory.FullName);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "Export OneFile", ClickOnExportOneFile, true, _exportOneFile);
        }

        private void ClickOnExportOneFile(object sender, EventArgs e)
        {
            _exportOneFile = !_exportOneFile;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:                
                return Properties.Resources.GH_ExportComponent;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("A4A283D0-CC4E-4111-A482-4590266BA18B"); }
        }
    }
}