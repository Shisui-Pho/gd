using GD.Models;
using GD.Services.Abstractions;
namespace GD.Infrastructure.Abstractions;
internal interface IGDConfigurationsFactory
{
    IGDConfigurations ConfigurationsInstance { get; }   
    bool GDConfigurationsLoaded();
    IGDConfigurations CreateConfigurations();
}