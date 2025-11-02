// WHY: Registering the IGameRepository interface with its DummyGameRepository implementation.
//      This is the core function of the Dependency Injection (DI) Container.
// TRADE-OFFS (PRO): Decoupling the application. Easily swappable implementations (e.g., swapping
//      DummyGameRepository for EfCoreGameRepository without touching the controller).
// TRADE-OFFS (CON): Increased startup time (very minor). Runtime cost of object graph resolution.
// ALTERNATIVES CONSIDERED: Manually instantiating the repository in the controller. Rejected as it
//      violates DIP and makes the code untestable and tightly coupled.
// LEARNING GOAL: Master service registration and the Scoped lifetime.
using CatalogAPI.Persistence;
using Npgsql;
using CatalogAPI.Interfaces;
using CatalogAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace CatalogAPI
{
    // WHY: We explicitly define the 'Program' class as public. This is required because
    //      the test framework's WebApplicationFactory<Program> needs public access to the
    //      application's entry point, satisfying the C# accessibility rules (CS0060).
    // TRADE-OFF: This removes the implicit convenience of top-level statements but ensures
    //      testability and visibility for the Integration Test framework.
    // LEARNING GOAL: Understand the trade-off between minimalist code and testability.
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // --- CORE ENGINEERING DECISION: DEPENDENCY INJECTION REGISTRATION ---

            // STEP 1: Register the DbContext
            // WHY: We use AddDbContext<T> and configure it to use Npgsql (the PostgreSQL provider).
            //      We pull the connection string from the configuration file we just created.
            //      The DbContext is registered with a SCOPED lifetime automatically by EF Core, 
            //      which enforces our transactional isolation principle.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Note: Ensure the connection string is not null before proceeding in production!
            // We assume it's valid for this development step.

            builder.Services.AddDbContext<GameDbContext>(options => options.UseNpgsql(connectionString));

            // The AddScoped lifetime is chosen here for transactional integrity (Rule 5).
            // STEP 2: Register the IGameRepository - THE GREAT SWAP!
            // WHY: We are swapping the concrete implementation. Because the controller depends only on IGameRepository, 
            //      no other code needs to change. This validates the Dependency Inversion Principle.
            // Trade-Off: We must now ensure the database is running for the application to function.
            // builder.Services.AddScoped<IGameRepository, DummyGameRepository>(); // REMOVE THIS LINE
            builder.Services.AddScoped<IGameRepository, EfCoreGameRepository>();
            // -------------------------------------------------------------------

            // IMPORTANT: AddControllers is required for ASP.NET Core MVC controllers to be recognized!
            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(); // The correct method for Controllers/Swagger is AddSwaggerGen()

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                // These are the correct endpoints for the full Swagger UI/JSON endpoint
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Order of these Use/Map calls matters!
            app.UseHttpsRedirection(); // Enforces secure connections
            app.UseAuthorization(); // Checks identity/permissions

            // IMPORTANT: MapControllers is required to map HTTP requests to the controller classes!
            app.MapControllers();

            app.Run();
        }
    }
}
// FIXES applied:
// 1. Swapped 'builder.Services.AddOpenApi();' for 'builder.Services.AddControllers();' and 'AddSwaggerGen()'.
// 2. Swapped 'app.MapOpenApi();' for 'app.UseSwagger();' and 'app.UseSwaggerUI();'.
// 3. Re-enabled and correctly ordered 'app.UseHttpsRedirection();', 'app.MapControllers();'.