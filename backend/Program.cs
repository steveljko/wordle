using backend.Hubs;
using backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder
                .WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<LobbyService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();

app.MapControllers();
app.MapHub<GameHub>("/game");

app.Run();
