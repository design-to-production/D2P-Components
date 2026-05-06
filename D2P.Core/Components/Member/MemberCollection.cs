using D2P.Core.Extensions;
using D2P.Core.Interfaces;
using D2P.Core.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace D2P.Core.Components.Member {
    public abstract class MemberCollection : IMemberCollection {
        protected Dictionary<string, IMember> _dynamicMembers = new Dictionary<string, IMember>();

        public IMember ParentMember { get; set; }

        public IEnumerable<IMember> AllMembers {
            get => StaticMembers.Concat(DynamicMembers);
        }
        public IEnumerable<IMember> DynamicMembers {
            get => _dynamicMembers.Values;
            set => _dynamicMembers = value.ToDictionary(m => Layers.ComposeFullLayerPath(m), m => m);
        }
        public IEnumerable<IMember> StaticMembers {
            get => GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p =>
                    p.CanRead &&
                    p.GetIndexParameters().Length == 0 &&
                    typeof(IMember).IsAssignableFrom(p.PropertyType) &&
                    p.Name != nameof(ParentMember))
                .Select(p => p.GetValue(this))
                .OfType<IMember>();
        }

        public virtual void SetMember(IMember member)
        {
            var key = Layers.ComposeFullLayerPath(member);
            if (_dynamicMembers.ContainsKey(key))
                _dynamicMembers.Remove(key);
            _dynamicMembers.Add(key, member);
        }
        public void SetMembers(IEnumerable<IMember> members)
        {
            foreach (var member in members) {
                SetMember(member);
            }
        }
        public IMember FindMember(IComponentBase component, string layerName, out int membersFound)
        {
            var matchedMembers = FindMembers(component, layerName);
            membersFound = matchedMembers.Count();
            return matchedMembers?.FirstOrDefault();
        }

        public IEnumerable<IMember> FindMembers(IComponentBase component, string layerName)
        {
            var allMembersFlattened = Members.FindMembers(component).Flatten();
            var memberDict = allMembersFlattened.ToDictionary(m => Layers.ComposeMemberLayerName(m), m => m);
            return memberDict
                .Where(item => item.Key.Contains(layerName))
                .Select(item => item.Value);
        }
    }
}
