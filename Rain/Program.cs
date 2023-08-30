using Rain.Clients;
using Rain.Elastic;
using Rain.Infrastructure;
using Rain.Quartz;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // This reads Serilog configuration from appsettings.json
    .CreateLogger();

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddHttpClient<IJsonPlaceholderApiClient, JsonPlaceholderApiClient>();
builder.Services.AddSingleton<IStockDataSqlRepository, StockDataSqlRepository>();
builder.Services.AddSingleton<IExecutionLogRepository, ExecutionLogRepository>();
builder.Services.AddSingleton<IAlphaVantageSynchronizer, AlphaVantageSynchronizer>();
builder.Services.AddSingleton<IElasticSynchronizer, ElasticSynchronizer>();

builder.Services.AddTransient<IDatabaseConnectionFactory, DatabaseConnectionFactory>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEbookClient();
builder.Services.AddAlphaVantageClient();
builder.Services.AddRainElasticClient();
builder.Services.AddJobScheduling();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
