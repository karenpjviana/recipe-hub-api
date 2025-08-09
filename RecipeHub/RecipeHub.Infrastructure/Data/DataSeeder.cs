using RecipeHub.Domain.Entities;
using RecipeHub.Infrastructure.Data;

namespace RecipeHub.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(RecipeDbContext context)
    {
        if (!context.Categories.Any())
        {
            var categories = new List<Category>
            {
                new() { Name = "Sobremesas", Description = "Receitas doces e sobremesas" },
                new() { Name = "Massas", Description = "Receitas com massas" },
                new() { Name = "Carnes", Description = "Receitas com carnes" },
                new() { Name = "Vegetariano", Description = "Receitas vegetarianas" },
                new() { Name = "Bebidas", Description = "Receitas de bebidas" }
            };

            await context.Categories.AddRangeAsync(categories);
        }

        if (!context.Users.Any())
        {
            var users = new List<User>
            {
                new() { Username = "admin", FullName = "Administrador", Role = "Admin", PasswordHash = "$2a$11$rQZJ5AuFcD1pJzlcWxN6ZOW8rZo0IjU8Zo6VlvF5K7Kd8pMjnGPc." }, // admin123
                new() { Username = "chef_maria", FullName = "Maria Silva", Role = "User", PasswordHash = "$2a$11$YoNlzJvp8xd9VkWqJKzJZ.xJhQ4WzLJmKqHl9LfFwJc.o5k8LcMrq" }, // password123
                new() { Username = "chef_joao", FullName = "João Santos", Role = "User", PasswordHash = "$2a$11$YoNlzJvp8xd9VkWqJKzJZ.xJhQ4WzLJmKqHl9LfFwJc.o5k8LcMrq" }, // password123
                new() { Username = "chef_ana", FullName = "Ana Costa", Role = "User", PasswordHash = "$2a$11$YoNlzJvp8xd9VkWqJKzJZ.xJhQ4WzLJmKqHl9LfFwJc.o5k8LcMrq" } // password123
            };

            await context.Users.AddRangeAsync(users);
        }

        if (!context.Tags.Any())
        {
            var tags = new List<Tag>
            {
                new() { Name = "Fácil" },
                new() { Name = "Rápido" },
                new() { Name = "Saudável" },
                new() { Name = "Vegetariano" },
                new() { Name = "Sem Glúten" },
                new() { Name = "Low Carb" }
            };

            await context.Tags.AddRangeAsync(tags);
        }

        await context.SaveChangesAsync();
    }
}
