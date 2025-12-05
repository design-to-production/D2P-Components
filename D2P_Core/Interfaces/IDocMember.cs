namespace D2P_Core.Interfaces
{
    public interface IDocMember
    {
        bool Exists();
        void Commit();
        void Delete();
    }
}
