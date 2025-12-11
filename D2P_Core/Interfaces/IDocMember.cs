using System.Collections.Generic;

namespace D2P_Core.Interfaces
{
    public interface IDocMember
    {
        IEnumerable<IMember> Members { get; }

        bool Exists();
        void Commit();
        void Delete();
    }
}
