using Microsoft.EntityFrameworkCore;
using SmartExpenseSplitter.Api.Models;

namespace SmartExpenseSplitter.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Group> Groups => Set<Group>();

    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();

    public DbSet<GroupJoinRequest> GroupJoinRequests => Set<GroupJoinRequest>();

    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<ExpenseSplit> ExpenseSplits => Set<ExpenseSplit>();

    public DbSet<ExpensePayment> ExpensePayments => Set<ExpensePayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Name).IsRequired().HasMaxLength(150);
            entity.Property(user => user.Email).IsRequired().HasMaxLength(255);
            entity.Property(user => user.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(user => user.CreatedAt).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(group => group.Id);
            entity.Property(group => group.Name).IsRequired().HasMaxLength(150);
            entity.Property(group => group.CreatedAt).IsRequired();

            entity.HasOne(group => group.CreatedByUser)
                .WithMany(user => user.CreatedGroups)
                .HasForeignKey(group => group.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(groupMember => groupMember.Id);

            entity.HasOne(groupMember => groupMember.User)
                .WithMany(user => user.GroupMemberships)
                .HasForeignKey(groupMember => groupMember.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(groupMember => groupMember.Group)
                .WithMany(group => group.Members)
                .HasForeignKey(groupMember => groupMember.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(groupMember => new { groupMember.UserId, groupMember.GroupId }).IsUnique();
        });

        modelBuilder.Entity<GroupJoinRequest>(entity =>
        {
            entity.HasKey(joinRequest => joinRequest.Id);
            entity.Property(joinRequest => joinRequest.Status).IsRequired();
            entity.Property(joinRequest => joinRequest.CreatedAt).IsRequired();

            entity.HasOne(joinRequest => joinRequest.Group)
                .WithMany(group => group.JoinRequests)
                .HasForeignKey(joinRequest => joinRequest.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(joinRequest => joinRequest.RequestedByUser)
                .WithMany(user => user.SentGroupJoinRequests)
                .HasForeignKey(joinRequest => joinRequest.RequestedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(joinRequest => joinRequest.TargetUser)
                .WithMany(user => user.ReceivedGroupJoinRequests)
                .HasForeignKey(joinRequest => joinRequest.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(joinRequest => new { joinRequest.GroupId, joinRequest.TargetUserId, joinRequest.Status });
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(expense => expense.Id);
            entity.Property(expense => expense.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(expense => expense.Description).IsRequired().HasMaxLength(500);
            entity.Property(expense => expense.Notes).HasMaxLength(1000);
            entity.Property(expense => expense.CreatedAt).IsRequired();

            entity.HasOne(expense => expense.Group)
                .WithMany(group => group.Expenses)
                .HasForeignKey(expense => expense.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(expense => expense.PaidByUser)
                .WithMany(user => user.PaidExpenses)
                .HasForeignKey(expense => expense.PaidBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ExpenseSplit>(entity =>
        {
            entity.HasKey(expenseSplit => expenseSplit.Id);
            entity.Property(expenseSplit => expenseSplit.Amount).HasPrecision(18, 2).IsRequired();

            entity.HasOne(expenseSplit => expenseSplit.Expense)
                .WithMany(expense => expense.Splits)
                .HasForeignKey(expenseSplit => expenseSplit.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(expenseSplit => expenseSplit.User)
                .WithMany(user => user.ExpenseSplits)
                .HasForeignKey(expenseSplit => expenseSplit.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(expenseSplit => new { expenseSplit.ExpenseId, expenseSplit.UserId }).IsUnique();
        });

        modelBuilder.Entity<ExpensePayment>(entity =>
        {
            entity.HasKey(expensePayment => expensePayment.Id);
            entity.Property(expensePayment => expensePayment.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(expensePayment => expensePayment.CreatedAt).IsRequired();

            entity.HasOne(expensePayment => expensePayment.Expense)
                .WithMany(expense => expense.Payments)
                .HasForeignKey(expensePayment => expensePayment.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(expensePayment => expensePayment.User)
                .WithMany(user => user.ExpensePayments)
                .HasForeignKey(expensePayment => expensePayment.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(expensePayment => new { expensePayment.ExpenseId, expensePayment.UserId }).IsUnique();
        });
    }
}
