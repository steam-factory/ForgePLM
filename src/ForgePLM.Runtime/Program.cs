using ForgePLM.Runtime.Data;
using ForgePLM.Runtime.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ForgePLM Runtime API",
        Version = "v1"
    });
});

builder.Services.AddDbContext<ForgePlmDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ForgePlmDb")));

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IEcoService, EcoService>();
builder.Services.AddScoped<IRevisionService, RevisionService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();