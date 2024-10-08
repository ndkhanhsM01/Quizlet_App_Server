using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Models;
using Quizlet_App_Server.Utility;
using System.Text;

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

/*FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("../Quizlet_App_Server/Config/quizlet-firebase-adminsdk.json")
});*/

//JWT Authentication
#region JWT authentication
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        ValidAudience = builder.Configuration["Jwt:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//    };
//});
#endregion
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
