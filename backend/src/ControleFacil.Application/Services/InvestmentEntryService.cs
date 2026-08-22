using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Application.Services;

public class InvestmentEntryService : IInvestmentEntryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public InvestmentEntryService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<InvestmentEntryResponseDto>> GetAllAsync(int? year, int? month)
    {
        var query = _unitOfWork.InvestmentEntries.QueryWithDetails().Where(e => e.UserId == _currentUser.UserId);
        if (year.HasValue)
            query = query.Where(e => e.Year == year.Value);
        if (month.HasValue)
            query = query.Where(e => e.Month == month.Value);

        var entries = await query
            .OrderBy(e => e.Year).ThenBy(e => e.Month).ThenBy(e => e.InvestmentCategory!.Name)
            .ToListAsync();

        return entries.Select(ToDto).ToList();
    }

    public async Task<InvestmentEntryResponseDto> GetByIdAsync(int id)
    {
        var entry = await GetOwnedAsync(id);
        return ToDto(entry);
    }

    public async Task<InvestmentEntryResponseDto> CreateAsync(InvestmentEntryCreateDto dto)
    {
        await EnsureCategoryOwnedAsync(dto.InvestmentCategoryId);
        await EnsureNoDuplicateAsync(dto.InvestmentCategoryId, dto.Year, dto.Month, excludingId: null);

        var now = DateTime.UtcNow;
        var entry = new InvestmentEntry
        {
            InvestmentCategoryId = dto.InvestmentCategoryId,
            Year = dto.Year,
            Month = dto.Month,
            Value = dto.Value,
            UserId = _currentUser.UserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _unitOfWork.InvestmentEntries.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync();

        var reloaded = await _unitOfWork.InvestmentEntries.QueryWithDetails().FirstAsync(e => e.Id == entry.Id);
        return ToDto(reloaded);
    }

    public async Task<InvestmentEntryResponseDto> UpdateAsync(int id, InvestmentEntryUpdateDto dto)
    {
        var entry = await GetOwnedAsync(id);
        await EnsureCategoryOwnedAsync(dto.InvestmentCategoryId);
        await EnsureNoDuplicateAsync(dto.InvestmentCategoryId, dto.Year, dto.Month, excludingId: id);

        entry.InvestmentCategoryId = dto.InvestmentCategoryId;
        entry.Year = dto.Year;
        entry.Month = dto.Month;
        entry.Value = dto.Value;
        entry.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.InvestmentEntries.Update(entry);
        await _unitOfWork.SaveChangesAsync();

        var reloaded = await _unitOfWork.InvestmentEntries.QueryWithDetails().FirstAsync(e => e.Id == entry.Id);
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int id)
    {
        var entry = await GetOwnedAsync(id);
        _unitOfWork.InvestmentEntries.Remove(entry);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task EnsureCategoryOwnedAsync(int investmentCategoryId)
    {
        var exists = await _unitOfWork.InvestmentCategories.Query()
            .AnyAsync(c => c.Id == investmentCategoryId && c.UserId == _currentUser.UserId);

        if (!exists)
            throw new NotFoundException("Categoria de investimento não encontrada.");
    }

    private async Task EnsureNoDuplicateAsync(int investmentCategoryId, int year, int month, int? excludingId)
    {
        var exists = await _unitOfWork.InvestmentEntries.Query()
            .AnyAsync(e => e.UserId == _currentUser.UserId
                && e.InvestmentCategoryId == investmentCategoryId
                && e.Year == year
                && e.Month == month
                && e.Id != (excludingId ?? 0));

        if (exists)
            throw new ConflictException("Já existe um valor lançado para essa categoria nesse mês.");
    }

    private async Task<InvestmentEntry> GetOwnedAsync(int id)
    {
        var entry = await _unitOfWork.InvestmentEntries.QueryWithDetails()
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == _currentUser.UserId);

        return entry ?? throw new NotFoundException("Lançamento de investimento não encontrado.");
    }

    private static InvestmentEntryResponseDto ToDto(InvestmentEntry e) => new(
        e.Id,
        e.InvestmentCategoryId,
        e.InvestmentCategory?.Name ?? string.Empty,
        e.Year,
        e.Month,
        e.Value);
}
