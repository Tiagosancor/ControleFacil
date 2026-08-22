using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Application.Services;

public class LongTermGoalService : ILongTermGoalService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public LongTermGoalService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<LongTermGoalResponseDto>> GetAllAsync()
    {
        var goals = await _unitOfWork.LongTermGoals.QueryWithDetails()
            .Where(g => g.UserId == _currentUser.UserId)
            .OrderBy(g => g.TargetYear).ThenBy(g => g.TargetMonth).ThenBy(g => g.Name)
            .ToListAsync();

        var result = new List<LongTermGoalResponseDto>(goals.Count);
        foreach (var goal in goals)
            result.Add(await ToDtoAsync(goal));

        return result;
    }

    public async Task<LongTermGoalResponseDto> GetByIdAsync(int id)
    {
        var goal = await GetOwnedAsync(id);
        return await ToDtoAsync(goal);
    }

    public async Task<LongTermGoalResponseDto> CreateAsync(LongTermGoalCreateDto dto)
    {
        await EnsureTargetInFutureAsync(dto.TargetYear, dto.TargetMonth);
        await EnsureInvestmentCategoryOwnedAsync(dto.InvestmentCategoryId);

        var now = DateTime.UtcNow;
        var goal = new LongTermGoal
        {
            Name = dto.Name,
            TargetAmount = dto.TargetAmount,
            TargetYear = dto.TargetYear,
            TargetMonth = dto.TargetMonth,
            InvestmentCategoryId = dto.InvestmentCategoryId,
            ManualCurrentAmount = dto.ManualCurrentAmount,
            UserId = _currentUser.UserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _unitOfWork.LongTermGoals.AddAsync(goal);
        await _unitOfWork.SaveChangesAsync();

        var reloaded = await _unitOfWork.LongTermGoals.QueryWithDetails().FirstAsync(g => g.Id == goal.Id);
        return await ToDtoAsync(reloaded);
    }

    public async Task<LongTermGoalResponseDto> UpdateAsync(int id, LongTermGoalUpdateDto dto)
    {
        var goal = await GetOwnedAsync(id);
        await EnsureTargetInFutureAsync(dto.TargetYear, dto.TargetMonth);
        await EnsureInvestmentCategoryOwnedAsync(dto.InvestmentCategoryId);

        goal.Name = dto.Name;
        goal.TargetAmount = dto.TargetAmount;
        goal.TargetYear = dto.TargetYear;
        goal.TargetMonth = dto.TargetMonth;
        goal.InvestmentCategoryId = dto.InvestmentCategoryId;
        goal.ManualCurrentAmount = dto.ManualCurrentAmount;
        goal.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.LongTermGoals.Update(goal);
        await _unitOfWork.SaveChangesAsync();

        var reloaded = await _unitOfWork.LongTermGoals.QueryWithDetails().FirstAsync(g => g.Id == goal.Id);
        return await ToDtoAsync(reloaded);
    }

    public async Task DeleteAsync(int id)
    {
        var goal = await GetOwnedAsync(id);
        _unitOfWork.LongTermGoals.Remove(goal);
        await _unitOfWork.SaveChangesAsync();
    }

    private Task EnsureTargetInFutureAsync(int targetYear, int targetMonth)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var target = new DateOnly(targetYear, targetMonth, 1);
        if (target < new DateOnly(today.Year, today.Month, 1))
            throw new BusinessRuleException("A data alvo da meta deve ser no mês atual ou no futuro.");

        return Task.CompletedTask;
    }

    private async Task EnsureInvestmentCategoryOwnedAsync(int? investmentCategoryId)
    {
        if (investmentCategoryId is null)
            return;

        var exists = await _unitOfWork.InvestmentCategories.Query()
            .AnyAsync(c => c.Id == investmentCategoryId.Value && c.UserId == _currentUser.UserId);

        if (!exists)
            throw new NotFoundException("Categoria de investimento não encontrada.");
    }

    private async Task<LongTermGoal> GetOwnedAsync(int id)
    {
        var goal = await _unitOfWork.LongTermGoals.QueryWithDetails()
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == _currentUser.UserId);

        return goal ?? throw new NotFoundException("Meta de longo prazo não encontrada.");
    }

    // "Valor atual" vem do último lançamento da categoria de investimento vinculada (Sprint
    // K, mesma semântica de carry-forward do Patrimônio Total) quando a meta está linkada;
    // caso contrário usa o valor digitado manualmente na própria meta.
    private async Task<decimal> GetCurrentAmountAsync(LongTermGoal goal)
    {
        if (goal.InvestmentCategoryId is null)
            return goal.ManualCurrentAmount;

        var latest = await _unitOfWork.InvestmentEntries.Query()
            .Where(e => e.UserId == goal.UserId && e.InvestmentCategoryId == goal.InvestmentCategoryId.Value)
            .OrderByDescending(e => e.Year).ThenByDescending(e => e.Month)
            .Select(e => (decimal?)e.Value)
            .FirstOrDefaultAsync();

        return latest ?? 0m;
    }

    // Aporte mensal necessário = quanto falta pra bater a meta, dividido pelos meses
    // restantes até a data alvo (mínimo 1, pra não dividir por zero/negativo quando a
    // meta é no mês corrente). Meta já atingida ou superada -> aporte necessário é 0.
    private async Task<LongTermGoalResponseDto> ToDtoAsync(LongTermGoal goal)
    {
        var currentAmount = await GetCurrentAmountAsync(goal);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthsRemaining = (goal.TargetYear - today.Year) * 12 + (goal.TargetMonth - today.Month);
        monthsRemaining = Math.Max(monthsRemaining, 1);

        var remaining = Math.Max(goal.TargetAmount - currentAmount, 0m);
        var monthlyContributionNeeded = Math.Round(remaining / monthsRemaining, 2);
        var progressPercentage = goal.TargetAmount > 0 ? Math.Round(currentAmount / goal.TargetAmount, 4) : 0m;

        return new LongTermGoalResponseDto(
            goal.Id,
            goal.Name,
            goal.TargetAmount,
            goal.TargetYear,
            goal.TargetMonth,
            goal.InvestmentCategoryId,
            goal.InvestmentCategory?.Name,
            currentAmount,
            progressPercentage,
            monthsRemaining,
            monthlyContributionNeeded);
    }
}
