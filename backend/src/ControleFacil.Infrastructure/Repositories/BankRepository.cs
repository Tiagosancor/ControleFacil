using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using ControleFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Infrastructure.Repositories;

public class BankRepository : Repository<Bank>, IBankRepository
{
    public BankRepository(AppDbContext context) : base(context)
    {
    }

    public Task<Bank?> GetByIspbAsync(string ispb) => Set.FirstOrDefaultAsync(b => b.Ispb == ispb);
}
