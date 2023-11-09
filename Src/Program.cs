using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
#region UserStoreDatabaseSetting
builder.Services.Configure<UserStoreDatabaseSetting>(
                            builder.Configuration.GetSection(nameof(UserStoreDatabaseSetting)));
builder.Services.AddSingleton<UserStoreDatabaseSetting>(
                            sp => sp.GetRequiredService<IOptions<UserStoreDatabaseSetting>>().Value);
builder.Services.AddSingleton<IMongoClient>(
                            s => new MongoClient(builder.Configuration.GetValue<string>("UserStoreDatabaseSetting:ConnectionString")));
#endregion


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

app.Run();
