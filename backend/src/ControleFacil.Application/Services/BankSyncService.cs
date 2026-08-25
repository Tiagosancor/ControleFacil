using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Application.Services;

public class BankSyncService : IBankSyncService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBrasilApiBankClient _client;

    public BankSyncService(IUnitOfWork unitOfWork, IBrasilApiBankClient client)
    {
        _unitOfWork = unitOfWork;
        _client = client;
    }

    // Upsert por Ispb — nunca deleta bancos que sumirem de uma resposta (evita quebrar
    // contas já ligadas a um Ispb por causa de uma resposta parcial/instável da BrasilAPI).
    public async Task<int> SyncAsync(CancellationToken cancellationToken = default)
    {
        var items = await _client.FetchAllAsync();
        if (items.Count == 0) return 0;

        var existing = await _unitOfWork.Banks.Query().ToDictionaryAsync(b => b.Ispb, cancellationToken);
        var now = DateTime.UtcNow;

        foreach (var item in items)
        {
            if (existing.TryGetValue(item.Ispb, out var bank))
            {
                bank.Code = item.Code;
                bank.Name = item.Name;
                bank.FullName = item.FullName;
                bank.LogoUrl = item.LogoUrl;
                bank.UpdatedAt = now;
                _unitOfWork.Banks.Update(bank);
            }
            else
            {
                await _unitOfWork.Banks.AddAsync(new Bank
                {
                    Ispb = item.Ispb,
                    Code = item.Code,
                    Name = item.Name,
                    FullName = item.FullName,
                    LogoUrl = item.LogoUrl,
                    UpdatedAt = now,
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return items.Count;
    }
}
