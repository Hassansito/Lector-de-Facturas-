using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Models.Entities;

public interface IFacturaParser
{
    ClienteEntity Parse(Stream pdfStream);
}
public class EtecsaFacturaParser : IFacturaParser
{
    // Lista completa de provincias cubanas (sin acentos para comparación)
    private static readonly string[] Provincias = new[]
    {
        "PINAR DEL RIO", "ARTEMISA", "LA HABANA", "MAYABEQUE", "MATANZAS", "CIENFUEGOS",
        "VILLA CLARA", "SANCTI SPIRITUS", "CIEGO DE AVILA", "CAMAGÜEY", "LAS TUNAS",
        "HOLGUIN", "GRANMA", "SANTIAGO DE CUBA", "GUANTANAMO", "ISLA DE LA JUVENTUD"
    };

    // Palabras clave que indican que una línea no es parte del nombre del cliente
    private static readonly string[] KeywordsExcluir = new[]
    {
        "NÚMERO", "NUMERO", "DIRECCIÓN", "DIRECCION", "PERIODO", "OFICINA", "FACTURA", "FITECSA",
        "CUOTA", "CONSUMO", "COMISIÓN", "COMISION", "IMPUESTO", "FACTURADO", "CRÉDITO", "CREDITO",
        "PAGAR", "TOTAL", "DESGLOSE", "RESUMEN", "SERVICIO", "IMPORTE", "CARGOS", "MISCELÁNEOS",
        "CUENTA", "MONEDA", "FECHA", "VENCIMIENTO", "PAGAR A", "CUENTA", "NO.FACTURA", "FOLIO",
        "PROLONGACIÓN", "CARRETERA", "CALLE", "AVENIDA", "AVE", "KM", "EDIFICIO", "PISO"
    };

    public ClienteEntity Parse(Stream pdfStream)
    {
        var texto = ExtraerTexto(pdfStream);
        var lineas = texto.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        // 1. Número de cliente
        var numeroCliente = Regex.Match(texto, @"Número de Cliente\s*:\s*(\d+)", RegexOptions.IgnoreCase).Groups[1].Value;

        // 2. Periodo de consumo (completo)
        var ciclo = Regex.Match(texto, @"Periodo de consum[o]?\s*:\s*(.*?)(?=\r?\n|$)", RegexOptions.IgnoreCase).Groups[1].Value.Trim();

        // 3. Total a pagar
        decimal monto = 0;
        var matchTotal = Regex.Match(texto, @"Total a\s*Pagar\s*(.*)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (matchTotal.Success)
        {
            var resto = matchTotal.Groups[1].Value;
            var numeros = Regex.Matches(resto, @"\d+\.\d+");
            if (numeros.Count > 0)
            {
                var ultimoNumero = numeros[numeros.Count - 1].Value;
                decimal.TryParse(ultimoNumero, NumberStyles.Any, CultureInfo.InvariantCulture, out monto);
            }
        }

        // 4. Oficina
        string oficina = "";
        var matchOficina = Regex.Match(texto, @"Oficina:\s*(.*?)(?=\r?\n|$)", RegexOptions.IgnoreCase);
        if (matchOficina.Success)
            oficina = matchOficina.Groups[1].Value.Trim();
        else
        {
            matchOficina = Regex.Match(texto, @"Pagar a:\s*(.*?)(?=\r?\n|$)", RegexOptions.IgnoreCase);
            if (matchOficina.Success)
                oficina = matchOficina.Groups[1].Value.Trim();
        }

        // 5. Provincia: buscar en la línea anterior y en la misma línea de "Periodo de consumo"
        string provincia = "";
        int periodoIndex = -1;
        for (int i = 0; i < lineas.Length; i++)
        {
            if (lineas[i].Contains("Periodo de consumo", StringComparison.OrdinalIgnoreCase))
            {
                periodoIndex = i;
                break;
            }
        }

        if (periodoIndex >= 0)
        {
            // Revisar la línea del periodo primero
            string lineaPeriodo = lineas[periodoIndex];
            provincia = BuscarProvinciaEnLinea(lineaPeriodo);

            // Si no se encontró, revisar la línea anterior
            if (string.IsNullOrEmpty(provincia) && periodoIndex > 0)
            {
                string lineaAnterior = lineas[periodoIndex - 1];
                provincia = BuscarProvinciaEnLinea(lineaAnterior);
            }
        }

        // Fallback: buscar en todo el texto
        if (string.IsNullOrEmpty(provincia))
        {
            provincia = BuscarProvinciaEnTexto(texto);
        }

        // 6. Nombre del cliente: líneas que contengan solo letras mayúsculas y espacios,
        //    que no sean palabras clave y que no sean la provincia.
        var lineasNombre = new List<string>();
        foreach (var linea in lineas)
        {
            string lineaTrim = linea.Trim();
            if (string.IsNullOrWhiteSpace(lineaTrim)) continue;

            // Verificar si la línea contiene solo letras mayúsculas (con acentos) y espacios
            if (!Regex.IsMatch(lineaTrim, @"^[A-ZÁÉÍÓÚÑ\s]+$")) continue;

            // Excluir si contiene alguna palabra clave
            bool contieneKeyword = KeywordsExcluir.Any(k => lineaTrim.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
            if (contieneKeyword) continue;

            // Excluir si es la provincia encontrada (para evitar duplicados)
            if (!string.IsNullOrEmpty(provincia) && lineaTrim.Contains(provincia)) continue;

            // Excluir líneas muy cortas (menos de 3 palabras) para evitar encabezados sueltos
            if (lineaTrim.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 3) continue;

            lineasNombre.Add(lineaTrim);
        }

        string nombre = lineasNombre.Count > 0 ? string.Join(" ", lineasNombre) : "";

        // Si no se encontró nombre, usar la primera línea no vacía como fallback
        if (string.IsNullOrWhiteSpace(nombre))
        {
            nombre = lineas.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l))?.Trim() ?? "";
        }

        return new ClienteEntity
        {
            Id = Guid.NewGuid(),
            Name = nombre,
            NumeroCliente = long.Parse(numeroCliente),
            Ciclo = ciclo,
            Monto = monto,
            Entidad = oficina,
            Provincia = provincia,
            CreatedDate = DateTime.UtcNow
        };
    }

    private string BuscarProvinciaEnLinea(string linea)
    {
        // Normalizar la línea (quitar acentos y convertir a mayúsculas)
        string lineaNorm = RemoveDiacritics(linea.ToUpperInvariant());

        // Separar por comas y buscar de atrás hacia adelante
        var partes = lineaNorm.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var parte in partes.Reverse())
        {
            string candidato = parte.Trim();
            foreach (var prov in Provincias)
            {
                if (candidato.Contains(prov) || prov.Contains(candidato)) // Coincidencia parcial o exacta
                {
                    // Devolver la parte original de la línea (con acentos) si es posible
                    int idx = linea.IndexOf(parte.Trim(), StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0)
                    {
                        return linea.Substring(idx, parte.Trim().Length).Trim();
                    }
                    return parte.Trim();
                }
            }
        }
        // Si no hay comas, buscar en toda la línea
        foreach (var prov in Provincias)
        {
            if (lineaNorm.Contains(prov))
            {
                // Encontrar la subcadena original
                int idx = linea.IndexOf(prov, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    return linea.Substring(idx, prov.Length);
                }
                return prov;
            }
        }
        return null;
    }

    private string BuscarProvinciaEnTexto(string texto)
    {
        string textoNorm = RemoveDiacritics(texto.ToUpperInvariant());
        foreach (var prov in Provincias)
        {
            if (textoNorm.Contains(prov))
            {
                return prov; // Devolvemos la forma normalizada
            }
        }
        return "";
    }
    private string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (char c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private string ExtraerTexto(Stream stream)
    {
        using var reader = new PdfReader(stream);
        using var pdf = new PdfDocument(reader);
        string texto = "";
        for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
        {
            texto += PdfTextExtractor.GetTextFromPage(pdf.GetPage(i));
        }
        return texto;
    }
}