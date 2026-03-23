using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DTO;
using Repositories.Interfaces;
using Services.Interfaces;

namespace BillReader.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly ICLienteService _service;
        private readonly IFileQueue queue;
        private readonly IClientePDF _clienteRepo;

        public ClienteController(ICLienteService service, IFileQueue queue, IClientePDF clienteRepo)
        {
            _service = service;
            _clienteRepo = clienteRepo;
            this.queue = queue;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "Uploads",
                DateTime.Now.ToString("yyyyMMdd"));

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            foreach (var file in files)
            {
                var filePath = Path.Combine(path, file.FileName);

                using var stream = new FileStream(filePath, FileMode.Create);

                await file.CopyToAsync(stream);

                queue.Enqueue(filePath);
            }

            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<ResponseDTO>>GetAll()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { message = "Ocurrió un error interno", detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseDTO>> Update(Guid id, UpdateClienteDTO dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }


        [HttpGet("descargar-factura/{id}")]
        public async Task<IActionResult> DescargarFactura(Guid id)
        {
            var cliente = await _clienteRepo.GetByIdAsync(id);
            if (cliente == null)
                return NotFound("Cliente no encontrado");

            if (string.IsNullOrEmpty(cliente.RutaArchivoFactura))
                return NotFound("No hay factura asociada a este cliente");

            if (!System.IO.File.Exists(cliente.RutaArchivoFactura))
                return NotFound("El archivo físico no existe en el servidor");

            var bytes = await System.IO.File.ReadAllBytesAsync(cliente.RutaArchivoFactura);
            return File(bytes, "application/octet-stream", cliente.NombreArchivoFactura ?? "factura.pdf");
        }

    }
}
