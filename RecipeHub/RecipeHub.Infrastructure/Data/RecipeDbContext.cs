using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RecipeHub.Domain.Common;
using RecipeHub.Domain.Entities;

namespace RecipeHub.Infrastructure.Data;

public class RecipeDbContext : DbContext
{
    public RecipeDbContext(DbContextOptions<RecipeDbContext> options) : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Instruction> Instructions => Set<Instruction>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<RecipeTag> RecipeTags => Set<RecipeTag>();
    public DbSet<ImageStorage> ImageStorage => Set<ImageStorage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar chave composta para Favorite
        modelBuilder.Entity<Favorite>()
            .HasKey(f => new { f.UserId, f.RecipeId });

        // Configurar chave composta para RecipeTag
        modelBuilder.Entity<RecipeTag>()
            .HasKey(rt => new { rt.RecipeId, rt.TagId });

        // Configurar relacionamentos de ImageStorage
        modelBuilder.Entity<ImageStorage>()
            .HasOne(i => i.UploadedByUser)
            .WithMany()
            .HasForeignKey(i => i.UploadedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configurar relacionamento User -> AvatarImage
        modelBuilder.Entity<User>()
            .HasOne(u => u.AvatarImage)
            .WithMany()
            .HasForeignKey(u => u.AvatarImageId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configurar relacionamento Recipe -> Image
        modelBuilder.Entity<Recipe>()
            .HasOne(r => r.Image)
            .WithMany()
            .HasForeignKey(r => r.ImageId)
            .OnDelete(DeleteBehavior.SetNull);

        // Excluir entidades marcadas como soft deleted
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(RecipeHub.Domain.Common.BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
            }
        }
    }

    private static LambdaExpression ConvertFilterExpression(Type type)
    {
        var param = Expression.Parameter(type, "e");
        var prop = Expression.Property(param, nameof(BaseEntity.IsDeleted));
        var body = Expression.Equal(prop, Expression.Constant(false));
        return Expression.Lambda(body, param);
    }
}