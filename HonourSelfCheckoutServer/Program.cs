using HonourSelfCheckoutServer.Data;
using Microsoft.EntityFrameworkCore;
using Stripe;


var builder = WebApplication.CreateBuilder(args);


StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
Console.WriteLine($"Stripe key Length: {StripeConfiguration.ApiKey.Length}");
// Register the database context
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {

        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");

        // Use string.Empty to make Swagger UI available at the root (e.g., http://localhost:5147/)
        options.RoutePrefix = string.Empty;

        options.DocumentTitle = "SelfCheckout";

        options.EnableDeepLinking();


        options.DisplayOperationId();
    });

}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/health", () => Results.Ok(new { status = "Server is running" }));

app.Run();
