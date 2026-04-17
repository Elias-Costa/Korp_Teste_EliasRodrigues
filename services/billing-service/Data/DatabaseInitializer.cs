using Microsoft.EntityFrameworkCore;

namespace BillingService.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("BillingDatabaseInitializer");
        var dbContext = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

        const int maxAttempts = 10;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await dbContext.Database.EnsureCreatedAsync();
                logger.LogInformation("Billing database is ready.");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    ex,
                    "Billing database is not ready yet. Waiting before retry {Attempt}/{MaxAttempts}.",
                    attempt,
                    maxAttempts);

                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }

        await dbContext.Database.EnsureCreatedAsync();
    }
}
