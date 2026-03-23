using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;
using Models.Entities;
using Data; 
using Repositories.Interfaces; 
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class FacturaWorker : BackgroundService
{
    private readonly IFileQueue queue;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<FacturaWorker> logger;

    // Políticas de resiliencia
    private readonly AsyncRetryPolicy retryPolicy;
    private readonly AsyncCircuitBreakerPolicy circuitBreakerPolicy;
    private readonly AsyncPolicyWrap resiliencePipeline;

    public FacturaWorker(
        IFileQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<FacturaWorker> logger)
    {
        this.queue = queue;
        this.scopeFactory = scopeFactory;
        this.logger = logger;

        // Configuración de reintentos (exponential backoff)
        retryPolicy = Policy
            .Handle<Exception>(IsTransientException)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(exception,
                        "Reintento {RetryCount} después de {Delay}ms. Error: {Message}",
                        retryCount, timeSpan.TotalMilliseconds, exception.Message);
                });

        // Circuit breaker: 5 fallos en 30s, abierto 1 minuto
        circuitBreakerPolicy = Policy
            .Handle<Exception>(IsTransientException)
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (exception, breakDuration) =>
                {
                    logger.LogError(exception,
                        "Circuit breaker abierto por {BreakDuration} segundos debido a error: {Message}",
                        breakDuration.TotalSeconds, exception.Message);
                },
                onReset: () => logger.LogInformation("Circuit breaker cerrado nuevamente."),
                onHalfOpen: () => logger.LogInformation("Circuit breaker en half‑open. Probando nueva ejecución."));

        // Combinar: primero reintentos, luego circuit breaker
        resiliencePipeline = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
    }

    private bool IsTransientException(Exception ex)
    {
        // Define qué excepciones consideras transitorias (ej. de base de datos, timeout, etc.)
        // Ajusta según tu entorno y proveedor de BD (PostgreSQL, SQL Server, etc.)
        return ex is DbUpdateException
            || ex is TimeoutException
            || (ex is Npgsql.NpgsqlException && ex.Message.Contains("connection"))
            || (ex.InnerException is Npgsql.NpgsqlException);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("WORKER INICIADO");

        int procesadas = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            var file = queue.Dequeue();

            if (file != null)
            {
                using var scope = scopeFactory.CreateScope();

                // Obtener servicios necesarios dentro del ámbito
                var parser = scope.ServiceProvider.GetRequiredService<IFacturaParser>();
                var progress = scope.ServiceProvider.GetRequiredService<IProgressService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); // Inyección directa del contexto

                bool succeeded = false;
                try
                {
                    // Ejecutar la operación crítica con la pipeline de resiliencia
                    await resiliencePipeline.ExecuteAsync(async token =>
                    {
                        // 1. Parsear el archivo a un objeto ClienteEntity
                        using var stream = File.OpenRead(file);
                        var cliente = parser.Parse(stream); // ParseAsync retorna ClienteEntity

                        // 2. Guardar el cliente en la base de datos
                        await dbContext.Cliente.AddAsync(cliente, token);
                        await dbContext.SaveChangesAsync(token);

                        // 3. Actualizar el cliente con la ruta del archivo y nombre
                        cliente.RutaArchivoFactura = file;                     // Ruta completa
                        cliente.NombreArchivoFactura = Path.GetFileName(file); // Solo nombre
                        await dbContext.SaveChangesAsync(token);

                        succeeded = true;
                    }, stoppingToken);

                    // Si llegamos aquí, todo OK
                    procesadas++;
                    await progress.Report(
                        procesadas,
                        procesadas + queue.Count(),
                        Path.GetFileName(file),
                        true,
                        null
                    );

                    logger.LogInformation("Factura procesada correctamente: {FileName}", Path.GetFileName(file));
                }
                catch (Exception ex) when (!IsTransientException(ex))
                {
                    // Error permanente o después de agotar reintentos
                    succeeded = false;
                    procesadas++;

                    // Mover archivo a carpeta de fallos
                    MoveToFailedFolder(file);

                    // Registrar el error en la base de datos (opcional)
                    try
                    {
                        // Podrías crear una entidad de log de errores, pero no es obligatorio
                        logger.LogError(ex, "Error permanente procesando archivo: {FileName}", Path.GetFileName(file));
                    }
                    catch { }

                    await progress.Report(
                        procesadas,
                        procesadas + queue.Count(),
                        Path.GetFileName(file),
                        false,
                        ex.Message
                    );
                }
                catch (Exception ex)
                {
                    // Fallo transitorio después de reintentos o error no clasificado
                    logger.LogError(ex, "Error no recuperable procesando archivo (transitorio agotado): {FileName}", Path.GetFileName(file));
                    // También se podría mover a failed, o dejarlo en la cola para reintentar después.
                    // En este ejemplo, lo movemos a failed para no bloquear la cola.
                    MoveToFailedFolder(file);
                    procesadas++;
                    await progress.Report(procesadas, procesadas + queue.Count(), Path.GetFileName(file), false, ex.Message);
                }

                // Si el circuit breaker está abierto, pausamos un poco antes de continuar
                if (circuitBreakerPolicy.CircuitState == CircuitState.Open)
                {
                    logger.LogWarning("Circuit breaker abierto. Esperando 5 segundos...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            await Task.Delay(500, stoppingToken);
        }
    }

    private void MoveToFailedFolder(string filePath)
    {
        try
        {
            var failedFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Uploads",
                "Failed");
            Directory.CreateDirectory(failedFolder);

            var fileName = Path.GetFileName(filePath);
            var destPath = Path.Combine(failedFolder, fileName);
            if (File.Exists(filePath))
            {
                File.Move(filePath, destPath, overwrite: true);
                logger.LogInformation("Archivo movido a Failed: {DestPath}", destPath);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al mover archivo a carpeta de fallos: {FilePath}", filePath);
        }
    }
}