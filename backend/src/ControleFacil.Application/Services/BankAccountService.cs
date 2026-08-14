using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
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

        return new PagedResultDto<BankAccountResponseDto>(total, page, pageSize, items.Select(ToDto).ToList());
    }

    public async Task<BankAccountResponseDto> GetByIdAsync(int id)
    {
        var bankAccount = await GetOwnedAsync(id);
        return ToDto(bankAccount);
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

        return ToDto(bankAccount);
    }

    public async Task<BankAccountResponseDto> UpdateAsync(int id, BankAccountUpdateDto dto)
    {
        var bankAccount = await GetOwnedAsync(id);

        bankAccount.Name = dto.Name;
        bankAccount.InitialBalance = dto.InitialBalance;
        bankAccount.IsActive = dto.IsActive;

        _unitOfWork.BankAccounts.Update(bankAccount);
        await _unitOfWork.SaveChangesAsync();

        return ToDto(bankAccount);
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

    private static BankAccountResponseDto ToDto(BankAccount b) => new(b.Id, b.Name, b.InitialBalance, b.IsActive);
}
