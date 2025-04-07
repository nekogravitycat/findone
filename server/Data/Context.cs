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

            // set RoomId as foreign key in User
            modelBuilder.Entity<Room>()
                .HasMany(r => r.Users)
                .WithOne(u => u.Room)
                .HasForeignKey(u => u.RoomId);

            // set RoomId as foreign key in RoomTarget
            modelBuilder.Entity<RoomTarget>()
                .HasKey(rt => rt.Id);
            modelBuilder.Entity<RoomTarget>()
                .HasOne(rt => rt.Room)
                .WithMany(r => r.Targets)
                .HasForeignKey(rt => rt.RoomId);

            // set UserId as foreign key in UserScore
            modelBuilder.Entity<UserScore>()
                .HasKey(us => us.Id);
            modelBuilder.Entity<UserScore>()
                .HasOne(us => us.User)
                .WithMany(u => u.Scores)
                .HasForeignKey(us => us.UserId);
        }
    }
}
