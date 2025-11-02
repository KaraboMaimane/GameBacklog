// WHY: This custom factory is the core of integration testing. It allows us to boot
//      our entire API in-memory (using WebApplicationFactory) but override services
//      in the DI container *before* the app starts.
// TRADE-OFFS (PRO): Full end-to-end testing of the HTTP pipeline, DI, and persistence.
// TRADE-OFFS (CON): Slower than unit tests, more complex setup.
// ALTERNATIVES CONSIDERED: Testing against the real PostgreSQL Docker container. Rejected
//      because it's slower, harder to clean up (data pollution), and violates test isolation.
// LEARNING GOAL: Master overriding DI services in a test environment.
using System.Linq;
using CatalogAPI.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CatalogAPI.Tests.Integration
{
    /// <summary>
    /// Custom WebApplicationFactory to configure the API for integration testing.
    /// This factory will replace the production database (PostgreSQL) with an
    /// in-memory database to ensure tests are fast, isolated, and repeatable.
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program> // <Program> points to your API's entry point
    {
        // WHY: We override the standard setup to replace the PostgreSQL provider with a fast, in-memory database.
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // The 'services' variable is only available inside this lambda function.
            builder.ConfigureServices(services =>
            {
                // 1. CLEANUP: Find and remove the existing DbContext configurations (PostgreSQL setup).
                // WHY: We remove all references to the real database to ensure isolation.
                var dbContextOptionsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<GameDbContext>));

                if (dbContextOptionsDescriptor != null)
                {
                    services.Remove(dbContextOptionsDescriptor);
                }

                // CRITICAL FIX: Remove the services related to the DbContext itself, 
                // ensuring no lingering connection to Npgsql remains.
                var dbContextServices = services
                    .Where(s => s.ServiceType.FullName?.Contains("GameDbContext") == true ||
                                s.ServiceType.FullName?.Contains("Npgsql") == true)
                    .ToList();

                foreach (var service in dbContextServices)
                {
                    services.Remove(service);
                }


                // 2. RE-REGISTER: Add a *new* DbContext configured for the in-memory database
                // WHY: This is the replacement service. The DbContext is now isolated to this test run.
                services.AddDbContext<GameDbContext>(options =>
                {
                    // Use a specific, consistent name for the in-memory database during the test run.
                    // Using a GUID guarantees a fresh, clean database for every test class run.
                    options.UseInMemoryDatabase($"InMemoryDbForTesting-{System.Guid.NewGuid()}");
                });

                // 3. MANUAL SEEDING: Ensure the in-memory schema is created.
                // WHY: In-Memory DB does not automatically create tables; we must force it here.
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<GameDbContext>();

                    // Ensure the database is created (the in-memory equivalent of a migration)
                    db.Database.EnsureCreated();

                    // Note: We don't need to seed data here, as the test immediately POSTs data.
                }
            });

            // Set the environment to "Development" for consistency
            builder.UseEnvironment("Development");
        }
    }
}

// NOTE: You will need to add a <Using> statement in your CatalogAPI.csproj
// to make the internal 'Program' class visible to the test project.