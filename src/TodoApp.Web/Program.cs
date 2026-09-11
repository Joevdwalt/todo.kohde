using TodoApp.Web.Components;
using TodoApp.Web.Components.Todos;
using TodoApp.Core.Todos;
using TodoApp.Discovery.Todos;
using TodoApp.Persistence.Json;
using TodoApp.Persistence.Json.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<TodoStorageOptions>()
    .Bind(builder.Configuration.GetSection(TodoStorageOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.FilePath),
        "TodoStorage:FilePath must not be empty.")
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IGuidGenerator, SystemGuidGenerator>();
builder.Services.AddSingleton<TodoQueryService>();
builder.Services.AddScoped<TodoService>();
builder.Services.AddScoped<TodoPageState>();

// The persistence adapter owns its path resolution and process-wide write lock.
// Keep those details out of this composition root.
builder.Services.AddJsonTodoStore(builder.Configuration, builder.Environment.ContentRootPath);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
