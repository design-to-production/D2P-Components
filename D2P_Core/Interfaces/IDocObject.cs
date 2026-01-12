namespace D2P_Core.Interfaces {
    public interface IDocObject<T> {
        bool Exists();
        void Delete();
        void Commit();
        T Duplicate();
    }
}
