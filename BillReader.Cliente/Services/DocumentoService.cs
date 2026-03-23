namespace BillReader.Cliente.Services
{
    public class DocumentoService
    {
        private readonly HttpClient _httpClientNoAuth;

        public DocumentoService(IHttpClientFactory clientFactory)
        {
            _httpClientNoAuth = clientFactory.CreateClient("AuthorizedClient");
        }

        public async Task<Stream> DescargarFacturaAsync(Guid clienteId)
        {
            var response = await _httpClientNoAuth.GetAsync($"api/cliente/descargar-factura/{clienteId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
    }
}
