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
builder.Services.AddSignalR();//.AddAzureSignalR();
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
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatApp.Server.Hubs.ChatHub>("/chatHub");
app.Run();
