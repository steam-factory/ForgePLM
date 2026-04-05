using ForgePLM.Contracts.Parts;
using ForgePLM.Service.Data;
using Microsoft.Data.SqlClient;
using ForgePLM.Contracts.Customers;

var builder = WebApplication.CreateBuilder(args);

// Force a predictable localhost port
builder.WebHost.UseUrls("http://127.0.0.1:5217");

builder.Services.AddScoped<PartCategoryRepository>();
builder.Services.AddScoped<PartRepository>();
builder.Services.AddScoped<CustomerRepository>();

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
        var result = await repo.CreatePartAsync(request, ct);

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

app.Run();