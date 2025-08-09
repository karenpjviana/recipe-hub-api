using System.Text.RegularExpressions;
using RecipeHub.Domain.Common;
using RecipeHub.Domain.Enums;

namespace RecipeHub.Domain.Entities;

public class Recipe : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Description { get; set; }
    public int? PrepTime { get; set; }
    public int? Servings { get; set; }
    public string? Difficulty { get; set; }
    public Guid? ImageId { get; set; }
    public ImageStorage? Image { get; set; }
    public bool IsPublished { get; set; }

    public ICollection<Instruction> Instructions { get; set; } = new List<Instruction>();
    public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    public ICollection<RecipeTag> RecipeTags { get; set; } = new List<RecipeTag>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    private Recipe() { }

    public static Recipe Create(
        Guid userId,
        Guid? categoryId,
        string title,
        string? description,
        int? prepTime,
        int? servings,
        string? difficulty,
        Guid? imageId,
        bool isPublished,
        List<Instruction> instructions,
        List<Ingredient> ingredients,
        List<RecipeTag> recipeTags)
    {
        var recipe = new Recipe
        {
            UserId = userId,
            CategoryId = categoryId,
            Title = title,
            Slug = GenerateSlug(title),
            Description = description,
            PrepTime = prepTime,
            Servings = servings,
            Difficulty = difficulty,
            ImageId = imageId,
            IsPublished = isPublished,
            Instructions = instructions,
            Ingredients = ingredients,
            RecipeTags = recipeTags,
            CreatedOnUtc = DateTime.UtcNow,
            IsDeleted = false
        };

        return recipe;
    }

    public void Update(
        Guid? categoryId,
        string title,
        string? description,
        int? prepTime,
        int? servings,
        string? difficulty,
        Guid? imageId,
        bool isPublished,
        List<Instruction> instructions,
        List<Ingredient> ingredients,
        List<RecipeTag> recipeTags)
    {
        CategoryId = categoryId;
        Title = title;
        Slug = GenerateSlug(title);
        Description = description;
        PrepTime = prepTime;
        Servings = servings;
        Difficulty = difficulty;
        ImageId = imageId;
        IsPublished = isPublished;

        Instructions = instructions;
        Ingredients = ingredients;
        RecipeTags = recipeTags;

        UpdatedOnUtc = DateTime.UtcNow;
    }

    private static string GenerateSlug(string phrase)
    {
        string str = phrase.ToLowerInvariant();
        // Remove invalid chars
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        // Convert multiple spaces into one space
        str = Regex.Replace(str, @"\s+", " ").Trim();
        // Replace spaces with hyphens
        str = Regex.Replace(str, @"\s", "-");
        return str;
    }
}
