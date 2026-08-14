using ControleFacil.Application.Interfaces;
using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Infrastructure.Data.Seed;

public static class DbSeeder
{
    private static readonly Dictionary<string, string[]> IncomeGroups = new()
    {
        ["RENDA FAMILIAR"] = new[] { "Salários", "Bônus", "Comissões", "Participação em lucros", "13º Salário", "Férias", "Outras fontes de renda familiar" },
        ["RECEITAS FINANCEIRAS"] = new[] { "Poupança", "Títulos", "Ações", "Outras receitas com aplicações" },
        ["OUTRAS RECEITAS"] = new[] { "Herança", "Aluguel de imóveis", "Outras receitas gerais" },
    };

    private static readonly Dictionary<string, string[]> ExpenseGroups = new()
    {
        ["DESPESAS COM MORADIA"] = new[] { "Condomínio", "Aluguel", "Conta de luz", "Conta de gás", "Conta de água", "TV por assinatura", "Telefone / Internet", "Empregada", "Diarista", "Babá", "Reforma", "Compra de móveis", "Decoração", "IPTU", "Eletrodomésticos" },
        ["DESPESAS COM ALIMENTAÇÃO"] = new[] { "Compras no supermercado", "Compras na padaria", "Cafés da manhã", "Almoços", "Lanches", "Jantares", "Feira" },
        ["DESPESAS COM SAÚDE"] = new[] { "Plano de saúde", "Consulta com médicos", "Consulta com dentista", "Remédios", "Seguro de vida", "Seguro funeral" },
        ["DESPESAS COM TRANSPORTES"] = new[] { "Combustível", "Seguro veicular", "Estacionamento", "IPVA", "Oficina mecânica", "Táxi", "Uber / Cabify", "Revisão", "Transporte público", "Pedágios", "Multas", "Lavagem" },
        ["DESPESAS COM LAZER"] = new[] { "Viagens", "Alimentação", "Cinema", "Teatro", "Museus", "Esportes", "Passeios", "Livros e revistas", "Academia", "Clube", "Salão de beleza", "Vestuário", "Cursos" },
        ["DESPESAS COM DEPENDENTES"] = new[] { "Escola", "Faculdade", "Livros didáticos", "Serviços jurídicos", "Roupas", "Despesas bancárias", "Mesada", "Cursos extras" },
        ["IMPOSTOS"] = new[] { "Imposto de renda", "INSS" },
    };

    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync())
            return;

        var user = new User
        {
            Name = "Usuário Teste",
            Email = "teste@controlefacil.com",
            CreatedAt = DateTime.UtcNow,
        };
        user.PasswordHash = passwordHasher.HashPassword(user, "Teste@123");
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var categoriesByName = new Dictionary<string, Category>();

        void AddGroups(Dictionary<string, string[]> groups, CategoryType type)
        {
            foreach (var (groupName, subNames) in groups)
            {
                var group = new Category { Name = groupName, Type = type, UserId = user.Id, IsActive = true };
                context.Categories.Add(group);
                categoriesByName[groupName] = group;

                foreach (var subName in subNames)
                {
                    var sub = new Category { Name = subName, Type = type, ParentCategory = group, UserId = user.Id, IsActive = true };
                    context.Categories.Add(sub);
                    categoriesByName[$"{groupName}/{subName}"] = sub;
                }
            }
        }

        AddGroups(IncomeGroups, CategoryType.Income);
        AddGroups(ExpenseGroups, CategoryType.Expense);
        await context.SaveChangesAsync();

        var banco1 = new BankAccount { Name = "Banco 1", InitialBalance = 2000m, UserId = user.Id, IsActive = true };
        var banco2 = new BankAccount { Name = "Banco 2", InitialBalance = 5000m, UserId = user.Id, IsActive = true };
        var caixinha = new BankAccount { Name = "Caixinha", InitialBalance = 200m, UserId = user.Id, IsActive = true };
        context.BankAccounts.AddRange(banco1, banco2, caixinha);
        await context.SaveChangesAsync();

        var salarios = categoriesByName["RENDA FAMILIAR/Salários"];
        var aluguel = categoriesByName["DESPESAS COM MORADIA/Aluguel"];

        context.Transactions.AddRange(
            new Transaction
            {
                EntryDate = new DateOnly(2026, 1, 5),
                CategoryId = salarios.Id,
                Description = "Primeira metade",
                PaymentMethod = PaymentMethod.Cash,
                BankAccountId = banco1.Id,
                Amount = 7500m,
                PaymentDate = new DateOnly(2026, 1, 5),
                Status = TransactionStatus.Paid,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            },
            new Transaction
            {
                EntryDate = new DateOnly(2026, 1, 6),
                CategoryId = aluguel.Id,
                Description = "Pago ao Daniel",
                PaymentMethod = PaymentMethod.Cash,
                BankAccountId = banco1.Id,
                Amount = 3000m,
                PaymentDate = new DateOnly(2026, 1, 6),
                Status = TransactionStatus.Paid,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }
        );
        await context.SaveChangesAsync();
    }
}
