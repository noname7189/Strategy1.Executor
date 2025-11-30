using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Strategy1.Executor.Core.Enum;

namespace Strategy1.Executor.Core.EntityClass
{
    [Index(nameof(StartTime), IsUnique = true)]
    [Index(nameof(EndTime))]
    public class BaseSignal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime StartTime { get; set; }
        public SignalType SignalType { get; set; }
        public DateTime? EndTime { get; set; }

        public decimal? EnterPrice { get; set; }
        public decimal? ExitPrice { get; set; }
        public decimal? ExpectedProfit { get; set; }
        public decimal TakeProfitPrice { get; set; }
        public decimal LosscutPrice { get; set; }

        [JsonIgnore]
        [Required]
        [ForeignKey(nameof(Candle))]
        public int CandleId { get; set; }

        [JsonIgnore]
        public virtual BaseCandle Candle { get; set; }
    }
}
