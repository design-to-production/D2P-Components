using D2P.Core.Components;
using D2P.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P.Core.Utility
{
    internal static class Group
    {
        // Add Group
        internal static int AddGroup()
        {
            return Settings.ActiveDoc.Groups.Add();
        }

        // Add Objects To Group
        internal static int AddObjectsToGroup(IEnumerable<Guid> objIDs, int grpIdx)
        {
            if (!objIDs.Any() || grpIdx < 0) return 0;
            if (!Settings.ActiveDoc.Groups.AddToGroup(grpIdx, objIDs)) return 0;
            return objIDs.Count();
        }

        // Remove Objects From All Groups
        internal static bool RemoveObjectFromAllGroups(Guid objectID)
        {
            var rhinoObj = Settings.ActiveDoc.Objects.Find(objectID);
            if (rhinoObj.GroupCount < 1) return false;
            var attr = rhinoObj.Attributes;
            attr.RemoveFromAllGroups();
            return Settings.ActiveDoc.Objects.ModifyAttributes(rhinoObj, attr, true);
        }

        // Get Group Index
        internal static bool GetGroupIndex(IComponentBase component, out int grpIdx)
        {
            grpIdx = -1;
            if (component.ID.Equals(Guid.Empty))
                return false;
            var rhObj = Settings.ActiveDoc.Objects.FindId(component.ID);
            if (rhObj?.GroupCount != 1)
                return false;
            grpIdx = rhObj.GetGroupList()[0];
            return true;
        }
    }
}
