using System.Collections.Generic;

namespace D2P.Core.Interfaces {
    public interface IMemberCollection {
        IMember ParentMember { get; set; }

        IEnumerable<IMember> AllMembers { get; }
        IEnumerable<IMember> DynamicMembers { get; }
        IEnumerable<IMember> StaticMembers { get; }

        void SetMember(IMember member);
        void SetMembers(IEnumerable<IMember> members);

        IMember FindMember(IComponentBase component, string layerName, out int membersFound);
        IEnumerable<IMember> FindMembers(IComponentBase component, string layerName);
    }
}
