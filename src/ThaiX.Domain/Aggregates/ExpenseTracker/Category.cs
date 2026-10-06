using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.ExpenseTracker;

public sealed class Category : BaseAuditableEntity
{
    private readonly List<Category> _children = new();

    private Category()
    {
    }

    public string Name { get; private set; } = null!;
    public CategoryType CategoryType { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public Category? ParentCategory { get; private set; }
    public IReadOnlyCollection<Category> Children => _children.AsReadOnly();

    public static Category Create(string name, CategoryType categoryType, Guid? parentCategoryId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            CategoryType = categoryType,
            ParentCategoryId = parentCategoryId
        };
    }

    public void Update(string name, CategoryType categoryType, Guid? parentCategoryId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (parentCategoryId.HasValue && parentCategoryId.Value == Id)
            throw new InvalidOperationException("Category parent cannot self-reference.");

        Name = name.Trim();
        CategoryType = categoryType;
        ParentCategoryId = parentCategoryId;
    }

    public void SoftDelete()
    {
        Delete();
    }
}
