

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// catch logger
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// add logs
logger.LogInformation("Application is starting up.");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    logger.LogInformation("Swagger has been enabled for the development environment.");
}

app.UseHttpsRedirection();
logger.LogInformation("HTTPS redirection has been configured.");

app.UseAuthorization();
logger.LogInformation("Authorization middleware has been added.");

app.MapControllers();
logger.LogInformation("Controllers have been mapped.");

// // test
// app.MapGet("/test", () => {
//     logger.LogInformation("Test endpoint was called.");
//     return "Hello from the test endpoint!";
// });

logger.LogInformation("Application is about to start running.");

app.Run();

logger.LogInformation("Application has stopped.");