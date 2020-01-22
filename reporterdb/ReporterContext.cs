using Microsoft.EntityFrameworkCore;
using nsreporter.ReporterDB.Models;

namespace nsreporter.ReporterDB
{
     public class ReporterContext : DbContext
    {
        private string _connectionString;

        public DbSet<DatedScale> DatedScales { get; set; }
        public ReporterContext():this("Data Source=nsreporter.db")
        {

        }
        public ReporterContext(string connString)
        {            
            this._connectionString = connString;
            this.Database.EnsureCreated();

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(this._connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //     modelBuilder.Entity<IssOptResult>()
            //                 .HasKey(ior => new
            //                 {
            //                     ior.IDIssue,
            //                     ior.IDOption,
            //                     ior.IDRanking
            //                 });

            //     modelBuilder.Entity<Option>()
            //                 .HasKey(opt => new
            //                 {
            //                     opt.ID,
            //                     opt.IDIssue
            //                 });
        }
    }
}