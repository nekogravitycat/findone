using server.Models;
using Microsoft.EntityFrameworkCore;

namespace server.Data
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomTarget> RoomTargets { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserScore> UserScores { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Room primary key
            modelBuilder.Entity<Room>()
                .HasKey(r => r.RoomId);

            // User primary key
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);
        }
    }
}
