namespace ControleFacil.Domain.Enums;

// Fonte única da classificação Categoria -> Tipo de investimento. O grupo nunca é
// persistido junto do tipo (evitaria os dois saírem de sincronia) — é sempre derivado
// daqui, tanto na validação quanto na resposta da API.
public static class InvestmentTypeCatalog
{
    public static readonly IReadOnlyDictionary<InvestmentType, InvestmentGroup> GroupOf = new Dictionary<InvestmentType, InvestmentGroup>
    {
        [InvestmentType.TesouroDireto] = InvestmentGroup.RendaFixa,
        [InvestmentType.CDB] = InvestmentGroup.RendaFixa,
        [InvestmentType.RDB] = InvestmentGroup.RendaFixa,
        [InvestmentType.LCI] = InvestmentGroup.RendaFixa,
        [InvestmentType.LCA] = InvestmentGroup.RendaFixa,
        [InvestmentType.CRI] = InvestmentGroup.RendaFixa,
        [InvestmentType.CRA] = InvestmentGroup.RendaFixa,
        [InvestmentType.Debenture] = InvestmentGroup.RendaFixa,
        [InvestmentType.Poupanca] = InvestmentGroup.RendaFixa,

        [InvestmentType.Acoes] = InvestmentGroup.RendaVariavel,
        [InvestmentType.FII] = InvestmentGroup.RendaVariavel,
        [InvestmentType.ETF] = InvestmentGroup.RendaVariavel,
        [InvestmentType.BDR] = InvestmentGroup.RendaVariavel,
        [InvestmentType.Opcoes] = InvestmentGroup.RendaVariavel,

        [InvestmentType.FundoMultimercado] = InvestmentGroup.FundoInvestimento,
        [InvestmentType.FundoAcoes] = InvestmentGroup.FundoInvestimento,
        [InvestmentType.FundoRendaFixa] = InvestmentGroup.FundoInvestimento,
        [InvestmentType.FundoCambial] = InvestmentGroup.FundoInvestimento,
        [InvestmentType.FundoInfraestrutura] = InvestmentGroup.FundoInvestimento,

        [InvestmentType.PGBL] = InvestmentGroup.PrevidenciaPrivada,
        [InvestmentType.VGBL] = InvestmentGroup.PrevidenciaPrivada,

        [InvestmentType.COE] = InvestmentGroup.Outros,
        [InvestmentType.Criptomoeda] = InvestmentGroup.Outros,
        [InvestmentType.Outros] = InvestmentGroup.Outros,
    };

    // Taxa de juros só faz sentido pra quem tem uma remuneração contratada
    // (Renda Fixa) ou algo análogo (planos de previdência) — Renda Variável, fundos
    // e "Outros" não têm taxa fixa pra registrar.
    public static readonly IReadOnlySet<InvestmentGroup> GroupsWithInterestRate = new HashSet<InvestmentGroup>
    {
        InvestmentGroup.RendaFixa,
        InvestmentGroup.PrevidenciaPrivada,
    };
}
