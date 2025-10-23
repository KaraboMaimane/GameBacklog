// WHY: Registering the IGameRepository interface with its DummyGameRepository implementation.
//      This is the core function of the Dependency Injection (DI) Container.
// TRADE-OFFS (PRO): Decoupling the application. Easily swappable implementations (e.g., swapping
//      DummyGameRepository for EfCoreGameRepository without touching the controller).
// TRADE-OFFS (CON): Increased startup time (very minor). Runtime cost of object graph resolution.
// ALTERNATIVES CONSIDERED: Manually instantiating the repository in the controller. Rejected as it
//      violates DIP and makes the code untestable and tightly coupled.
// LEARNING GOAL: Master service registration and the Scoped lifetime.
using CatalogAPI.Interfaces;
using CatalogAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// --- CORE ENGINEERING DECISION: DEPENDENCY INJECTION REGISTRATION ---
// The AddScoped lifetime is chosen here for transactional integrity (Rule 5).
builder.Services.AddScoped<IGameRepository, DummyGameRepository>();
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

// FIXES applied:
// 1. Swapped 'builder.Services.AddOpenApi();' for 'builder.Services.AddControllers();' and 'AddSwaggerGen()'.
// 2. Swapped 'app.MapOpenApi();' for 'app.UseSwagger();' and 'app.UseSwaggerUI();'.
// 3. Re-enabled and correctly ordered 'app.UseHttpsRedirection();', 'app.MapControllers();'.