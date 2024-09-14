using D2P_Core.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace D2P_Core.Utility
{
    public static class IO
    {

        public static void Export(IComponent component, string directoryPath)
        {
            component.ActiveDoc.Objects.UnselectAll();
            foreach (var rhObj in component.RHObjects)
            {
                rhObj.Select(true, false, true, true, true, true);
            }
            var fileName = Path.Combine(directoryPath, component.ShortName + ".3dm");
            component.ActiveDoc.ExportSelected(fileName);
            component.ActiveDoc.Objects.UnselectAll();
        }

        public static void ExportWithHeadless(IComponent component, string directoryPath)
        {
            var headlessDoc = RHDoc.CreateHeadless(component.ActiveDoc);
            RHDoc.AddToRhinoDoc(component, headlessDoc);
            RHDoc.Purge(headlessDoc);
            var fileName = Path.Combine(directoryPath, component.ShortName + ".3dm");
            headlessDoc.Export(fileName);
        }
        public static void ExportWithHeadless(IEnumerable<IComponent> components, string directory, string fileName)
        {
            if (!components.Any())
                return;

            var activeDoc = components.FirstOrDefault()?.ActiveDoc;
            var headlessDoc = RHDoc.CreateHeadless(activeDoc);
            foreach (var component in components)
            {
                RHDoc.AddToRhinoDoc(component, headlessDoc);
            }
            RHDoc.Purge(headlessDoc);
            var filePath = Path.Combine(directory, fileName);
            if (!Path.HasExtension(filePath))
                filePath = Path.ChangeExtension(filePath, "3dm");
            headlessDoc.Export(filePath);
        }

    }
}
