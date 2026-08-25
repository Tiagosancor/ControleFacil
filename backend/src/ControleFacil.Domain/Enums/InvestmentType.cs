namespace ControleFacil.Domain.Enums;

public enum InvestmentType
{
    // Renda Fixa
    TesouroDireto,
    CDB,
    RDB,
    LCI,
    LCA,
    CRI,
    CRA,
    Debenture,
    Poupanca,

    // Renda Variável
    Acoes,
    FII,
    ETF,
    BDR,
    Opcoes,

    // Fundo de Investimento
    FundoMultimercado,
    FundoAcoes,
    FundoRendaFixa,
    FundoCambial,
    FundoInfraestrutura,

    // Previdência Privada
    PGBL,
    VGBL,

    // Outros
    COE,
    Criptomoeda,
    Outros,
}
