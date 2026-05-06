using Grasshopper.Kernel;
using Rhino;
using Rhino.UI;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace D2P.GHPlugin.GH {
    public abstract class GHComponentBase : GH_Component {
        protected GHComponentBase(string name, string shortname, string description, string category, string subcategory)
            : base(name, shortname, description, category, subcategory)
        { }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            // Add Changelog
            var menuItemChangelog = Menu_AppendItem(menu, "ShowChangelog", ChangelogHandler, true, false);
            menuItemChangelog.ToolTipText = "The changelog for this project lists all changes made in previous versions";
            // Add Settings
            var menuItemSettings = Menu_AppendItem(menu, "EditDefaultSettings", DefaultSettingsHandler, true, false);
            menuItemSettings.ToolTipText = "In the default settings you can setup the standard settings being used when creating new settings for a component";
        }

        private static void DefaultSettingsHandler(object sender, EventArgs e)
        {
            var panel = new EtoDefaultSettingsPanel();
            panel.Show(RhinoDoc.ActiveDoc);
        }

        private void ChangelogHandler(object sender, EventArgs e)
        {
            var url = "https://github.com/design-to-production/D2P-Components/blob/main/CHANGELOG.md";
            try { OpenURL(url); }
            catch (Exception ex) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
            }
        }

        private static void OpenURL(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Process.Start(new ProcessStartInfo {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                Process.Start("open", url);
            }
        }
    }
}