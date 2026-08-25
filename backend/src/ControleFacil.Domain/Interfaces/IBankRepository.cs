using ControleFacil.Domain.Entities;

namespace ControleFacil.Domain.Interfaces;

public interface IBankRepository : IRepository<Bank>
{
    Task<Bank?> GetByIspbAsync(string ispb);
}
