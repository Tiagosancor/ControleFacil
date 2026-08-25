using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface IBrasilApiBankClient
{
    Task<IReadOnlyList<BankSyncItemDto>> FetchAllAsync();
}
