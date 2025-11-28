using Microsoft.EntityFrameworkCore;
using Shop.Extensions;
using Shop.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseInMemoryDatabase("shop"));

builder.Services.AddHttpContextAccessor();

builder.Services.AddRequiredServices();

builder.Services.ConfigureJwtOptions(builder.Configuration);

builder.Services.AddJwtBearerAuthentication(builder.Configuration);

builder.Services.AddPolicies();

builder.Services.AddCategoryStatistics(builder.Configuration);

builder.Services.AddValidators();

builder.Services.AddMediatR();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
