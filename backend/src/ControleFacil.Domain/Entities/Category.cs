using ControleFacil.Domain.Enums;

namespace ControleFacil.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public CategoryType Type { get; set; }
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public int UserId { get; set; }
    public User? User { get; set; }
    public bool IsActive { get; set; } = true;
}
