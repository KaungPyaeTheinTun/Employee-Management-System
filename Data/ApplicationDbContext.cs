using EmployeesManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using EmployeeManagement.Models;

namespace EmployeeManagement.Data
{
    // Inherit from IdentityDbContext if you are using Identity
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LeaveApplication>()
                .HasOne(l => l.Duration)
                .WithMany()
                .HasForeignKey(l => l.DurationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LeaveApplication>()
                .HasOne(l => l.Status)
                .WithMany()
                .HasForeignKey(l => l.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CompanyInformation>()
                .HasOne(c => c.Country)
                .WithMany()
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CompanyInformation>()
                .HasOne(c => c.City)
                .WithMany()
                .HasForeignKey(c => c.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApprovalEntry>()
                .HasOne(a => a.Approver)
                .WithMany()
                .HasForeignKey(a => a.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApprovalEntry>()
                .HasOne(a => a.LastModifiedBy)
                .WithMany()
                .HasForeignKey(a => a.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApprovalEntry>()
                .HasOne(a => a.DocumentType)
                .WithMany()
                .HasForeignKey(a => a.DocumentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApprovalEntry>()
                .HasOne(a => a.Status)
                .WithMany()
                .HasForeignKey(a => a.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkFlowUserGroupMember>()
                .HasOne(w => w.Sender)
                .WithMany()
                .HasForeignKey(w => w.SenderId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<WorkFlowUserGroupMember>()
                .HasOne(w => w.Approver)
                .WithMany()
                .HasForeignKey(w => w.ApproverId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<WorkFlowUserGroupMember>()
                .HasOne(w => w.WorkFlowUserGroup)
                .WithMany()
                .HasForeignKey(w => w.WorkFlowUserGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<Bank> Banks { get; set; }  
        public DbSet<SystemCode> SystemCodes { get; set; }
        public DbSet<SystemCodeDetail> SystemCodeDetails { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<LeaveApplication> LeaveApplications { get; set; }
        public DbSet<SystemProfile> SystemProfiles { get; set; }
        public DbSet<Audit> AuditLogs { get; set; }
        public DbSet<RoleProfile> RoleProfiles { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<LeaveAdjustmentEntry> LeaveAdjustmentEntries { get; set; }
        public DbSet<LeavePeriod> LeavePeriods { get; set; }
        public DbSet<CompanyInformation> CompanyInformations { get; set; }
        public DbSet<ApprovalEntry> ApprovalEntries { get; set; }
        public DbSet<WorkFlowUserGroup> WorkFlowUserGroups { get; set; }
        public DbSet<WorkFlowUserGroupMember> WorkFlowUserGroupMembers { get; set; }
        public DbSet<ApprovalsUserMatrix> ApprovalsUserMatrixs { get; set; }
        public virtual async Task<int> SaveChangesAsync(string userId)
        {
            OnBeforeSavingChanges(userId);
            var result = await base.SaveChangesAsync();
            return result;
        }

        private void OnBeforeSavingChanges(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("UserId is required for auditing.");
            }

            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Audit ||
                    entry.State == EntityState.Detached ||
                    entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry);
                auditEntry.TableName = entry.Entity.GetType().Name;
                auditEntry.UserId = userId;

                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;

                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.AuditType = AuditType.Create;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            auditEntry.AuditType = AuditType.Delete;
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.AuditType = AuditType.Update;
                                auditEntry.ChangeColumns.Add(propertyName);
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }

                auditEntries.Add(auditEntry);
            }

            foreach (var auditEntry in auditEntries)
            {
                AuditLogs.Add(auditEntry.ToAudit());
            }
        }
    }
}