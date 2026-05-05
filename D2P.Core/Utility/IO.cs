using D2P.Core.Components;
using D2P.Core.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace D2P.Core.Utility {
    public static class IO {
        public static void ExportWithHeadless(IEnumerable<IComponentBase> components, string directory, string fileName)
        {
            if (!components.Any())
                return;

            var currentDoc = Settings.ActiveDoc;
            Settings.ActiveDoc = RHDoc.CreateHeadless(currentDoc);
            foreach (var component in components) {
                component.Commit(false);
            }

            var filePath = Path.Combine(directory, fileName);
            if (!Path.HasExtension(filePath))
                filePath = Path.ChangeExtension(filePath, "3dm");

            RHDoc.Purge(Settings.ActiveDoc);
            Settings.ActiveDoc.Export(filePath);
            Settings.ActiveDoc = currentDoc;
        }

        public static void ExportWithHeadless(IComponentBase component, string directory)
        {
            ExportWithHeadless(new[] { component }, directory, component.Name);
        }
    }
}
