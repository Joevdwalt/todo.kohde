using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Core.Todos;
using TodoApp.Persistence.Json.Configuration;
using TodoApp.Persistence.Json.Files;

namespace TodoApp.Persistence.Json;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJsonTodoStore(
        this IServiceCollection services,
        TodoStorageOptions options,
        string contentRootPath)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        var path = TodoStoragePath.Resolve(options, contentRootPath);
        services.AddSingleton(options);
        services.AddSingleton<IAtomicFileCommitter, PhysicalAtomicFileCommitter>();
        services.AddSingleton<ITodoStore>(provider =>
            new JsonTodoStore(path, provider.GetRequiredService<IAtomicFileCommitter>()));
        return services;
    }

    public static IServiceCollection AddJsonTodoStore(
        this IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var options = new TodoStorageOptions
        {
            FilePath = configuration[$"{TodoStorageOptions.SectionName}:FilePath"] ?? "data/todos.json"
        };
        return services.AddJsonTodoStore(options, contentRootPath);
    }
}
