using GD.Models;

namespace GD.Services.Abstractions
{
    internal interface IGDListService
    {
        void ListAllGodotVersions();
        void ListMonoBuildGodotVersions();
    }
}