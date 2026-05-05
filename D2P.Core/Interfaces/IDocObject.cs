namespace D2P.Core.Interfaces {
    public interface IDocObject<T> {
        bool Exists();
        void Delete();
        void Commit(bool deleteExisting);
        T Duplicate();
    }
}
