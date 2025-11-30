using Strategy1.Executor.Core.Enum;

namespace Strategy1.Executor.Core.Converter.DependentConverter
{
    public class UpdateReasonConverter : BaseConverter<UpdateReason>
    {
        private static readonly List<KeyValuePair<UpdateReason, string>> Values =
        [
            new KeyValuePair<UpdateReason, string>(UpdateReason.FundingFee, "FUNDING_FEE"),
        ];
        public override List<KeyValuePair<UpdateReason, string>> Mapping => Values;
    }
}
