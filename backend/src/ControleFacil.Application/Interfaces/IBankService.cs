using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface IBankService
{
    Task<IReadOnlyList<BankDto>> SearchAsync(string? search);
}
