using ForgePLM.Runtime.Data;
using ForgePLM.Runtime.Services;
using Microsoft.EntityFrameworkCore;


namespace ForgePLM.Runtime
{
    public static class RuntimeBootstrap
    {
        public static WebApplication BuildApp(string[] args)
        {
            return BuildApp(args, enableSwagger: true);
        }

        public static WebApplication BuildApp(string[] args, bool enableSwagger)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.UseUrls("http://localhost:5269");

            ConfigureServices(builder);

            var app = builder.Build();

            ConfigurePipeline(app, enableSwagger);

            return app;
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Services.AddControllers()
                .AddApplicationPart(typeof(RuntimeBootstrap).Assembly);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ForgePlmDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("ForgePlmDb")));

            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();
            builder.Services.AddScoped<IEcoService, EcoService>();
            builder.Services.AddScoped<IRevisionService, RevisionService>();
            builder.Services.AddScoped<IPartCategoryService, PartCategoryService>();
            builder.Services.AddScoped<IPartService, PartService>();
        }

        private static void ConfigurePipeline(WebApplication app, bool enableSwagger)
        {
            if (enableSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();
            app.MapGet("/health", () => Results.Ok("ForgePLM Runtime OK"));
        }

    }


}