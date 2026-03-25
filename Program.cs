using MiddlewareEngine.Configuration;
using MiddlewareEngine.Repositories;
using MiddlewareEngine.Services;
using MiddlewareEngine.Executors;
using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;

var builder = WebApplication.CreateBuilder(args);

// Configure BSON serialization to handle object types without type discriminators
BsonSerializer.RegisterSerializer(new ObjectSerializer(type => 
    ObjectSerializer.DefaultAllowedTypes(type) || type.FullName!.StartsWith("System")
));

// Configure MongoDB settings
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// Register MongoDB Database
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = new MongoClient(settings.ConnectionString);
    return client.GetDatabase(settings.DatabaseName);
});

// Add repositories
builder.Services.AddSingleton<IFunctionDefinitionRepository, FunctionDefinitionRepository>();
builder.Services.AddSingleton<TestCaseRepository>();
builder.Services.AddSingleton<ICampaignRepository, CampaignRepository>();

// Add services
builder.Services.AddScoped<IFunctionDefinitionService, FunctionDefinitionService>();
builder.Services.AddScoped<IDataSeeder, DataSeeder>();
builder.Services.AddScoped<IAssemblyManager, AssemblyManager>();
builder.Services.AddScoped<TestCaseService>();
builder.Services.AddScoped<ICampaignService, CampaignService>();

// Add executors
builder.Services.AddScoped<RestApiExecutor>();
builder.Services.AddScoped<ScpiExecutor>();
builder.Services.AddScoped<SdkMethodExecutor>();
builder.Services.AddScoped<SshExecutor>();
builder.Services.AddScoped<FileOperationExecutor>();
builder.Services.AddScoped<IExecutionEngine, ExecutionEngine>();
builder.Services.AddScoped<TestCaseExecutor>();

// Add HttpClient for REST API executor
builder.Services.AddHttpClient();

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Add controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Add Razor Pages
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        // Require authentication for all pages except Login
        options.Conventions.AuthorizePage("/Index");
        options.Conventions.AuthorizePage("/Create");
        options.Conventions.AuthorizePage("/Edit");
        options.Conventions.AuthorizePage("/Execute");
        options.Conventions.AuthorizePage("/TestCases");
        options.Conventions.AuthorizePage("/TestCaseBuilder");
        options.Conventions.AllowAnonymousToPage("/Login");
        options.Conventions.AllowAnonymousToPage("/Logout");
    });

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "MiddlewareEngine API", 
        Version = "v1",
        Description = "Dynamic function execution engine with MongoDB backend"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MiddlewareEngine API v1"));
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
