using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Application.Services;

public class CreditCardService : ICreditCardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreditCardService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CreditCardResponseDto>> GetAllAsync(bool includeInactive)
    {
        var query = _unitOfWork.CreditCards.Query().Where(c => c.UserId == _currentUser.UserId);
        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        var cards = await query.OrderBy(c => c.Name).ToListAsync();
        return cards.Select(ToDto).ToList();
    }

    public async Task<CreditCardResponseDto> GetByIdAsync(int id)
    {
        var card = await GetOwnedAsync(id);
        return ToDto(card);
    }

    public async Task<CreditCardResponseDto> CreateAsync(CreditCardCreateDto dto)
    {
        var card = new CreditCard
        {
            Name = dto.Name,
            ClosingDay = dto.ClosingDay,
            DueDay = dto.DueDay,
            UserId = _currentUser.UserId,
            IsActive = true,
        };

        await _unitOfWork.CreditCards.AddAsync(card);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(card);
    }

    public async Task<CreditCardResponseDto> UpdateAsync(int id, CreditCardUpdateDto dto)
    {
        var card = await GetOwnedAsync(id);

        card.Name = dto.Name;
        card.ClosingDay = dto.ClosingDay;
        card.DueDay = dto.DueDay;
        card.IsActive = dto.IsActive;

        _unitOfWork.CreditCards.Update(card);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(card);
    }

    public async Task DeleteAsync(int id)
    {
        var card = await GetOwnedAsync(id);
        card.IsActive = false;
        _unitOfWork.CreditCards.Update(card);
        await _unitOfWork.SaveChangesAsync();
    }

    // Período da fatura (year, month) = janela de compras que fecha nesse mês: do dia
    // seguinte ao fechamento do mês anterior até o dia de fechamento deste mês. O
    // vencimento cai no mesmo mês do fechamento quando DueDay >= ClosingDay (ex: fecha
    // 10, vence 17); senão cai no mês seguinte (ex: fecha 28, vence 5) — sempre depois
    // do fechamento cronologicamente. Math.Min protege contra dia de fechamento/vencimento
    // maior que a quantidade de dias do mês (ex: dia 31 num mês de 30 dias).
    public async Task<CreditCardInvoiceDto> GetInvoiceAsync(int id, int year, int month)
    {
        if (month is < 1 or > 12)
            throw new BusinessRuleException("Mês deve estar entre 1 e 12.");

        var card = await GetOwnedAsync(id);

        var daysInMonth = DateTime.DaysInMonth(year, month);
        var periodEnd = new DateOnly(year, month, Math.Min(card.ClosingDay, daysInMonth));

        var prevMonthAnchor = periodEnd.AddMonths(-1);
        var daysInPrevMonth = DateTime.DaysInMonth(prevMonthAnchor.Year, prevMonthAnchor.Month);
        var periodStart = new DateOnly(prevMonthAnchor.Year, prevMonthAnchor.Month, Math.Min(card.ClosingDay, daysInPrevMonth)).AddDays(1);

        DateOnly dueDate;
        if (card.DueDay >= card.ClosingDay)
        {
            dueDate = new DateOnly(year, month, Math.Min(card.DueDay, daysInMonth));
        }
        else
        {
            var nextMonthAnchor = periodEnd.AddMonths(1);
            var daysInNextMonth = DateTime.DaysInMonth(nextMonthAnchor.Year, nextMonthAnchor.Month);
            dueDate = new DateOnly(nextMonthAnchor.Year, nextMonthAnchor.Month, Math.Min(card.DueDay, daysInNextMonth));
        }

        var transactions = await _unitOfWork.Transactions.QueryWithDetails()
            .Where(t => t.UserId == _currentUser.UserId
                && t.CreditCardId == card.Id
                && t.EntryDate >= periodStart
                && t.EntryDate <= periodEnd)
            .OrderBy(t => t.EntryDate)
            .ToListAsync();

        var total = transactions.Sum(t => t.Amount);

        return new CreditCardInvoiceDto(
            card.Id,
            card.Name,
            year,
            month,
            periodStart,
            periodEnd,
            dueDate,
            total,
            transactions.Select(ToTransactionDto).ToList());
    }

    private async Task<CreditCard> GetOwnedAsync(int id)
    {
        var card = await _unitOfWork.CreditCards.Query()
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == _currentUser.UserId);

        return card ?? throw new NotFoundException("Cartão de crédito não encontrado.");
    }

    private static CreditCardResponseDto ToDto(CreditCard c) => new(c.Id, c.Name, c.ClosingDay, c.DueDay, c.IsActive);

    private static TransactionResponseDto ToTransactionDto(Transaction t) => new(
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
        t.SeriesId,
        t.CreditCardId,
        t.CreditCard?.Name);
}
