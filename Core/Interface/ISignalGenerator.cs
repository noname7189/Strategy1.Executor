using Strategy1.Executor.Core.EntityClass;

namespace Strategy1.Executor.Core.Interface
{
    public interface ISignalGenerator<S>
        where S : BaseSignal
    {
        public List<S> GetOnlineSignals();
        public List<S> GenerateSignalsDuringSystemOff(int startIndex);
        public Task FinalizeFinishedSignals(List<S> finishedSignals);
        public List<S> TryTerminateResidualSignals();
        public S? AddInmemorySignal();
        public void InitializeGeneratedSignals(List<S> generatedSignals);
    }
}
