using System;
using System.Collections.Generic;
using HotelManagementSystem.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Data.Context;

public partial class HotelManagementDbContext : DbContext
{
    public HotelManagementDbContext()
    {
    }

    public HotelManagementDbContext(DbContextOptions<HotelManagementDbContext> options)
        : base(options)
    {
    }
    public DbSet<HotelService> HotelServices { get; set; }
    public virtual DbSet<CheckInOut> CheckInOuts { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<MaintenanceTask> MaintenanceTasks { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomCleaning> RoomCleanings { get; set; }

    public virtual DbSet<Staff> Staffs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>()
        .Property(r => r.BasePrice)
        .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<HotelService>()
            .Property(s => s.Price)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<CheckInOut>(entity =>
        {
            entity.HasIndex(e => e.CheckInBy, "IX_CheckInOuts_CheckInBy");

            entity.HasIndex(e => e.CheckOutBy, "IX_CheckInOuts_CheckOutBy");

            entity.HasIndex(e => e.ReservationId, "IX_CheckInOuts_ReservationId");

            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.CheckInByNavigation).WithMany(p => p.CheckInOutCheckInByNavigations).HasForeignKey(d => d.CheckInBy);

            entity.HasOne(d => d.CheckOutByNavigation).WithMany(p => p.CheckInOutCheckOutByNavigations).HasForeignKey(d => d.CheckOutBy);

            entity.HasOne(d => d.Reservation).WithMany(p => p.CheckInOuts).HasForeignKey(d => d.ReservationId);
        });

        modelBuilder.Entity<MaintenanceTask>(entity =>
        {
            entity.HasIndex(e => e.ApprovedBy, "IX_MaintenanceTasks_ApprovedBy");

            entity.HasIndex(e => e.AssignedTo, "IX_MaintenanceTasks_AssignedTo");

            entity.HasIndex(e => e.RoomId, "IX_MaintenanceTasks_RoomId");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.MaintenanceTaskApprovedByNavigations).HasForeignKey(d => d.ApprovedBy);

            entity.HasOne(d => d.AssignedToNavigation).WithMany(p => p.MaintenanceTaskAssignedToNavigations).HasForeignKey(d => d.AssignedTo);

            entity.HasOne(d => d.Room).WithMany(p => p.MaintenanceTasks).HasForeignKey(d => d.RoomId);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => e.RecipientId, "IX_Notifications_RecipientId");

            entity.HasIndex(e => e.SenderId, "IX_Notifications_SenderId");

            entity.HasOne(d => d.Recipient).WithMany(p => p.NotificationRecipients).HasForeignKey(d => d.RecipientId);

            entity.HasOne(d => d.Sender).WithMany(p => p.NotificationSenders).HasForeignKey(d => d.SenderId);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasIndex(e => e.CustomerId, "IX_Reservations_CustomerId");

            entity.HasIndex(e => e.ReservedBy, "IX_Reservations_ReservedBy");

            entity.HasIndex(e => e.RoomId, "IX_Reservations_RoomId");

            entity.HasOne(d => d.Customer).WithMany(p => p.Reservations).HasForeignKey(d => d.CustomerId);

            entity.HasOne(d => d.ReservedByNavigation).WithMany(p => p.Reservations).HasForeignKey(d => d.ReservedBy);

            entity.HasOne(d => d.Room).WithMany(p => p.Reservations).HasForeignKey(d => d.RoomId);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<RoomCleaning>(entity =>
        {
            entity.HasIndex(e => e.CleanedBy, "IX_RoomCleanings_CleanedBy");

            entity.HasIndex(e => e.RoomId, "IX_RoomCleanings_RoomId");

            entity.HasOne(d => d.CleanedByNavigation).WithMany(p => p.RoomCleanings).HasForeignKey(d => d.CleanedBy);

            entity.HasOne(d => d.Room).WithMany(p => p.RoomCleanings).HasForeignKey(d => d.RoomId);
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Staffs_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.Staff).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
