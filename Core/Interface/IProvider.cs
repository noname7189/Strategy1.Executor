namespace Strategy1.Executor.Core.Interface
{
    public interface IProvider
    {
        public S AddStreamCore<S>()
            where S : IStreamCore, new();
    }
}
