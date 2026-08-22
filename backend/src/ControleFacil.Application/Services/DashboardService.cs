using System.Globalization;
using ControleFacil.Application.Dtos;
using ControleFacil.Application.Exceptions;
using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Enums;
using ControleFacil.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ControleFacil.Application.Services;

public class DashboardService : IDashboardService
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly DueAlertOptions _dueAlertOptions;

    public DashboardService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IOptions<DueAlertOptions> dueAlertOptions)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _dueAlertOptions = dueAlertOptions.Value;
    }

    // Resumo mensal considera TODOS os lançamentos do mês (pagos ou não), refletindo o
    // planejamento do mês inteiro — diferente do saldo da conta bancária, que só conta
    // lançamentos pagos (reflete o extrato real).
    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(int year, int month)
    {
        if (month is < 1 or > 12)
            throw new BusinessRuleException("Mês deve estar entre 1 e 12.");

        var rows = await _unitOfWork.Transactions.Query()
            .Where(t => t.UserId == _currentUser.UserId && t.EntryDate.Year == year && t.EntryDate.Month == month)
            .Select(t => new
            {
                t.Amount,
                CategoryType = t.Category!.Type,
                GroupId = t.Category!.ParentCategoryId ?? t.Category!.Id,
                GroupName = t.Category!.ParentCategoryId == null ? t.Category!.Name : t.Category!.ParentCategory!.Name,
            })
            .ToListAsync();

        var totalIncome = rows.Where(r => r.CategoryType == CategoryType.Income).Sum(r => r.Amount);
        var totalExpense = rows.Where(r => r.CategoryType == CategoryType.Expense).Sum(r => r.Amount);

        var breakdown = rows
            .GroupBy(r => new { r.GroupId, r.GroupName, r.CategoryType })
            .Select(g => new CategoryBreakdownDto(g.Key.GroupId, g.Key.GroupName, g.Key.CategoryType, g.Sum(r => r.Amount)))
            .OrderByDescending(b => b.Total)
            .ToList();

        var totalBalance = await GetTotalBalanceAsync();
        var totalInvestments = await GetTotalInvestmentsAsync();

        return new MonthlySummaryDto(year, month, totalIncome, totalExpense, totalIncome - totalExpense, totalBalance, totalInvestments, breakdown);
    }

    // Saldo total = "quanto dinheiro o usuário tem agora": saldo inicial das contas bancárias
    // ATIVAS + todos os lançamentos já pagos dessas contas (receita soma, despesa subtrai),
    // sem recorte de mês — mesma semântica de saldo usada em BankAccountService, só que somada
    // entre todas as contas. Contas inativas (soft-deleted) e lançamentos pendentes ficam de fora.
    private async Task<decimal> GetTotalBalanceAsync()
    {
        var activeAccounts = await _unitOfWork.BankAccounts.Query()
            .Where(b => b.UserId == _currentUser.UserId && b.IsActive)
            .Select(b => new { b.Id, b.InitialBalance })
            .ToListAsync();

        var initialBalanceSum = activeAccounts.Sum(b => b.InitialBalance);
        var activeAccountIds = activeAccounts.Select(b => b.Id).ToList();
        if (activeAccountIds.Count == 0)
            return initialBalanceSum;

        var paidDelta = await _unitOfWork.Transactions.Query()
            .Where(t => activeAccountIds.Contains(t.BankAccountId) && t.UserId == _currentUser.UserId && t.Status == TransactionStatus.Paid)
            .Select(t => t.Category!.Type == CategoryType.Income ? t.Amount : -t.Amount)
            .SumAsync();

        return initialBalanceSum + paidDelta;
    }

    // Patrimônio investido = soma do ÚLTIMO valor lançado (mais recente por Ano/Mês) em
    // cada categoria de investimento ATIVA do usuário — mesma semântica "sem recorte de
    // mês" do saldo total das contas: se o usuário não lançou nada este mês, mantém o
    // último valor conhecido em vez de zerar (evita que o Patrimônio Total oscile só
    // porque o usuário ainda não atualizou a planilha do mês corrente).
    private async Task<decimal> GetTotalInvestmentsAsync()
    {
        var activeCategoryIds = await _unitOfWork.InvestmentCategories.Query()
            .Where(c => c.UserId == _currentUser.UserId && c.IsActive)
            .Select(c => c.Id)
            .ToListAsync();

        if (activeCategoryIds.Count == 0)
            return 0m;

        var entries = await _unitOfWork.InvestmentEntries.Query()
            .Where(e => e.UserId == _currentUser.UserId && activeCategoryIds.Contains(e.InvestmentCategoryId))
            .Select(e => new { e.InvestmentCategoryId, e.Year, e.Month, e.Value })
            .ToListAsync();

        return entries
            .GroupBy(e => e.InvestmentCategoryId)
            .Select(g => g.OrderByDescending(e => e.Year).ThenByDescending(e => e.Month).First().Value)
            .Sum();
    }

    // Mostra "o que está vencendo" independente do DueAlertSentAt do job de e-mail (Sprint
    // F): essa é uma leitura ao vivo pra tela, não controla se o e-mail já saiu ou não —
    // um lançamento continua aparecendo aqui mesmo depois do alerta por e-mail já ter sido
    // enviado, até ser pago.
    public async Task<IReadOnlyList<DueAlertItemDto>> GetDueSoonAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var threshold = today.AddDays(_dueAlertOptions.DaysBefore);

        var items = await _unitOfWork.Transactions.Query()
            .Where(t => t.UserId == _currentUser.UserId && t.Status == TransactionStatus.Pending && t.EntryDate <= threshold)
            .OrderBy(t => t.EntryDate)
            .Select(t => new { t.Id, t.Description, t.Amount, t.EntryDate })
            .ToListAsync();

        return items
            .Select(t => new DueAlertItemDto(t.Id, t.Description, t.Amount, t.EntryDate, t.EntryDate < today))
            .ToList();
    }

    // Retorna null quando o usuário não definiu meta pro mês (Sprint I é opcional) — o
    // frontend mostra um convite pra definir uma em vez de tentar comparar contra zero.
    public async Task<MonthlyGoalComparisonDto?> GetGoalComparisonAsync(int year, int month)
    {
        var goal = await _unitOfWork.MonthlyGoals.Query()
            .FirstOrDefaultAsync(g => g.UserId == _currentUser.UserId && g.Year == year && g.Month == month);
        if (goal is null)
            return null;

        var summary = await GetMonthlySummaryAsync(year, month);
        var profitGoal = goal.IncomeGoal - goal.ExpenseGoal;

        return new MonthlyGoalComparisonDto(
            BuildComparison(summary.TotalIncome, goal.IncomeGoal),
            BuildComparison(summary.TotalExpense, goal.ExpenseGoal),
            BuildComparison(summary.Balance, profitGoal));
    }

    private static GoalComparisonDto BuildComparison(decimal realized, decimal goal)
    {
        var deviationPercent = goal > 0 ? Math.Round((realized - goal) / goal * 100, 1) : (decimal?)null;
        return new GoalComparisonDto(realized, goal, deviationPercent);
    }

    // Mês selecionado + monthsBack anteriores, em ordem cronológica (mais antigo
    // primeiro) — pronto pro eixo X de um gráfico de barras.
    public async Task<IReadOnlyList<HistoricalMonthDto>> GetHistoricalSummaryAsync(int year, int month, int monthsBack = 3)
    {
        if (month is < 1 or > 12)
            throw new BusinessRuleException("Mês deve estar entre 1 e 12.");
        if (monthsBack is < 0 or > 24)
            throw new BusinessRuleException("monthsBack deve estar entre 0 e 24.");

        var anchor = new DateOnly(year, month, 1);
        var result = new List<HistoricalMonthDto>();
        for (var i = monthsBack; i >= 0; i--)
        {
            var cursor = anchor.AddMonths(-i);
            var summary = await GetMonthlySummaryAsync(cursor.Year, cursor.Month);
            result.Add(new HistoricalMonthDto(cursor.Year, cursor.Month, summary.TotalIncome, summary.TotalExpense));
        }

        return result;
    }

    // Regras condicionais simples (sem IA/LLM, por design — mais previsível), combinando
    // a comparação com meta (se houver) e o mês anterior. Constrói em cima dos dois
    // métodos acima, propositalmente implementados primeiro.
    public async Task<IReadOnlyList<AnalysisDto>> GetAutomaticAnalysesAsync(int year, int month)
    {
        var analyses = new List<AnalysisDto>();
        var summary = await GetMonthlySummaryAsync(year, month);
        var goalComparison = await GetGoalComparisonAsync(year, month);
        var historical = await GetHistoricalSummaryAsync(year, month, monthsBack: 1);

        if (summary.Balance < 0)
            analyses.Add(new AnalysisDto("Você gastou mais do que ganhou este mês.", "negative"));
        else
            analyses.Add(new AnalysisDto("Mês positivo: suas receitas superaram as despesas.", "positive"));

        if (goalComparison is not null)
        {
            if (goalComparison.Expense.Goal > 0)
            {
                if (summary.TotalExpense > goalComparison.Expense.Goal)
                {
                    var over = (summary.TotalExpense - goalComparison.Expense.Goal) / goalComparison.Expense.Goal * 100;
                    analyses.Add(new AnalysisDto($"Você ultrapassou a meta de despesa em {FormatPercent(over)}.", "warning"));
                }
                else
                {
                    analyses.Add(new AnalysisDto("Suas despesas ficaram dentro da meta definida.", "positive"));
                }
            }

            if (goalComparison.Income.Goal > 0 && summary.TotalIncome < goalComparison.Income.Goal)
            {
                var under = (goalComparison.Income.Goal - summary.TotalIncome) / goalComparison.Income.Goal * 100;
                analyses.Add(new AnalysisDto($"Sua receita ficou {FormatPercent(under)} abaixo da meta.", "warning"));
            }
        }

        if (historical.Count == 2)
        {
            var previous = historical[0];
            var current = historical[1];
            if (previous.TotalExpense > 0)
            {
                var change = (current.TotalExpense - previous.TotalExpense) / previous.TotalExpense * 100;
                if (change > 10)
                    analyses.Add(new AnalysisDto($"Suas despesas aumentaram {FormatPercent(change)} em relação ao mês anterior.", "warning"));
                else if (change < -10)
                    analyses.Add(new AnalysisDto($"Suas despesas caíram {FormatPercent(Math.Abs(change))} em relação ao mês anterior.", "positive"));
            }
        }

        return analyses;
    }

    private static string FormatPercent(decimal value) => Math.Round(value, 1).ToString("0.0", PtBr) + "%";
}
