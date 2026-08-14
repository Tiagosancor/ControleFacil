using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public TransactionService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PagedResultDto<TransactionResponseDto>> GetAllAsync(TransactionFilterDto filter, int page, int pageSize)
    {
        var query = _unitOfWork.Transactions.QueryWithDetails().Where(t => t.UserId == _currentUser.UserId);

        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
        if (filter.BankAccountId.HasValue)
            query = query.Where(t => t.BankAccountId == filter.BankAccountId.Value);
        if (filter.Status.HasValue)
            query = query.Where(t => t.Status == filter.Status.Value);
        if (filter.Year.HasValue)
            query = query.Where(t => t.EntryDate.Year == filter.Year.Value);
        if (filter.Month.HasValue)
            query = query.Where(t => t.EntryDate.Month == filter.Month.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.EntryDate)
            .ThenByDescending(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<TransactionResponseDto>(total, page, pageSize, items.Select(ToDto).ToList());
    }

    public async Task<TransactionResponseDto> GetByIdAsync(int id)
    {
        var transaction = await GetOwnedAsync(id);
        return ToDto(transaction);
    }

    public async Task<IReadOnlyList<TransactionResponseDto>> CreateAsync(TransactionCreateDto dto)
    {
        await EnsureCategoryOwnedAsync(dto.CategoryId);
        await EnsureBankAccountOwnedAsync(dto.BankAccountId);

        var totalInstallments = dto.TotalInstallments is > 1 ? dto.TotalInstallments.Value : 1;
        var now = DateTime.UtcNow;

        TransactionSeries? series = null;
        if (totalInstallments > 1)
        {
            series = new TransactionSeries
            {
                TotalInstallments = totalInstallments,
                UserId = _currentUser.UserId,
                CreatedAt = now,
            };
            await _unitOfWork.TransactionSeries.AddAsync(series);
            await _unitOfWork.SaveChangesAsync(); // gera o Id da série antes de vincular as parcelas
        }

        var created = new List<Transaction>();
        for (var i = 1; i <= totalInstallments; i++)
        {
            var transaction = new Transaction
            {
                EntryDate = dto.EntryDate.AddMonths(i - 1),
                CategoryId = dto.CategoryId,
                Description = dto.Description,
                PaymentMethod = dto.PaymentMethod,
                BankAccountId = dto.BankAccountId,
                Amount = dto.Amount,
                PaymentDate = dto.PaymentDate?.AddMonths(i - 1),
                Status = dto.Status,
                InstallmentNumber = totalInstallments > 1 ? i : null,
                TotalInstallments = totalInstallments > 1 ? totalInstallments : null,
                SeriesId = series?.Id,
                UserId = _currentUser.UserId,
                CreatedAt = now,
                UpdatedAt = now,
            };
            await _unitOfWork.Transactions.AddAsync(transaction);
            created.Add(transaction);
        }
        await _unitOfWork.SaveChangesAsync();

        var ids = created.Select(t => t.Id).ToList();
        var withDetails = await _unitOfWork.Transactions.QueryWithDetails()
            .Where(t => ids.Contains(t.Id))
            .OrderBy(t => t.InstallmentNumber)
            .ToListAsync();

        return withDetails.Select(ToDto).ToList();
    }

    public async Task<TransactionResponseDto> UpdateAsync(int id, TransactionUpdateDto dto)
    {
        var transaction = await GetOwnedAsync(id);
        await EnsureCategoryOwnedAsync(dto.CategoryId);
        await EnsureBankAccountOwnedAsync(dto.BankAccountId);

        transaction.EntryDate = dto.EntryDate;
        transaction.CategoryId = dto.CategoryId;
        transaction.Description = dto.Description;
        transaction.PaymentMethod = dto.PaymentMethod;
        transaction.BankAccountId = dto.BankAccountId;
        transaction.Amount = dto.Amount;
        transaction.PaymentDate = dto.PaymentDate;
        transaction.Status = dto.Status;
        transaction.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Transactions.Update(transaction);
        await _unitOfWork.SaveChangesAsync();

        var reloaded = await _unitOfWork.Transactions.QueryWithDetails().FirstAsync(t => t.Id == transaction.Id);
        return ToDto(reloaded);
    }

    public async Task DeleteAsync(int id)
    {
        var transaction = await GetOwnedAsync(id);
        _unitOfWork.Transactions.Remove(transaction);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteSeriesAsync(int seriesId)
    {
        var series = await _unitOfWork.TransactionSeries.Query()
            .FirstOrDefaultAsync(s => s.Id == seriesId && s.UserId == _currentUser.UserId);
        if (series is null)
            throw new NotFoundException("Série de lançamentos não encontrada.");

        // as parcelas são removidas em cascata pelo banco (FK Transactions.SeriesId ON DELETE CASCADE)
        _unitOfWork.TransactionSeries.Remove(series);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task EnsureCategoryOwnedAsync(int categoryId)
    {
        var exists = await _unitOfWork.Categories.AnyAsync(c => c.Id == categoryId && c.UserId == _currentUser.UserId);
        if (!exists)
            throw new NotFoundException("Categoria não encontrada.");
    }

    private async Task EnsureBankAccountOwnedAsync(int bankAccountId)
    {
        var exists = await _unitOfWork.BankAccounts.AnyAsync(b => b.Id == bankAccountId && b.UserId == _currentUser.UserId);
        if (!exists)
            throw new NotFoundException("Conta bancária não encontrada.");
    }

    private async Task<Transaction> GetOwnedAsync(int id)
    {
        var transaction = await _unitOfWork.Transactions.QueryWithDetails()
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == _currentUser.UserId);

        return transaction ?? throw new NotFoundException("Lançamento não encontrado.");
    }

    private static TransactionResponseDto ToDto(Transaction t) => new(
        t.Id,
        t.EntryDate,
        t.CategoryId,
        t.Category?.Name ?? string.Empty,
        t.Description,
        t.PaymentMethod,
        t.BankAccountId,
        t.BankAccount?.Name ?? string.Empty,
        t.Amount,
        t.PaymentDate,
        t.Status,
        t.InstallmentNumber,
        t.TotalInstallments,
        t.SeriesId);
}
