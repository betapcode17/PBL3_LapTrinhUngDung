using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Volunteer_website.Data;

public partial class VolunteerManagementContext : DbContext
{
    public VolunteerManagementContext()
    {
    }

    public VolunteerManagementContext(DbContextOptions<VolunteerManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Donation> Donations { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<Registration> Registrations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Volunteer> Volunteers { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Data Source=MSI\\SQLEXPRESS;Initial Catalog=VolunteerManagement;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;Connect Timeout=30");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__Admin__43AA4141C26052D9");

            entity.ToTable("Admin");

            entity.Property(e => e.AdminId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("admin_id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.ImgPath)
                .HasMaxLength(200)
                .HasColumnName("img_path");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Donation>(entity =>
        {
            entity.HasKey(e => e.DonationId).HasName("PK__Donation__296B91DC678E6A79");

            entity.ToTable("Donation");

            entity.Property(e => e.DonationId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("donation_id");
            entity.Property(e => e.Amount)
                .HasColumnType("money")
                .HasColumnName("amount");
            entity.Property(e => e.DonationDate).HasColumnName("donation_date");
            entity.Property(e => e.EventId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("event_id");
            entity.Property(e => e.Message)
                .HasMaxLength(500)
                .HasColumnName("message");
            entity.Property(e => e.VolunteerId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("volunteer_id");

            entity.HasOne(d => d.Event).WithMany(p => p.Donations)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__Donation__event___5AEE82B9");

            entity.HasOne(d => d.Volunteer).WithMany(p => p.Donations)
                .HasForeignKey(d => d.VolunteerId)
                .HasConstraintName("FK__Donation__volunt__59FA5E80");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Events__2370F72732470AFA");

            entity.Property(e => e.EventId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("event_id");
            entity.Property(e => e.DayBegin)
                .HasColumnType("datetime")
                .HasColumnName("day_begin");
            entity.Property(e => e.DayEnd)
                .HasColumnType("datetime")
                .HasColumnName("day_end");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(100)
                .HasColumnName("image_path");
            entity.Property(e => e.ListImg)
                .HasMaxLength(100)
                .HasColumnName("list_img");
            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .HasColumnName("location");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.OrgId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("org_id");
            entity.Property(e => e.Status)
                .HasDefaultValue(false)
                .HasColumnName("status");
            entity.Property(e => e.TargetFunds).HasColumnName("target_funds");
            entity.Property(e => e.TargetMember).HasColumnName("target_member");
            entity.Property(e => e.type_event_name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type_event_name");

            entity.HasOne(d => d.Org).WithMany(p => p.Events)
                .HasForeignKey(d => d.OrgId)
                .HasConstraintName("FK__Events__org_id__52593CB8");
        });

       

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.OrgId).HasName("PK__Organiza__F6AD8012AD205AE0");

            entity.ToTable("Organization");

            entity.Property(e => e.OrgId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("org_id");
            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .HasColumnName("address");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(50)
                .HasColumnName("image_path");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("phone_number");
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.RegId).HasName("PK__Registra__74038772661EDC80");

            entity.Property(e => e.RegId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("reg_id");
            entity.Property(e => e.EventId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("event_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.VolunteerId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("volunteer_id");

            entity.HasOne(d => d.Event).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK__Registrat__event__571DF1D5");

            entity.HasOne(d => d.Volunteer).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.VolunteerId)
                .HasConstraintName("FK__Registrat__volun__5629CD9C");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F052DC85A");

            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("user_id");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("role");
            entity.Property(e => e.UserName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("user_name");
            entity.Property(e => e.is_active) // Thêm dòng này
       .HasColumnName("is_active")
       .HasDefaultValue(true); // Nếu mặc định là true
        });

        modelBuilder.Entity<Volunteer>(entity =>
        {
            entity.HasKey(e => e.VolunteerId).HasName("PK__Voluntee__0FE766B1E87E1CB4");

            entity.Property(e => e.VolunteerId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("volunteer_id");
            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .HasColumnName("address");
            entity.Property(e => e.DateOfBirth)
                .HasColumnType("datetime")
                .HasColumnName("date_of_birth");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .HasColumnName("gender");
            entity.Property(e => e.ImagePath)
                .HasMaxLength(200)
                .HasColumnName("image_path");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("phone_number");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
