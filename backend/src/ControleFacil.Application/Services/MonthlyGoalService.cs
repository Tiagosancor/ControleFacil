using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Application.Services;

public class MonthlyGoalService : IMonthlyGoalService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public MonthlyGoalService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<MonthlyGoalResponseDto>> GetAllAsync(int? year, int? month)
    {
        var query = _unitOfWork.MonthlyGoals.Query().Where(g => g.UserId == _currentUser.UserId);
        if (year.HasValue)
            query = query.Where(g => g.Year == year.Value);
        if (month.HasValue)
            query = query.Where(g => g.Month == month.Value);

        var goals = await query.OrderBy(g => g.Year).ThenBy(g => g.Month).ToListAsync();
        return goals.Select(ToDto).ToList();
    }

    public async Task<MonthlyGoalResponseDto> GetByIdAsync(int id)
    {
        var goal = await GetOwnedAsync(id);
        return ToDto(goal);
    }

    public async Task<MonthlyGoalResponseDto> CreateAsync(MonthlyGoalCreateDto dto)
    {
        await EnsureNoDuplicateAsync(dto.Year, dto.Month, excludingId: null);

        var now = DateTime.UtcNow;
        var goal = new MonthlyGoal
        {
            Year = dto.Year,
            Month = dto.Month,
            IncomeGoal = dto.IncomeGoal,
            ExpenseGoal = dto.ExpenseGoal,
            UserId = _currentUser.UserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _unitOfWork.MonthlyGoals.AddAsync(goal);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(goal);
    }

    public async Task<MonthlyGoalResponseDto> UpdateAsync(int id, MonthlyGoalUpdateDto dto)
    {
        var goal = await GetOwnedAsync(id);
        await EnsureNoDuplicateAsync(dto.Year, dto.Month, excludingId: id);

        goal.Year = dto.Year;
        goal.Month = dto.Month;
        goal.IncomeGoal = dto.IncomeGoal;
        goal.ExpenseGoal = dto.ExpenseGoal;
        goal.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.MonthlyGoals.Update(goal);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(goal);
    }

    public async Task DeleteAsync(int id)
    {
        var goal = await GetOwnedAsync(id);
        _unitOfWork.MonthlyGoals.Remove(goal);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task EnsureNoDuplicateAsync(int year, int month, int? excludingId)
    {
        var exists = await _unitOfWork.MonthlyGoals.Query()
            .AnyAsync(g => g.UserId == _currentUser.UserId
                && g.Year == year
                && g.Month == month
                && g.Id != (excludingId ?? 0));

        if (exists)
            throw new ConflictException("Já existe uma meta definida para esse mês.");
    }

    private async Task<MonthlyGoal> GetOwnedAsync(int id)
    {
        var goal = await _unitOfWork.MonthlyGoals.Query()
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == _currentUser.UserId);

        return goal ?? throw new NotFoundException("Meta mensal não encontrada.");
    }

    private static MonthlyGoalResponseDto ToDto(MonthlyGoal g) => new(g.Id, g.Year, g.Month, g.IncomeGoal, g.ExpenseGoal);
}
