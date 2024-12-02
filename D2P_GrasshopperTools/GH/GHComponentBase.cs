using Grasshopper.Kernel;
using System;
using System.Windows.Forms;

namespace D2P_GrasshopperTools.GH
{
    public abstract class GHComponentBase : GH_Component
    {
        protected GHComponentBase(string name, string shortname, string description, string category, string subcategory)
            : base(name, shortname, description, category, subcategory)
        { }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            var menuItem = Menu_AppendItem(menu, "ShowChangelog", ChangelogHandler, true, false);
            menuItem.ToolTipText = "The changelog for this project lists all changes made in previous versions";
        }

        private static void ChangelogHandler(object sender, EventArgs e)
        {
            var url = "https://github.com/design-to-production/D2P-Components/blob/main/CHANGELOG.md";
            System.Diagnostics.Process.Start(url);
        }
    }
}