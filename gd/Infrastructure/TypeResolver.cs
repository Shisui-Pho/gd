using Spectre.Console.Cli;

namespace GD.Infrastructure;

public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver
{
    public object Resolve(Type type)
    {
        return type == null ? null : provider.GetService(type);
    }
}
