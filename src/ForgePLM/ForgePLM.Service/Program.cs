using ForgePLM.Contracts.Parts;
using ForgePLM.Service.Data;
using Microsoft.Data.SqlClient;
using ForgePLM.Contracts.Customers;
using ForgePLM.Contracts.Projects;
using ForgePLM.Contracts.Eco;

var builder = WebApplication.CreateBuilder(args);

// Force a predictable localhost port
builder.WebHost.UseUrls("http://127.0.0.1:5217");

builder.Services.AddScoped<PartCategoryRepository>();
builder.Services.AddScoped<PartRepository>();
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<EcoRepository>();


var app = builder.Build();

// ---- Health Endpoint ----
app.MapGet("/api/health", (HttpContext http) =>
{
    return Results.Ok(new
    {
        success = true,
        service = "ForgePLM.Service",
        timestampUtc = DateTime.UtcNow,
        traceId = http.TraceIdentifier
    });
});

app.MapGet("/api/part-categories", async (
    PartCategoryRepository repo,
    HttpContext http,
    CancellationToken ct) =>
{
    var categories = await repo.GetActiveAsync(ct);

    return Results.Ok(new
    {
        success = true,
        data = categories,
        traceId = http.TraceIdentifier
    });
});

app.MapPost("/api/parts", async (
    CreatePartRequest request,
    PartRepository repo,
    HttpContext http,
    CancellationToken ct) =>
{
    try
    {
        var result = await repo.CreatePartAndInitialRevisionAsync(request, ct);

        return Results.Ok(new
        {
            success = true,
            data = result,
            traceId = http.TraceIdentifier
        });
    }
    catch (SqlException ex)
    {
        return Results.BadRequest(new
        {
            success = false,
            error = ex.Message,
            traceId = http.TraceIdentifier
        });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new
        {
            success = false,
            error = ex.Message,
            traceId = http.TraceIdentifier
        });
    }
});

app.MapGet("/api/customers", async (
    CustomerRepository repo,
    CancellationToken ct) =>
{
    var customers = await repo.GetAllAsync(ct);
    return Results.Ok(customers);
});

app.MapPost("/api/customers", async (
    CustomerDto customer,
    CustomerRepository repo,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(customer.CustomerCode))
        return Results.BadRequest("Customer Code is required.");

    if (string.IsNullOrWhiteSpace(customer.CustomerName))
        return Results.BadRequest("Customer Name is required.");

    var created = await repo.CreateAsync(customer, ct);
    return Results.Ok(created);
});

app.MapPut("/api/customers/{customerId:int}", async (
    int customerId,
    CustomerDto customer,
    CustomerRepository repo,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(customer.CustomerCode))
        return Results.BadRequest("Customer Code is required.");

    if (string.IsNullOrWhiteSpace(customer.CustomerName))
        return Results.BadRequest("Customer Name is required.");

    try
    {
        var updated = await repo.UpdateAsync(customerId, customer, ct);
        return Results.Ok(updated);
    }
    catch (SqlException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});


app.MapGet("/api/projects/by-customer/{customerId:int}", async (
    int customerId,
    ProjectRepository repo,
    CancellationToken ct) =>
{
    var projects = await repo.GetByCustomerAsync(customerId, ct);
    return Results.Ok(projects);
});

app.MapGet("/api/parts/by-eco/{ecoId:int}", async (
    int ecoId,
    PartRepository repo,
    CancellationToken ct) =>
{
    var parts = await repo.GetEcoContentsAsync(ecoId, ct);
    return Results.Ok(parts);
});

app.MapPost("/api/projects", async (
    CreateProjectRequest request,
    ProjectRepository repo,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.ProjectName))
        return Results.BadRequest("Project Name is required.");

    var created = await repo.CreateAsync(request, ct);
    return Results.Ok(created);
});

app.MapPut("/api/projects/{projectId:int}", async (
    int projectId,
    UpdateProjectRequest request,
    ProjectRepository repo,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.ProjectName))
        return Results.BadRequest("Project Name is required.");

    try
    {
        var updated = await repo.UpdateAsync(projectId, request, ct);
        return Results.Ok(updated);
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(ex.Message);
    }
});

app.MapGet("/api/eco/by-project/{projectId:int}", async (
    int projectId,
    EcoRepository repo,
    CancellationToken ct) =>
{
    var ecos = await repo.GetByProjectAsync(projectId, ct);
    return Results.Ok(ecos);
});

app.MapGet("/api/parts/by-project/{projectId:int}", async (
    int projectId,
    PartRepository repo,
    CancellationToken ct) =>
{
    var parts = await repo.GetProjectPartsCurrentAsync(projectId, ct);
    return Results.Ok(parts);
});

app.MapPost("/api/eco", async (
    CreateEcoRequest request,
    EcoRepository repo,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.EcoTitle))
        return Results.BadRequest("ECO Title is required.");

    var created = await repo.CreateAsync(request, ct);
    return Results.Ok(created);
});

app.MapPut("/api/eco/{ecoId:int}", async (
    int ecoId,
    UpdateEcoRequest request,
    EcoRepository repo,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.EcoTitle))
        return Results.BadRequest("ECO Title is required.");

    try
    {
        var updated = await repo.UpdateAsync(ecoId, request, ct);
        return Results.Ok(updated);
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound(ex.Message);
    }
});

app.MapPost("/api/parts/create-under-eco", async (
    ForgePLM.Contracts.Parts.CreatePartRequest request,
    PartRepository repo,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.ProjectCode))
        return Results.BadRequest("ProjectCode is required.");

    if (string.IsNullOrWhiteSpace(request.EcoNumber))
        return Results.BadRequest("EcoNumber is required.");

    if (string.IsNullOrWhiteSpace(request.CategoryCode))
        return Results.BadRequest("CategoryCode is required.");

    if (string.IsNullOrWhiteSpace(request.Description))
        return Results.BadRequest("Description is required.");

    try
    {
        var created = await repo.CreatePartAndInitialRevisionAsync(request, ct);
        return Results.Ok(created);
    }
    catch (SqlException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});



app.Run();