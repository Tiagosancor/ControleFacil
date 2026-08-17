using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Application.Services;

public class BankAccountService : IBankAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public BankAccountService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PagedResultDto<BankAccountResponseDto>> GetAllAsync(bool includeInactive, int page, int pageSize)
    {
        var query = _unitOfWork.BankAccounts.Query().Where(b => b.UserId == _currentUser.UserId);
        if (!includeInactive)
            query = query.Where(b => b.IsActive);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(b => b.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var deltas = await GetPaidDeltasAsync(items.Select(b => b.Id));
        return new PagedResultDto<BankAccountResponseDto>(
            total, page, pageSize,
            items.Select(b => ToDto(b, deltas.GetValueOrDefault(b.Id, 0m))).ToList());
    }

    public async Task<BankAccountResponseDto> GetByIdAsync(int id)
    {
        var bankAccount = await GetOwnedAsync(id);
        var delta = await GetPaidDeltaAsync(id);
        return ToDto(bankAccount, delta);
    }

    public async Task<BankAccountResponseDto> CreateAsync(BankAccountCreateDto dto)
    {
        var bankAccount = new BankAccount
        {
            Name = dto.Name,
            InitialBalance = dto.InitialBalance,
            UserId = _currentUser.UserId,
            IsActive = true,
        };

        await _unitOfWork.BankAccounts.AddAsync(bankAccount);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(bankAccount, 0m);
    }

    public async Task<BankAccountResponseDto> UpdateAsync(int id, BankAccountUpdateDto dto)
    {
        var bankAccount = await GetOwnedAsync(id);

        bankAccount.Name = dto.Name;
        bankAccount.InitialBalance = dto.InitialBalance;
        bankAccount.IsActive = dto.IsActive;

        _unitOfWork.BankAccounts.Update(bankAccount);
        await _unitOfWork.SaveChangesAsync();

        var delta = await GetPaidDeltaAsync(id);
        return ToDto(bankAccount, delta);
    }

    public async Task DeleteAsync(int id)
    {
        var bankAccount = await GetOwnedAsync(id);
        bankAccount.IsActive = false;
        _unitOfWork.BankAccounts.Update(bankAccount);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<BankAccount> GetOwnedAsync(int id)
    {
        var bankAccount = await _unitOfWork.BankAccounts.Query()
            .FirstOrDefaultAsync(b => b.Id == id && b.UserId == _currentUser.UserId);

        return bankAccount ?? throw new NotFoundException("Conta bancária não encontrada.");
    }

    // Saldo atual = saldo inicial + lançamentos já pagos (receitas somam, despesas subtraem).
    // Lançamentos "Pendente" ainda não afetaram o extrato bancário real, por isso ficam de fora.
    private async Task<decimal> GetPaidDeltaAsync(int bankAccountId)
    {
        return await _unitOfWork.Transactions.Query()
            .Where(t => t.BankAccountId == bankAccountId && t.UserId == _currentUser.UserId && t.Status == TransactionStatus.Paid)
            .Select(t => t.Category!.Type == CategoryType.Income ? t.Amount : -t.Amount)
            .SumAsync();
    }

    private async Task<Dictionary<int, decimal>> GetPaidDeltasAsync(IEnumerable<int> bankAccountIds)
    {
        var idList = bankAccountIds.ToList();
        if (idList.Count == 0) return new Dictionary<int, decimal>();

        var rows = await _unitOfWork.Transactions.Query()
            .Where(t => idList.Contains(t.BankAccountId) && t.UserId == _currentUser.UserId && t.Status == TransactionStatus.Paid)
            .Select(t => new { t.BankAccountId, Signed = t.Category!.Type == CategoryType.Income ? t.Amount : -t.Amount })
            .ToListAsync();

        return rows
            .GroupBy(r => r.BankAccountId)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Signed));
    }

    private static BankAccountResponseDto ToDto(BankAccount b, decimal paidDelta) =>
        new(b.Id, b.Name, b.InitialBalance, b.InitialBalance + paidDelta, b.IsActive);
}
