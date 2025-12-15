using D2P_Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace D2P_Core.Components.Member
{
    public abstract class MemberCollection : IMemberCollection
    {
        private Dictionary<string, IMember> _dynamicMembers = new Dictionary<string, IMember>();

        public IMember ParentMember { get; set; }

        public IEnumerable<IMember> AllMembers
        {
            get => StaticMembers.Concat(DynamicMembers);
        }
        public IEnumerable<IMember> DynamicMembers
        {
            get => _dynamicMembers.Values;
            set => _dynamicMembers = value.ToDictionary(m => m.Name, m => m);
        }
        public IEnumerable<IMember> StaticMembers
        {
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

        public IMember this[string name]
        {
            get
            {
                _dynamicMembers.TryGetValue(name, out var v);
                return v ?? null;
            }
            set
            {
                if (value == null) return;
                _dynamicMembers[name] = value;
            }
        }
    }
}
