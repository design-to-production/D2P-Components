using System;

namespace D2P_Core.Interfaces
{
    public interface IDocObject : ICloneable
    {
        bool Exists();
        void Commit();
        void Delete();
    }
}
