using DTOs.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<CheckInOut> CheckInOuts { get; set; }
        public DbSet<RoomCleaning> RoomCleanings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Money Precision
            modelBuilder.Entity<Room>()
                .Property(r => r.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CheckInOut>()
                .Property(c => c.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // --- UPDATED RELATIONSHIPS ---

            // 1. User (1) -> Staffs (Many)
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.User)
                .WithMany(u => u.Staffs) // Changed from WithOne to WithMany
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Reservation (1) -> CheckInOuts (Many)
            modelBuilder.Entity<CheckInOut>()
                .HasOne(c => c.Reservation)
                .WithMany(r => r.CheckInOuts) // Changed from WithOne to WithMany
                .HasForeignKey(c => c.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Keep other User FKs Restricted (Audit logs)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.ReservedByUser)
                .WithMany()
                .HasForeignKey(r => r.ReservedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CheckInOut>()
                .HasOne(c => c.CheckInStaff)
                .WithMany()
                .HasForeignKey(c => c.CheckInBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CheckInOut>()
                .HasOne(c => c.CheckOutStaff)
                .WithMany()
                .HasForeignKey(c => c.CheckOutBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoomCleaning>()
                .HasOne(rc => rc.Cleaner)
                .WithMany()
                .HasForeignKey(rc => rc.CleanedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}