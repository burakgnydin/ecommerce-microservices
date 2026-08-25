using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductService.Api.ExceptionHandling;
using ProductService.Application.Interfaces;
using ProductService.Application.Strategies;
using ProductService.Application.Validators;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Persistence.Repositories;
using Scalar.AspNetCore;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using AppProductService = ProductService.Application.Services.ProductService;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Add services to the container.
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ProductDb")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductService, AppProductService>();

builder.Services.AddKeyedScoped<IStockValidationStrategy, StandardStockValidationStrategy>(StockValidationStrategyKeys.Standard);
builder.Services.AddKeyedScoped<IStockValidationStrategy, PreOrderStockValidationStrategy>(StockValidationStrategyKeys.PreOrder);

builder.Services.AddValidatorsFromAssemblyContaining<ProductCreateDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseExceptionHandler();
app.UseHttpsRedirection();

// API documentation is only exposed in Development; leaving it public in
// production would expose internal contract details, against our security baseline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

app.Run();
