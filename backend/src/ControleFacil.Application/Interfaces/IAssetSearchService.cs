using ControleFacil.Application.Dtos;

namespace ControleFacil.Application.Interfaces;

public interface IAssetSearchService
{
    Task<IReadOnlyList<AssetSearchResultDto>> SearchAsync(string type, string search);
}
