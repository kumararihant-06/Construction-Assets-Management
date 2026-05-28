using ConstructionAssetAPI.Data;
using ConstructionAssetAPI.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder  =  WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidProgramException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapEquipmentEndpoints();
app.MapJobSiteEndpoints();
//app.MapAssignmentEndpoints();

app.MapGet("/", () => new [] {"hello"});

app.Run();


