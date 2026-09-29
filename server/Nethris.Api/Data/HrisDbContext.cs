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
    public DbSet<AuthSession> Sessions => Set<AuthSession>();
    public DbSet<OperationReceipt> Receipts => Set<OperationReceipt>();
    public DbSet<WorkshopSettings> WorkshopSettings => Set<WorkshopSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.EmployeeNumber).IsUnique();
            entity.Property(e => e.Email).UseCollation("NOCASE");
            entity.Property(e => e.EmployeeNumber).UseCollation("NOCASE");
            entity.Property(e => e.PtoBalanceHours).HasConversion(v => HrisRules.QuarterUnits(v, true, 10000), v => v / 4m);
            entity.HasOne(e => e.Manager).WithMany(e => e.DirectReports).HasForeignKey(e => e.ManagerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Department).WithMany(d => d.Employees).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.JobRole).WithMany(r => r.Employees).HasForeignKey(e => e.JobRoleId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("employee_balance", "PtoBalanceHours BETWEEN 0 AND 40000");
                t.HasCheckConstraint("employee_status", "Status IN (0,1,2)");
                t.HasCheckConstraint("employee_not_self_manager", "ManagerId IS NULL OR ManagerId <> Id");
            });
        });
        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(d => d.Name).UseCollation("NOCASE");
            entity.HasIndex(d => d.Name).IsUnique();
            entity.HasOne(d => d.LeadEmployee).WithMany().HasForeignKey(d => d.LeadEmployeeId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<JobRole>(entity =>
        {
            entity.Property(r => r.Title).UseCollation("NOCASE");
            entity.Property(r => r.Level).UseCollation("NOCASE");
            entity.HasIndex(r => new { r.Title, r.Level }).IsUnique();
            entity.Property(r => r.SalaryBandMin).HasConversion(v => HrisRules.Cents(v), v => v / 100m);
            entity.Property(r => r.SalaryBandMax).HasConversion(v => HrisRules.Cents(v), v => v / 100m);
            entity.ToTable(t => t.HasCheckConstraint("salary_band", "SalaryBandMin >= 0 AND SalaryBandMax >= SalaryBandMin AND SalaryBandMax <= 10000000000"));
        });
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.Property(u => u.Email).UseCollation("NOCASE");
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasOne(u => u.Employee).WithOne(e => e.UserAccount).HasForeignKey<UserAccount>(u => u.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t => t.HasCheckConstraint("account_role", "Role IN (0,1,2,3)"));
        });
        modelBuilder.Entity<PtoRequest>(entity =>
        {
            entity.Property(p => p.Hours).HasConversion(v => HrisRules.QuarterUnits(v, false, 240), v => v / 4m);
            entity.HasOne(p => p.Employee).WithMany(e => e.PtoRequests).HasForeignKey(p => p.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(p => p.AssignedManager).WithMany().HasForeignKey(p => p.AssignedManagerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(p => new { p.EmployeeId, p.Status, p.StartDate });
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("pto_hours", "Hours BETWEEN 1 AND 960");
                t.HasCheckConstraint("pto_dates", "EndDate >= StartDate");
                t.HasCheckConstraint("pto_status", "Status IN (0,1,2,3,4)");
            });
        });
        modelBuilder.Entity<PtoApproval>(entity =>
        {
            entity.HasOne(a => a.ApproverEmployee).WithMany().HasForeignKey(a => a.ApproverEmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.PtoRequest).WithMany(p => p.Approvals).HasForeignKey(a => a.PtoRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(a => new { a.PtoRequestId, a.Stage }).IsUnique();
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("approval_stage", "Stage IN ('Manager','Hr')");
                t.HasCheckConstraint("approval_decision", "Decision IN (0,1)");
            });
        });
        modelBuilder.Entity<AuthSession>().HasOne(s => s.UserAccount).WithMany().HasForeignKey(s => s.UserAccountId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<OperationReceipt>(entity =>
        {
            entity.HasIndex(r => new { r.UserAccountId, r.Key }).IsUnique();
            entity.HasOne(r => r.UserAccount).WithMany().HasForeignKey(r => r.UserAccountId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<WorkshopSettings>().Property(s => s.Id).ValueGeneratedNever();
        modelBuilder.Entity<AuditLog>().HasIndex(a => a.CreatedAt);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Supplied Guid keys represent new records, not database-generated identities.
            // This fixes EF treating a newly attached approval as an existing row.
            var id = entity.FindProperty("Id");
            if (id?.ClrType == typeof(Guid)) modelBuilder.Entity(entity.ClrType).Property("Id").ValueGeneratedNever();
            if (typeof(IVersioned).IsAssignableFrom(entity.ClrType))
                modelBuilder.Entity(entity.ClrType).Property(nameof(IVersioned.Version)).IsConcurrencyToken();
            foreach (var property in entity.GetProperties().Where(p => p.ClrType == typeof(DateTimeOffset)))
                modelBuilder.Entity(entity.ClrType).Property<DateTimeOffset>(property.Name)
                    .HasConversion(value => value.ToUnixTimeMilliseconds(), value => DateTimeOffset.FromUnixTimeMilliseconds(value));
        }
    }

    private void PrepareChanges()
    {
        ChangeTracker.DetectChanges();
        foreach (var entry in ChangeTracker.Entries<IVersioned>())
        {
            if (entry.State == EntityState.Added) entry.Entity.Version = 1;
            else if (entry.State == EntityState.Modified)
                entry.Entity.Version = checked(entry.OriginalValues.GetValue<long>(nameof(IVersioned.Version)) + 1);
        }
    }
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    { PrepareChanges(); return base.SaveChanges(acceptAllChangesOnSuccess); }
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    { PrepareChanges(); return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken); }
}
