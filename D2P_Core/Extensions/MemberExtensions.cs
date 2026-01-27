using D2P_Core.Interfaces;
using System.Collections.Generic;

namespace D2P_Core.Extensions {
    public static class MemberExtensions {
        public static IEnumerable<IMember> Duplicate(this IEnumerable<IMember> members)
        {
            foreach (var member in members) {
                yield return member.Duplicate();
            }
        }
        public static void SetComponent(this IEnumerable<IMember> members, IComponentBase component)
        {
            foreach (var member in members) {
                member.Component = component;
                member.AllMembers.SetComponent(component);
            }
        }
    }
}
