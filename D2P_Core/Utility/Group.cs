using D2P_Core.Interfaces;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P_Core.Utility
{
    public static class Group
    {
        public static int AddGroup(RhinoDoc doc) => doc.Groups.Add();

        public static int AddObjectsToGroup(IEnumerable<Guid> objIDs, int grpIdx, RhinoDoc doc)
        {
            if (!objIDs.Any() || grpIdx < 0) return 0;
            if (!doc.Groups.AddToGroup(grpIdx, objIDs)) return 0;
            return objIDs.Count();
        }

        public static bool RemoveObjectFromAllGroups(Guid objectID, RhinoDoc doc)
        {
            var rhinoObj = doc.Objects.Find(objectID);
            if (rhinoObj.GroupCount < 1) return false;
            var attr = rhinoObj.Attributes;
            attr.RemoveFromAllGroups();
            return doc.Objects.ModifyAttributes(rhinoObj, attr, true);
        }
        public static bool RemoveObjectsFromAllGroups(IEnumerable<Guid> objectIDs, RhinoDoc doc)
        {
            var succeed = new List<bool>();
            foreach (var objectID in objectIDs)
            {
                succeed.Add(RemoveObjectFromAllGroups(objectID, doc));
            }
            return succeed.TrueForAll(x => x);
        }

        public static bool GetGroupIndex(IComponentBase component, out int grpIdx)
        {
            grpIdx = -1;
            if (component.ID.Equals(Guid.Empty))
                return false;
            var rhObj = component.ActiveDoc.Objects.FindId(component.ID);
            if (rhObj?.GroupCount != 1)
                return false;
            grpIdx = rhObj.GetGroupList()[0];
            return true;
        }
    }

}
