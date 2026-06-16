using AllChecksOut.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllChecksOut.Infrastructure;

public class AllChecksOutDbContext(DbContextOptions<AllChecksOutDbContext> options) : DbContext(options)
{
    public DbSet<SystemOwner> SystemOwners => Set<SystemOwner>();
    public DbSet<Authority> Authorities => Set<Authority>();
    public DbSet<AuthorityTerminology> AuthorityTerminologies => Set<AuthorityTerminology>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Stakeholder> Stakeholders => Set<Stakeholder>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<SystemOwnerUser> SystemOwnerUsers => Set<SystemOwnerUser>();
    public DbSet<AuthorityUser> AuthorityUsers => Set<AuthorityUser>();
    public DbSet<ParticipantUser> ParticipantUsers => Set<ParticipantUser>();
    public DbSet<StakeholderUser> StakeholderUsers => Set<StakeholderUser>();
    public DbSet<AgentUser> AgentUsers => Set<AgentUser>();
    public DbSet<StakeholderParticipantAccess> StakeholderParticipantAccesses => Set<StakeholderParticipantAccess>();
    public DbSet<AccessGrant> AccessGrants => Set<AccessGrant>();
    public DbSet<TaskType> TaskTypes => Set<TaskType>();
    public DbSet<CaseTemplate> CaseTemplates => Set<CaseTemplate>();
    public DbSet<TemplateTask> TemplateTasks => Set<TemplateTask>();
    public DbSet<CaseTemplateParticipant> CaseTemplateParticipants => Set<CaseTemplateParticipant>();
    public DbSet<Case> Cases => Set<Case>();
    public DbSet<AllChecksOut.Domain.Task> Tasks => Set<AllChecksOut.Domain.Task>();
    public DbSet<StakeholderReview> StakeholderReviews => Set<StakeholderReview>();
    public DbSet<RequestForInformation> RequestsForInformation => Set<RequestForInformation>();
    public DbSet<ParticipantSupplier> ParticipantSuppliers => Set<ParticipantSupplier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureEntity(modelBuilder.Entity<SystemOwner>());
        ConfigureEntity(modelBuilder.Entity<Authority>());
        ConfigureEntity(modelBuilder.Entity<AuthorityTerminology>());
        ConfigureEntity(modelBuilder.Entity<Participant>());
        ConfigureEntity(modelBuilder.Entity<Stakeholder>());
        ConfigureEntity(modelBuilder.Entity<Agent>());
        ConfigureEntity(modelBuilder.Entity<UserAccount>());
        ConfigureMembership(modelBuilder.Entity<SystemOwnerUser>());
        ConfigureMembership(modelBuilder.Entity<AuthorityUser>());
        ConfigureMembership(modelBuilder.Entity<ParticipantUser>());
        ConfigureMembership(modelBuilder.Entity<StakeholderUser>());
        ConfigureMembership(modelBuilder.Entity<AgentUser>());
        ConfigureEntity(modelBuilder.Entity<StakeholderParticipantAccess>());
        ConfigureEntity(modelBuilder.Entity<AccessGrant>());
        ConfigureEntity(modelBuilder.Entity<TaskType>());
        ConfigureEntity(modelBuilder.Entity<CaseTemplate>());
        ConfigureEntity(modelBuilder.Entity<TemplateTask>());
        ConfigureEntity(modelBuilder.Entity<CaseTemplateParticipant>());
        ConfigureEntity(modelBuilder.Entity<Case>());
        ConfigureEntity(modelBuilder.Entity<AllChecksOut.Domain.Task>());
        ConfigureEntity(modelBuilder.Entity<StakeholderReview>());
        ConfigureEntity(modelBuilder.Entity<RequestForInformation>());
        ConfigureEntity(modelBuilder.Entity<ParticipantSupplier>());

        modelBuilder.Entity<SystemOwner>().ToTable("SystemOwners");
        modelBuilder.Entity<Authority>().ToTable("Authorities");
        modelBuilder.Entity<AuthorityTerminology>().ToTable("AuthorityTerminologies");
        modelBuilder.Entity<Participant>().ToTable("Participants");
        modelBuilder.Entity<Stakeholder>().ToTable("Stakeholders");
        modelBuilder.Entity<Agent>().ToTable("Agents");
        modelBuilder.Entity<UserAccount>().ToTable("UserAccounts");
        modelBuilder.Entity<SystemOwnerUser>().ToTable("SystemOwnerUsers");
        modelBuilder.Entity<AuthorityUser>().ToTable("AuthorityUsers");
        modelBuilder.Entity<ParticipantUser>().ToTable("ParticipantUsers");
        modelBuilder.Entity<StakeholderUser>().ToTable("StakeholderUsers");
        modelBuilder.Entity<AgentUser>().ToTable("AgentUsers");
        modelBuilder.Entity<StakeholderParticipantAccess>().ToTable("StakeholderParticipantAccesses");
        modelBuilder.Entity<AccessGrant>().ToTable("AccessGrants");
        modelBuilder.Entity<TaskType>().ToTable("TaskTypes");
        modelBuilder.Entity<CaseTemplate>().ToTable("CaseTemplates");
        modelBuilder.Entity<TemplateTask>().ToTable("TemplateTasks");
        modelBuilder.Entity<CaseTemplateParticipant>().ToTable("CaseTemplateParticipants");
        modelBuilder.Entity<Case>().ToTable("Cases");
        modelBuilder.Entity<AllChecksOut.Domain.Task>().ToTable("Tasks");
        modelBuilder.Entity<StakeholderReview>().ToTable("StakeholderReviews");
        modelBuilder.Entity<RequestForInformation>().ToTable("RequestsForInformation");
        modelBuilder.Entity<ParticipantSupplier>().ToTable("ParticipantSuppliers");

        ConfigureStrings(modelBuilder, Database.IsSqlServer());
        AllChecksOutSeedData.Seed(modelBuilder);
    }

    private static void ConfigureEntity<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : Entity
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).HasMaxLength(120);
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.UpdatedAt).IsRequired();
    }

    private static void ConfigureMembership<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : Membership
    {
        ConfigureEntity(builder);
        builder.HasIndex(entity => new { entity.EntityId, entity.UserAccountId }).IsUnique();
    }

    private static void ConfigureStrings(ModelBuilder modelBuilder, bool isSqlServer)
    {
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(entity => entity.GetProperties()))
        {
            if (property.ClrType == typeof(string) && property.GetMaxLength() is null)
            {
                property.SetMaxLength(450);
            }
        }

        if (isSqlServer)
        {
            modelBuilder.Entity<AuthorityTerminology>().Property(entity => entity.LabelsJson).HasColumnType("nvarchar(max)");
            modelBuilder.Entity<TaskType>().Property(entity => entity.ParameterSchemaJson).HasColumnType("nvarchar(max)");
            modelBuilder.Entity<TemplateTask>().Property(entity => entity.ParametersJson).HasColumnType("nvarchar(max)");
            modelBuilder.Entity<AllChecksOut.Domain.Task>().Property(entity => entity.ResponseJson).HasColumnType("nvarchar(max)");
            modelBuilder.Entity<AllChecksOut.Domain.Task>().Property(entity => entity.EvidenceJson).HasColumnType("nvarchar(max)");
            modelBuilder.Entity<RequestForInformation>().Property(entity => entity.StatusHistoryJson).HasColumnType("nvarchar(max)");
        }

        modelBuilder.Entity<UserAccount>().HasIndex(entity => entity.Email).IsUnique();
        modelBuilder.Entity<UserAccount>().HasIndex(entity => entity.EntraObjectId).IsUnique();
        modelBuilder.Entity<AuthorityTerminology>().HasIndex(entity => entity.AuthorityId).IsUnique();
        modelBuilder.Entity<TaskType>().HasIndex(entity => entity.Code).IsUnique();
    }
}
