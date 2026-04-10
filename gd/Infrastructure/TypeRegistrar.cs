using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace GD.Infrastructure;

internal sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceCollection _serviceProvider;
    public IServiceProvider Provider { get; private set; } = null!;
    public TypeRegistrar(IServiceCollection services)
    {
        _serviceProvider = services;
    }
    public void Register(Type service, Type implementation)
    {
        _serviceProvider.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation)
    {
        _serviceProvider.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        _serviceProvider.AddSingleton(service, _ => factory());
    }

    public ITypeResolver Build()
    {
        Provider ??= _serviceProvider.BuildServiceProvider();
        return new TypeResolver(Provider);
    }
}