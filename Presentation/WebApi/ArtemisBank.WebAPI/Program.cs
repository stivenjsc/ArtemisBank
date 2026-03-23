using ArtemisBank.Core.Application.Mappings;
using ArtemisBank.Core.Application.IoC;
using ArtemisBank.Infrastructure.Identity;
using ArtemisBank.Infrastructure.Persistence.IoC;
using ArtemisBank.Infrastructure.Shared.IoC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration);

builder.Services.AddApplicationLayer();
builder.Services.AddAutoMapper(cfg => { }, typeof(AutoMapperProfile));
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseHealthChecks("/health");

app.MapControllers();

await app.RunAsync();