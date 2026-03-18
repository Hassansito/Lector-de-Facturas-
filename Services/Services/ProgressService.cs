using Microsoft.AspNetCore.SignalR;

public interface IProgressService
{
    Task Report(int procesadas, int total, string factura, bool ok, string error);
}

public class ProgressService : IProgressService
{
    private readonly IHubContext<FacturaProgressHub> hub;

    public ProgressService(IHubContext<FacturaProgressHub> hub)
    {
        this.hub = hub;
    }

    public async Task Report(int procesadas, int total, string factura, bool ok, string error)
    {
        await hub.Clients.All.SendAsync(
            "ReceiveProgress",
            procesadas,
            total,
            factura,
            ok,
            error);
    }
}