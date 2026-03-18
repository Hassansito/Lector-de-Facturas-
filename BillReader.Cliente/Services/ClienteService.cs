using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using BillReader.Cliente.Models;
using Microsoft.Extensions.Logging;

namespace BillReader.Cliente.Services;

public class ClienteService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClienteService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ClienteService(HttpClient httpClient, ILogger<ClienteService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ClienteModel>> GetAllClientesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/Cliente");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Respuesta API: {content}");

            return JsonSerializer.Deserialize<List<ClienteModel>>(content, _jsonOptions)
                   ?? new List<ClienteModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Clientes");
            throw;
        }
    }

    public async Task<(bool IsSuccess, string Message, ClienteModel Data)> UpdateClienteAsync(ClienteModel cliente)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Cliente/{cliente.Id}", cliente);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<ClienteModel>();
                return (true, "Cliente actualizado correctamente", data);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, $"Error {response.StatusCode}: {error}", null);
            }
        }
        catch (Exception ex)
        {
            return (false, $"Excepción: {ex.Message}", null);
        }
    }

    public async Task<bool> DeleteClienteAsync(Guid Id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/Cliente/{Id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar comemierda con ID: {Id}", Id);
            throw;
        }
    }
}