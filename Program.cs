using ConstructionAssetAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder  =  WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidProgramException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

var app = builder.Build();

app.MapGet("/", () => new [] {"hello"});

app.Run();


