using CityInfo.API;
using CityInfo.API.DbContexts;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Reflection;
using System.Text;

//Creating logger(serilog) with more features and configrations instead of the default logger of asp.net 
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/cityinfo.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
var builder = WebApplication.CreateBuilder(args);
//clearing console 
//builder.Logging.ClearProviders(); we used serilog instead
//after an action text will be written but
//when new action created the old action's text will be removed
//due to previous row
//builder.Logging.AddConsole(); we used serilog instead
builder.Host.UseSerilog();
// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setupAction=>
    {
        var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

        setupAction.IncludeXmlComments(xmlCommentsFullPath);

    }
 );
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();
#if DEBUG
builder.Services.AddTransient<IMailService,LocalMailService>();
#else
builder.Services.AddTransient<IMailService,CloudMailService>();
#endif
builder.Services.AddSingleton<CitiesDataStore>();
builder.Services.AddDbContext<CityInfoContext>(
    dbContextOptions => dbContextOptions.UseSqlite(
        builder.Configuration["ConnectionStrings:CityInfoDBConnectionString"])
    );
builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options=>{
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidAudience = builder.Configuration["Authentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(builder.Configuration["Authentication:SecretForKey"])
                )
        };
    });
builder.Services.AddAuthorization(
    options =>
    {
        options.AddPolicy("MustBeFromAntwerp", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim("city", "Antwerp");
        }
        );

    }
);

builder.Services.AddApiVersioning(
    setupAction =>
    {
        setupAction.AssumeDefaultVersionWhenUnspecified = true;
        setupAction.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
        setupAction.ReportApiVersions = true;
    }
    );

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
