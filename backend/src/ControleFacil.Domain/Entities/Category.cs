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
    public int? UserId { get; set; }
    public User? User { get; set; }
    public bool IsActive { get; set; } = true;

    // Categoria de sistema: fixa, compartilhada por todos os usuários (UserId = null),
    // não editável/excluível pelo usuário comum — ver CategoryService.
    public bool IsSystem { get; set; } = false;
    public string? IconKey { get; set; }
    public string? Color { get; set; }
}
