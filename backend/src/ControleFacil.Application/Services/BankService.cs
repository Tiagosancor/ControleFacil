using System.Globalization;
using System.Text;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Application.Services;

public class BankService : IBankService
{
    private const int SearchLimit = 20;
    private const int DefaultLimit = 50;

    private readonly IUnitOfWork _unitOfWork;

    public BankService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lê sempre da tabela local (sincronizada pelo BankSyncBackgroundService), nunca da
    // BrasilAPI direto — autocomplete não pode depender da disponibilidade/latência de
    // um serviço externo a cada tecla digitada.
    public async Task<IReadOnlyList<BankDto>> SearchAsync(string? search)
    {
        var term = search?.Trim();
        var limit = string.IsNullOrEmpty(term) ? DefaultLimit : SearchLimit;

        var query = _unitOfWork.Banks.Query().OrderBy(b => b.Name);

        List<Bank> banks;
        if (string.IsNullOrEmpty(term))
        {
            banks = await query.Take(limit).ToListAsync();
        }
        else
        {
            // Nomes na BrasilAPI vêm acentuados de forma inconsistente ("ITAÚ" com acento,
            // a maioria sem) — sem remover acento na comparação, buscar "itau" (o jeito que
            // praticamente todo mundo digita) não encontra "ITAÚ UNIBANCO S.A.". A tabela é
            // pequena (algumas centenas de linhas), então filtrar em memória é simples e
            // rápido o suficiente nessa escala.
            var normalizedTerm = RemoveDiacritics(term);
            var all = await query.ToListAsync();
            banks = all
                .Where(b => RemoveDiacritics(b.Name).Contains(normalizedTerm) || RemoveDiacritics(b.FullName).Contains(normalizedTerm))
                .Take(limit)
                .ToList();
        }

        return banks.Select(b => new BankDto(b.Ispb, b.Code, b.Name, b.FullName, b.LogoUrl)).ToList();
    }

    private static string RemoveDiacritics(string value)
    {
        var formD = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in formD)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
}
