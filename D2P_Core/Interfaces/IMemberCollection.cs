using System.Collections.Generic;

namespace D2P_Core.Interfaces {
    public interface IMemberCollection {
        IMember ParentMember { get; set; }

        IEnumerable<IMember> AllMembers { get; }
        IEnumerable<IMember> DynamicMembers { get; set; }
        IEnumerable<IMember> StaticMembers { get; }
    }
}
