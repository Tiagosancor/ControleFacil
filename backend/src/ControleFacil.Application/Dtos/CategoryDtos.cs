using ControleFacil.Domain.Enums;

namespace ControleFacil.Application.Dtos;

public record CategoryCreateDto(string Name, CategoryType Type, int? ParentCategoryId);

public record CategoryUpdateDto(string Name, CategoryType Type, int? ParentCategoryId, bool IsActive);

public record CategoryResponseDto(
    int Id,
    string Name,
    CategoryType Type,
    int? ParentCategoryId,
    string? ParentCategoryName,
    bool IsActive,
    bool IsSystem = false,
    string? IconKey = null,
    string? Color = null);
