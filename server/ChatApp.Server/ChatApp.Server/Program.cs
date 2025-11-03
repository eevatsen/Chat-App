using ChatApp.Server.Data;
using ChatApp.Server.Hubs;
using Microsoft.EntityFrameworkCore;
using Azure;
using Azure.AI.TextAnalytics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TextAnalyticsClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var endpoint = new Uri(configuration["TextAnalytics:Endpoint"]);
    var credential = new AzureKeyCredential(configuration["TextAnalytics:Key"]);
    return new TextAnalyticsClient(endpoint, credential);
});
builder.Services.AddSignalR().AddAzureSignalR("Endpoint=https://chatapplication-signalr.service.signalr.net;AccessKey=BVBgjbxkv09s5kvDodNhWpoU1favT8B3x15dCsziROLJsyPgaCWpJQQJ99BJACE1PydXJ3w3AAAAASRSHo9B;Version=1.0;");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
        dbContext.Database.Migrate();
        Console.WriteLine("!!!                    Database migration applied successfully.                            !!!!!!!!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"!!! An error occurred while migrating the database: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseWebSockets();

app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatApp.Server.Hubs.ChatHub>("/chatHub");
app.Run();
