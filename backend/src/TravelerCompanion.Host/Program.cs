
using Hangfire;
using Hangfire.MemoryStorage;
using Serilog;
using TravelerCompanion;
using TravelerCompanion.SouthwestJobs;
using TravelerCompanion.SouthwestJobs.Jobs;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Host.UseSerilog((hostingContext, _, loggerConfiguration) => 
{
    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();
builder.Services.AddSwaggerGen();
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMemoryStorage()
    .UseSerilogLogProvider());
builder.Services.AddHangfireServer();

builder.Services.AddSingleton<IAirportDataProvider, CsvAirportDataProvider>();
await builder.Services
    .AddSouthwestJobs()
    .AddSouthwestInMemoryRepositories()
    .AddSouthwestWebDriverAsync(true);

var app = builder.Build();
GlobalConfiguration.Configuration.UseActivator(new ServiceProviderActivator(app.Services));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseHangfireDashboard();
app.UseSerilogRequestLogging();

app.UseCors(cors => cors
    .SetIsOriginAllowed(_ => true)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials());
app.MapControllers();

 app.Run();

public class ServiceProviderActivator(IServiceProvider serviceProvider) : JobActivator
{
    public override object ActivateJob(Type jobType) => 
        serviceProvider.GetRequiredService(jobType);
}
