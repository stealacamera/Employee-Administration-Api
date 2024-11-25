using EmployeeAdministration.Application;
using EmployeeAdministration.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterInfrastructure(builder.Configuration);
builder.Services.RegisterApplication();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(EmployeeAdministration.API.Common.Startup.RegisterSwagger);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
