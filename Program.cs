using ImposterGameFinal.Components;
using ImposterGameFinal.Hubs;
using ImposterGameFinal.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = true;
    });
builder.Services.AddSignalR();
builder.Services.AddSingleton<GameService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.MapHub<VideoHub>("/videohub");

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
