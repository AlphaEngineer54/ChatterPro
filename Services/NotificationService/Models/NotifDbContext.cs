using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Models;

public partial class NotifDbContext : DbContext
{
    public NotifDbContext()
    {
    }

    public NotifDbContext(DbContextOptions<NotifDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Notification> Notifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("Notification");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Message)
                .HasMaxLength(255)
                .HasColumnName("message");
            entity.Property(e => e.UserId).HasColumnName("userId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
