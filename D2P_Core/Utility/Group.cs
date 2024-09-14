using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace D2P_Core.Utility
{
    public static class Group
    {
        public static int AddGroup(RhinoDoc doc) => doc.Groups.Add();
        public static int AddGroup(Guid objectId, RhinoDoc doc) => doc.Groups.Add(new List<Guid>() { objectId });
        public static int AddGroup(IEnumerable<Guid> objectIds, RhinoDoc doc) => doc.Groups.Add(objectIds);

        public static int AddObjectsToGroup(ObjRef[] objRefs, int grpIdx, RhinoDoc doc) => AddObjectsToGroup(objRefs.Select(obj => obj.ObjectId), grpIdx, doc);
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
        public static bool RemoveObjectsFromAllGroups(ObjRef[] objRefs, RhinoDoc doc) => RemoveObjectsFromAllGroups(objRefs.Select(obj => obj.ObjectId), doc);
        public static bool RemoveObjectsFromAllGroups(IEnumerable<Guid> objectIDs, RhinoDoc doc)
        {
            var succeed = new List<bool>();
            foreach (var objectID in objectIDs)
            {
                succeed.Add(RemoveObjectFromAllGroups(objectID, doc));
            }
            return succeed.TrueForAll(x => x);
        }

        public static int GetGroupIndex(Guid componentID, RhinoDoc doc)
        {
            if (componentID.Equals(Guid.Empty)) return -1;
            var rhObj = doc.Objects.FindId(componentID);
            if (rhObj?.GroupCount != 1)
                return -1;
            return rhObj.GetGroupList()[0];
        }
        public static int[] GetAllGroupIndices(RhinoDoc doc)
        {
            return doc.Groups.Select(grp => grp.Index).ToArray();
        }
    }

}
