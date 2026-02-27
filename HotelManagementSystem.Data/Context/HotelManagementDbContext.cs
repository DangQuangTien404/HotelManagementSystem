using System;
using System.Collections.Generic;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Data.Context;

public partial class HotelManagementDbContext : DbContext
{
    public HotelManagementDbContext() { }

    public HotelManagementDbContext(DbContextOptions<HotelManagementDbContext> options)
        : base(options) { }

    public virtual DbSet<HotelService> HotelServices { get; set; }
    public virtual DbSet<CheckInOut> CheckInOuts { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<MaintenanceTask> MaintenanceTasks { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<Reservation> Reservations { get; set; }
    public virtual DbSet<ReservationService> ReservationServices { get; set; }
    public virtual DbSet<Room> Rooms { get; set; }
    public virtual DbSet<RoomCleaning> RoomCleanings { get; set; }
    public virtual DbSet<Staff> Staffs { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Ràng buộc Duy nhất cho Username
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // 2. Cấu hình kiểu dữ liệu Decimal cho tiền tệ
        modelBuilder.Entity<Room>(entity => {
            entity.Property(e => e.BasePrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
        });
        modelBuilder.Entity<HotelService>().Property(e => e.Price).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<ReservationService>().Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<CheckInOut>().Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");

        // 3. Sửa lỗi Notification (Recipient vs Sender)
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasOne(d => d.Recipient)
                .WithMany(p => p.NotificationRecipients)
                .HasForeignKey(d => d.RecipientId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Sender)
                .WithMany(p => p.NotificationSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // 4. Cấu hình MaintenanceTask (Đồng bộ với User.cs và các trường mới)
        modelBuilder.Entity<MaintenanceTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Priority).HasMaxLength(50);
            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.CompletedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Staff)
                .WithMany(p => p.MaintenanceTaskAssignedToNavigations)
                .HasForeignKey(d => d.AssignedTo);

            entity.HasOne(d => d.Room)
                .WithMany(p => p.MaintenanceTasks)
                .HasForeignKey(d => d.RoomId);
        });

        // 5. Cấu hình RoomCleaning
        modelBuilder.Entity<RoomCleaning>(entity =>
        {
            entity.HasOne(d => d.CleanedByNavigation)
                .WithMany(p => p.RoomCleanings)
                .HasForeignKey(d => d.CleanedBy);

            entity.HasOne(d => d.Room)
                .WithMany(p => p.RoomCleanings)
                .HasForeignKey(d => d.RoomId);
        });

        // 6. Cấu hình CheckInOut (Lỗi Navigation)
        modelBuilder.Entity<CheckInOut>(entity =>
        {
            entity.HasOne(d => d.CheckInByNavigation)
                .WithMany(p => p.CheckInOutCheckInByNavigations)
                .HasForeignKey(d => d.CheckInBy)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.CheckOutByNavigation)
                .WithMany(p => p.CheckInOutCheckOutByNavigations)
                .HasForeignKey(d => d.CheckOutBy)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // 7. Cấu hình Customer - User (One-to-Many logic, practically One-to-One for consumer)
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasOne(d => d.User)
                  .WithMany(p => p.Customers)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.SetNull); // If user is deleted, customer record remains (or can be cascade)
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}