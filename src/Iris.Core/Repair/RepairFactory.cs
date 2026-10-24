namespace Iris.Core;

public sealed class RepairFactory : IRepairFactory
{
    private readonly Dictionary<string, Func<string?, IVideoRepair>> _repairs;

    public RepairFactory(IVideoCheck check, IVideoTools tools, IDisk disk, string workFolder)
    {
        _repairs = new(StringComparer.OrdinalIgnoreCase)
        {
            [CutRepair.RepairName] = _ => new CutRepair(check, tools, disk, workFolder),
            [FreezeRepair.RepairName] = encoder => new FreezeRepair(tools, disk, workFolder, encoder ?? FreezeRepair.Processor),
        };
    }

    public IVideoRepair Create(string name, string? videoEncoder = null) =>
        _repairs.TryGetValue(name, out var create)
            ? create(videoEncoder)
            : throw new ArgumentException($"Unknown repair '{name}': use {string.Join(" or ", _repairs.Keys)}.");
}
