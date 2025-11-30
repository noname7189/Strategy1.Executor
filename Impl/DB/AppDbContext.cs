using Microsoft.EntityFrameworkCore;
using Strategy1.Executor.Impl.DB.Entity;
using Strategy1.Executor.Impl.DB.Entity.ETH5M;

namespace Strategy1.Executor.Impl.DB
{
    public class AppDbContext : DbContext
    {
        public DbSet<ETH5M_Candle> ETH5M_Candle { get; set; }
        public DbSet<ETH5M_Indicator> ETH5M_Indicator { get; set; }
        public DbSet<ETH5M_Signal> ETH5M_Signal { get; set; }
        public DbSet<ETH5M_Order> ETH5M_Order { get; set; }
        public DbSet<TradeContext> TradeContext { get; set; }

        public const string Target = @"192.168.0.2";
        public const string ConnectionString =
            $"server={Target};database=tgen;user=root;password=1234";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseMySql(ConnectionString, new MySqlServerVersion(new Version(8, 0, 23)));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName()?.ToLower());
            }
        }
    }
}
