using ImposterGameV3.Components;
using ImposterGameV3.Hubs;
using ImposterGameV3.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSignalR();
builder.Services.AddSingleton<GameService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapHub<VideoHub>("/videohub");

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
