 using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Repositories.Interfaces;

public class FacturaWorker : BackgroundService
{
    private readonly IFileQueue queue;
    private readonly IServiceScopeFactory scopeFactory;

    public FacturaWorker(
        IFileQueue queue,
        IServiceScopeFactory scopeFactory)
    {
        this.queue = queue;
        this.scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("WORKER INICIADO");

        int procesadas = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            var file = queue.Dequeue();

            if (file != null)
            {
                Console.WriteLine($"Procesando archivo: {file}");

                using var scope = scopeFactory.CreateScope();

                var parser = scope.ServiceProvider.GetRequiredService<IFacturaParser>();
                var repo = scope.ServiceProvider.GetRequiredService<IClientePDF>();
                var progress = scope.ServiceProvider.GetRequiredService<IProgressService>();

                try
                {
                    using var stream = File.OpenRead(file);

                    var cliente = parser.Parse(stream);

                    await repo.Add(cliente);

                    procesadas++;

                    await progress.Report(
                        procesadas,
                        procesadas + queue.Count(),
                        Path.GetFileName(file),
                        true,
                        null
                    );

                    Console.WriteLine($"Factura guardada: {cliente.NumeroCliente}");
                }
                catch (Exception ex)
                {
                    procesadas++;

                    await progress.Report(
                        procesadas,
                        procesadas + queue.Count(),
                        Path.GetFileName(file),
                        false,
                        ex.Message
                    );

                    Console.WriteLine($"Error procesando {file}: {ex.Message}");
                }
            }

            await Task.Delay(500);
        }
    }
}