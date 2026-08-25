namespace ControleFacil.Application.Interfaces;

public interface IBankSyncService
{
    Task<int> SyncAsync(CancellationToken cancellationToken = default);
}
