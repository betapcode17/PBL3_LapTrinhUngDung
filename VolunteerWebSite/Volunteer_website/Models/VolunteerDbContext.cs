using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Volunteer_website.Models;

namespace Volunteer_website.Data;

public partial class VolunteerDbContext : DbContext
{
    public VolunteerDbContext()
    {
    }

    public VolunteerDbContext(DbContextOptions<VolunteerDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Donation> Donations { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventImage> EventImages { get; set; }

    public virtual DbSet<EventType> EventTypes { get; set; }

    public virtual DbSet<EventVolunteer> EventVolunteers { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Volunteer> Volunteers { get; set; }

    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        //modelBuilder.Entity<EventModel>(entity =>
        //{
        //    entity.ToTable("Event");
        //    entity.Property(e => e.EventDescription).HasColumnName("description"); // ánh xạ đúng cột
        //});

        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Account__B9BE370F7E94567D");

            entity.ToTable("Account");

            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("user_id");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .HasColumnName("user_name");

            entity.HasOne(d => d.User).WithOne(p => p.Account)
                .HasForeignKey<Account>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Account__user_id__398D8EEE");
        });

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__Admin__43AA41419FA2E9E5");

            entity.ToTable("Admin");

            entity.Property(e => e.AdminId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("admin_id");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasColumnName("image_path");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.AdminNavigation).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Admin__admin_id__3F466844");
        });

        modelBuilder.Entity<Donation>(entity =>
        {
            entity.HasKey(e => e.DonationId).HasName("PK__Donation__296B91DC5D06065D");

            entity.ToTable("Donation");

            entity.Property(e => e.DonationId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("donation_id");
            entity.Property(e => e.Amount)
                .HasColumnType("money")
                .HasColumnName("amount");
            entity.Property(e => e.DonationDate)
                .HasColumnType("datetime2")
                .HasColumnName("donation_date");
            entity.Property(e => e.EventId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("event_id");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.VolunteerId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("volunteer_id");

            entity.HasOne(d => d.Event).WithMany(p => p.Donations)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__Donation__event___4E88ABD4");

            entity.HasOne(d => d.Volunteer).WithMany(p => p.Donations)
                .HasForeignKey(d => d.VolunteerId)
                .HasConstraintName("FK__Donation__volunt__4D94879B");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Event__2370F72727C291FF");

            entity.ToTable("Event");

            entity.Property(e => e.EventId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("event_id");
            entity.Property(e => e.DayBegin).HasColumnName("day_begin");
            entity.Property(e => e.DayEnd).HasColumnName("day_end");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.OrgId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("org_id");
            entity.Property(e => e.TargetFunds)
                .HasColumnType("money")
                .HasColumnName("target_funds");
            entity.Property(e => e.TargetMember).HasColumnName("target_member");
            entity.Property(e => e.TypeEventId)
                .HasMaxLength(50)
                .IsUnicode(true)
                .HasColumnName("type_event_id");

            entity.HasOne(d => d.Org).WithMany(p => p.Events)
                .HasForeignKey(d => d.OrgId)
                .HasConstraintName("FK__Event__org_id__45F365D3");

            entity.HasOne(d => d.TypeEvent).WithMany(p => p.Events)
                .HasForeignKey(d => d.TypeEventId)
                .HasConstraintName("FK__Event__type_even__46E78A0C");
        });

        modelBuilder.Entity<EventImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EventIma__3213E83F2C73DA81");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EventId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("event_id");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("image_path");

            entity.HasOne(d => d.Event).WithMany(p => p.EventImages)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EventImag__event__5FB337D6");
        });

        modelBuilder.Entity<EventType>(entity =>
        {
            entity.HasKey(e => e.TypeEventId).HasName("PK__EventTyp__9AB5A4B04A1B82D4");

            entity.ToTable("EventType");

            entity.Property(e => e.TypeEventId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type_event_id");
            entity.Property(e => e.NameType)
                .HasMaxLength(100)
                .HasColumnName("name_type");
        });

        modelBuilder.Entity<EventVolunteer>(entity =>
        {
            entity.HasKey(e => new { e.EventId, e.VolunteerId }).HasName("PK__Event_Vo__238E814CFA7AFE71");

            entity.ToTable("Event_Volunteer");

            entity.Property(e => e.EventId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("event_id");
            entity.Property(e => e.VolunteerId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("volunteer_id");
            entity.Property(e => e.EventVolunteerDate).HasColumnName("Event_Volunteer_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.Event).WithMany(p => p.EventVolunteers)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Event_Vol__event__49C3F6B7");

            entity.HasOne(d => d.Volunteer).WithMany(p => p.EventVolunteers)
                .HasForeignKey(d => d.VolunteerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Event_Vol__volun__4AB81AF0");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.OrgId).HasName("PK__Organiza__F6AD8012BAAA9E30");

            entity.ToTable("Organization");

            entity.Property(e => e.OrgId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("org_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Description)
                .HasColumnType("nvarchar(max)")
                .HasColumnName("description");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasColumnName("image_path");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__B9BE370F7AFB61A5");

            entity.ToTable("User");

            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("user_id");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
        });

        modelBuilder.Entity<Volunteer>(entity =>
        {
            entity.HasKey(e => e.VolunteerId).HasName("PK__Voluntee__0FE766B176569259");

            entity.ToTable("Volunteer");

            entity.Property(e => e.VolunteerId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("volunteer_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(255)
                .HasColumnName("image_path");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");

            entity.HasOne(d => d.VolunteerNavigation).WithOne(p => p.Volunteer)
                .HasForeignKey<Volunteer>(d => d.VolunteerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Volunteer__volun__3C69FB99");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
