using D2P.Core.Components;
using D2P.Core.Interfaces;
using D2P.Core.Utility;
using Rhino.Input.Custom;
using System.Collections.Generic;
using System.Linq;

namespace D2P.Core.UI
{
    public static class RhinoUIHelper
    {
        public static T GetComponent<T>() where T : class, IComponentBase
        {
            if (!ComponentTable.TryGetTypeId(typeof(T), out string typeId))
                return null;

            var go = new GetObject();
            go.SetCommandPrompt($"{typeId} auswählen");
            go.GeometryFilter = Rhino.DocObjects.ObjectType.AnyObject;
            go.EnablePreSelect(false, true);
            go.DeselectAllBeforePostSelect = false;
            go.Get();

            var rc = go.CommandResult();
            if (rc != Rhino.Commands.Result.Success)
                return null;

            var objRef = go.Object(0);
            return Instantiation.InstanceFromObject<T>(objRef.Object());
        }

        public static IEnumerable<T> GetComponents<T>() where T : class, IComponentBase
        {
            if (!ComponentTable.TryGetTypeId(typeof(T), out string typeId))
                return null;

            var go = new GetObject();
            go.SetCommandPrompt($"{typeId}s auswählen");
            go.GeometryFilter = Rhino.DocObjects.ObjectType.AnyObject;
            go.EnablePreSelect(false, true);
            go.DeselectAllBeforePostSelect = false;
            go.GetMultiple(1, 0);

            var rc = go.CommandResult();
            if (rc != Rhino.Commands.Result.Success)
                return null;

            var objIds = go.Objects().Select(obj => obj.ObjectId);
            return Instantiation.InstancesFromObjects<T>(objIds);
        }
    }
}
