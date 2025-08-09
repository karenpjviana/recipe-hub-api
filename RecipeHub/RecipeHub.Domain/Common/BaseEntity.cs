namespace RecipeHub.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedOnUtc { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedOnUtc { get; set; }
}