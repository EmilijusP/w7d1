using AnagramSolver.Contracts.Interfaces;
using AnagramSolver.BusinessLogic.Services;
using AnagramSolver.Contracts.Models;
using AnagramSolver.BusinessLogic.Data;
using AnagramSolver.Api.GraphQL;
using Microsoft.Extensions.Options;
using AnagramSolver.BusinessLogic.Factories;
using AnagramSolver.BusinessLogic.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var settings = builder.Configuration.GetSection("Settings").Get<AppSettings>();
builder.Services.AddSingleton<IAppSettings>(settings);
builder.Services.AddSingleton<IMemoryCache<IEnumerable<string>>, MemoryCache<IEnumerable<string>>>();
builder.Services.AddSingleton<IWordProcessor, WordProcessor>();
builder.Services.AddScoped<IAnagramDictionaryService, AnagramDictionaryService>();
builder.Services.AddScoped<IComplexAnagramAlgorithm, ComplexAnagramAlgorithm>();
builder.Services.AddScoped<IAnagramSolverAlgorithm, SimpleAnagramAlgorithm>();
builder.Services.AddScoped<IAnagramAlgorithmFactory, AnagramAlgorithmFactory>();
builder.Services.AddDbContext<AnagramDbContext>();
builder.Services.AddScoped<IWordRepository, DapperWordRepository>();
builder.Services.AddScoped<ISearchLogRepository, DapperWordRepository>();
builder.Services.AddScoped<IInputValidation, InputValidation>();
builder.Services.AddTransient<IAnagramFilter, OutputLengthFilter>();
builder.Services.AddTransient<IAnagramFilter, CharacterFitFilter>();
builder.Services.AddScoped<IFilterPipeline, FilterPipeline>();
builder.Services.AddScoped<IAnagramSolver, AnagramSolverService>();
builder.Services.AddScoped<IInputNormalization, InputNormalizationService>();
builder.Services.AddScoped<IAnagramFinder, AnagramFinder>();

builder.Services.AddKernel();
builder.Services.AddOpenAIChatCompletion(
    modelId: builder.Configuration["OpenAI:Model"]!,
    apiKey: builder.Configuration["OpenAI:ApiKey"]!);
builder.Services.AddScoped<IAiChatService, AiChatService>();

builder.Services.AddGraphQLServer().AddQueryType<Query>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.MapGraphQL();

app.Run();
