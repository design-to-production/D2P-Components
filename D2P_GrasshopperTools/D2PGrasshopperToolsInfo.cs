using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace D2P_GrasshopperTools
{
    public class D2PGrasshopperToolsInfo : GH_AssemblyInfo
    {
        public override string Name => "d2p-components";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => Properties.Resources.Logo;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "D2P Components for Grasshopper";

        public override Guid Id => new Guid("94B5D1B5-5BC2-4D6F-B5B3-1FC7391668BA");

        //Return a string identifying you or your company.
        public override string AuthorName => "Design-To-Production";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "components@designtoproduction.com";

        public D2PGrasshopperToolsInfo()
        { }
    }
}