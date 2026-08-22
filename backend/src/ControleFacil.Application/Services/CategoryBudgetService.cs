using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Application.Services;

public class CategoryBudgetService : ICategoryBudgetService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CategoryBudgetService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CategoryBudgetResponseDto>> GetAllAsync(int? year, int? month)
    {
        var query = _unitOfWork.CategoryBudgets.QueryWithDetails().Where(b => b.UserId == _currentUser.UserId);
        if (year.HasValue)
            query = query.Where(b => b.Year == year.Value);
        if (month.HasValue)
            query = query.Where(b => b.Month == month.Value);

        var budgets = await query.OrderBy(b => b.Year).ThenBy(b => b.Month).ThenBy(b => b.Category!.Name).ToListAsync();
        var result = new List<CategoryBudgetResponseDto>(budgets.Count);
        foreach (var budget in budgets)
            result.Add(await ToDtoAsync(budget));

        return result;
    }

    public async Task<CategoryBudgetResponseDto> GetByIdAsync(int id)
    {
        var budget = await GetOwnedAsync(id);
        return await ToDtoAsync(budget);
    }

    public async Task<CategoryBudgetResponseDto> CreateAsync(CategoryBudgetCreateDto dto)
    {
        await EnsureBudgetableCategoryAsync(dto.CategoryId);
        await EnsureNoDuplicateAsync(dto.CategoryId, dto.Year, dto.Month, excludingId: null);

        var now = DateTime.UtcNow;
        var budget = new CategoryBudget
        {
            CategoryId = dto.CategoryId,
            Year = dto.Year,
            Month = dto.Month,
            LimitAmount = dto.LimitAmount,
            UserId = _currentUser.UserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _unitOfWork.CategoryBudgets.AddAsync(budget);
        await _unitOfWork.SaveChangesAsync();

        var reloaded = await _unitOfWork.CategoryBudgets.QueryWithDetails().FirstAsync(b => b.Id == budget.Id);
        return await ToDtoAsync(reloaded);
    }

    public async Task<CategoryBudgetResponseDto> UpdateAsync(int id, CategoryBudgetUpdateDto dto)
    {
        var budget = await GetOwnedAsync(id);
        await EnsureBudgetableCategoryAsync(dto.CategoryId);
        await EnsureNoDuplicateAsync(dto.CategoryId, dto.Year, dto.Month, excludingId: id);

        budget.CategoryId = dto.CategoryId;
        budget.Year = dto.Year;
        budget.Month = dto.Month;
        budget.LimitAmount = dto.LimitAmount;
        budget.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.CategoryBudgets.Update(budget);
        await _unitOfWork.SaveChangesAsync();

        var reloaded = await _unitOfWork.CategoryBudgets.QueryWithDetails().FirstAsync(b => b.Id == budget.Id);
        return await ToDtoAsync(reloaded);
    }

    public async Task DeleteAsync(int id)
    {
        var budget = await GetOwnedAsync(id);
        _unitOfWork.CategoryBudgets.Remove(budget);
        await _unitOfWork.SaveChangesAsync();
    }

    // Orçamento só faz sentido em categoria-grupo (raiz) de despesa — bate com os
    // exemplos da spec ("Alimentação: R$800/mês") e reaproveita o mesmo agrupamento
    // que o Dashboard já usa em CategoryBreakdown. Uma subcategoria isolada, ou uma
    // categoria de receita, não pode ter orçamento próprio.
    private async Task EnsureBudgetableCategoryAsync(int categoryId)
    {
        var category = await _unitOfWork.Categories.Query()
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == _currentUser.UserId);

        if (category is null)
            throw new NotFoundException("Categoria não encontrada.");
        if (category.ParentCategoryId is not null)
            throw new BusinessRuleException("Orçamento só pode ser definido numa categoria-grupo (raiz), não numa subcategoria.");
        if (category.Type != CategoryType.Expense)
            throw new BusinessRuleException("Orçamento só pode ser definido em categorias de despesa.");
    }

    private async Task EnsureNoDuplicateAsync(int categoryId, int year, int month, int? excludingId)
    {
        var exists = await _unitOfWork.CategoryBudgets.Query()
            .AnyAsync(b => b.UserId == _currentUser.UserId
                && b.CategoryId == categoryId
                && b.Year == year
                && b.Month == month
                && b.Id != (excludingId ?? 0));

        if (exists)
            throw new ConflictException("Já existe um orçamento para essa categoria nesse mês.");
    }

    private async Task<CategoryBudget> GetOwnedAsync(int id)
    {
        var budget = await _unitOfWork.CategoryBudgets.QueryWithDetails()
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == _currentUser.UserId);

        return budget ?? throw new NotFoundException("Orçamento não encontrado.");
    }

    // "Gasto" = pago + pendente (mesma semântica do Resumo do mês do Dashboard, reflete
    // o planejamento do mês inteiro) de todos os lançamentos cuja categoria é a própria
    // categoria-grupo do orçamento ou uma subcategoria dela.
    private async Task<CategoryBudgetResponseDto> ToDtoAsync(CategoryBudget budget)
    {
        var spent = await _unitOfWork.Transactions.Query()
            .Where(t => t.UserId == _currentUser.UserId
                && t.EntryDate.Year == budget.Year
                && t.EntryDate.Month == budget.Month
                && (t.CategoryId == budget.CategoryId || t.Category!.ParentCategoryId == budget.CategoryId))
            .SumAsync(t => (decimal?)t.Amount) ?? 0m;

        var percentage = budget.LimitAmount > 0 ? spent / budget.LimitAmount : 0m;

        return new CategoryBudgetResponseDto(
            budget.Id,
            budget.CategoryId,
            budget.Category?.Name ?? string.Empty,
            budget.Year,
            budget.Month,
            budget.LimitAmount,
            spent,
            percentage);
    }
}
