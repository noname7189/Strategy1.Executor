using Strategy1.Executor.Core.DataClass.Rest;
using Strategy1.Executor.Core.DataClass.Stream;
using Strategy1.Executor.Core.Enum;

namespace Strategy1.Executor.Core.Provider
{
    public record ProviderConfiguration
    {
        public required Exchange Exchange { get; init; }
        public required string PublicKey { get; init; }
        public required string SecretKey { get; init; }

        public required Action<AccountStreamRecv>? OnAccountUpdate { get; init; }
        public required Action<AccountInfo>? OnGetAccountInfo { get; init; }
        public required Action<BaseStreamRecv>? OnListenKeyExpired { get; init; }
    }
}
