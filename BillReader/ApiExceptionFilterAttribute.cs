using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        var exception = context.Exception;

        switch (exception)
        {
            case KeyNotFoundException _:
                context.Result = new NotFoundObjectResult(new
                {
                    message = exception.Message
                });
                break;

            case DbUpdateConcurrencyException _:
                context.Result = new ConflictObjectResult(new
                {
                    message = "Conflicto de concurrencia. Los datos fueron modificados por otro usuario."
                });
                break;

            case DbUpdateException _:
                context.Result = new BadRequestObjectResult(new
                {
                    message = "Error al guardar los datos. Verifique la integridad de la información."
                });
                break;

            default:
                // En producción, no devuelvas detalles internos
                context.Result = new ObjectResult(new
                {
                    message = "Ocurrió un error interno en el servidor."
                })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                break;
        }

        context.ExceptionHandled = true;
    }
}