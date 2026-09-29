using Microsoft.EntityFrameworkCore;
using Nethris.Api.Domain;

namespace Nethris.Api.Data;

public sealed class HrisDbContext(DbContextOptions<HrisDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<JobRole> JobRoles => Set<JobRole>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<PtoRequest> PtoRequests => Set<PtoRequest>();
    public DbSet<PtoApproval> PtoApprovals => Set<PtoApproval>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.EmployeeNumber).IsUnique();
            entity.Property(e => e.PtoBalanceHours).HasPrecision(8, 2);
            entity.HasOne(e => e.Manager)
                .WithMany(e => e.DirectReports)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.JobRole)
                .WithMany(r => r.Employees)
                .HasForeignKey(e => e.JobRoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Department>()
            .HasOne(d => d.LeadEmployee)
            .WithMany()
            .HasForeignKey(d => d.LeadEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<JobRole>(entity =>
        {
            entity.Property(r => r.SalaryBandMin).HasPrecision(12, 2);
            entity.Property(r => r.SalaryBandMax).HasPrecision(12, 2);
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasOne(u => u.Employee)
                .WithOne(e => e.UserAccount)
                .HasForeignKey<UserAccount>(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PtoRequest>(entity =>
        {
            entity.Property(p => p.Hours).HasPrecision(8, 2);
            entity.HasOne(p => p.Employee)
                .WithMany(e => e.PtoRequests)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PtoApproval>()
            .HasOne(a => a.ApproverEmployee)
            .WithMany()
            .HasForeignKey(a => a.ApproverEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

