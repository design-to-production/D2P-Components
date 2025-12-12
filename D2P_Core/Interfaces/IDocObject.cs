namespace D2P_Core.Interfaces
{
    public interface IDocObject
    {
        bool Exists();
        void Commit();
        void Delete();
    }
}
