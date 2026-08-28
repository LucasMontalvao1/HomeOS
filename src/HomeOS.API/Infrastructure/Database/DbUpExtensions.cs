using DbUp;
using System.Reflection;

namespace HomeOS.API.Infrastructure.Database;

public static class DbUpExtensions
{
    public static IServiceCollection AddDatabaseMigrations(this IServiceCollection services, string connectionString)
    {
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), filter => filter.Contains(".Scripts."))
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(result.Error);
            Console.ResetColor();
#if DEBUG
            Console.ReadLine();
#endif
            throw new Exception("Falha nas migrations", result.Error);
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Banco de dados atualizado com sucesso!");
        Console.ResetColor();

        return services;
    }
}
