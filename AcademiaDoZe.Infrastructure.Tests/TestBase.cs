using AcademiaDoZe.Infrastructure.Data;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = true)]

namespace AcademiaDoZe.Infrastructure.Tests;

public abstract class TestBase
{
    private const DatabaseType SelectedDatabaseType = DatabaseType.SqlServer;

    protected string ConnectionString { get; }
    protected DatabaseType DatabaseType { get; }

    protected TestBase()
    {
        DatabaseType = SelectedDatabaseType;
        ConnectionString = Environment.GetEnvironmentVariable("ACADEMIA_DO_ZE_CONNECTION")
            ?? "Server=localhost,1433;Initial Catalog=db_academia_do_ze;User Id=sa;Password=abcBolinhas12345;TrustServerCertificate=True;Encrypt=False;";
    }

    protected static string GerarCep() => (80000000 + Random.Shared.Next(0, 9999999)).ToString("D8");

    protected static string GerarCpf()
    {
        int[] digits = new int[9];
        do
        {
            for (var i = 0; i < 9; i++) digits[i] = Random.Shared.Next(0, 10);
        } while (digits.All(d => d == digits[0]));

        int sum1 = 0;
        for (var i = 0; i < 9; i++) sum1 += digits[i] * (10 - i);
        int d1 = sum1 % 11;
        d1 = d1 < 2 ? 0 : 11 - d1;

        int sum2 = 0;
        for (var i = 0; i < 9; i++) sum2 += digits[i] * (11 - i);
        sum2 += d1 * 2;
        int d2 = sum2 % 11;
        d2 = d2 < 2 ? 0 : 11 - d2;

        return string.Concat(digits.Concat(new[] { d1, d2 }));
    }

    protected static string GerarEmail() => $"user_{Guid.NewGuid():N}@test.com";
    protected static string GerarTelefone() => $"49{Random.Shared.Next(100000000, 999999999)}";
}
