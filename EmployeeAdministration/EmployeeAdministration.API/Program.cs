using EmployeeAdministration.API.Common;
using EmployeeAdministration.Application;
using EmployeeAdministration.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterInfrastructure();
builder.Services.RegisterApplication();
builder.Services.RegisterUtils();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();
