using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
public class ServicioExcel
{
    private readonly string _carpetaExcel;

    private const string Patron_Archivo = "FIN*.xlsx";

    // Constructor: La ruta al archivo Excel debe inyectarse o definirse aquí.


    public ServicioExcel(string carpetaExcel)
    {
        _carpetaExcel = carpetaExcel;
    }

    private string ObtenerUltimoArchivoExcel() 
    {
        var dir = new DirectoryInfo(_carpetaExcel);
        var archivoReciente = dir.GetFiles(Patron_Archivo)
                                 .OrderByDescending(f => f.LastWriteTime)
                                 .FirstOrDefault();

        if (archivoReciente == null)
            throw new FileNotFoundException($"No se encontró ningún Excel en: {_carpetaExcel}");
        return archivoReciente.FullName;
    }

    public object LeerDatos()
    {
        string rutaArchivo = ObtenerUltimoArchivoExcel();
        var infoArchivo = new FileInfo(rutaArchivo);
        string ultimaActualizacionArchivo = infoArchivo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");

        using (var package = new ExcelPackage(new FileInfo(rutaArchivo)))
        {                   
            // --- Hoja de porcentajes ---
            var hojaPorcentaje = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals("Niveles", StringComparison.OrdinalIgnoreCase));
            if (hojaPorcentaje == null) throw new Exception("No se encontró la hoja 'Niveles'.");
                       
            // --- Hoja de niveles m.s.n.m ---
            var hojaNivel = package.Workbook.Worksheets
                .FirstOrDefault(ws => ws.Name.Equals("Niveles", StringComparison.OrdinalIgnoreCase));
            if (hojaNivel == null) throw new Exception("No se encontró la hoja 'Niveles'.");
                                                                                            

            // --- Última fila con datos de porcentaje ---
            int[] columnasPorcentaje = { 12, 24, 35, 46, 47 }; // L, X, AI, AT, AU
            int ultimaFilaPct = hojaPorcentaje.Dimension.End.Row;
            while (ultimaFilaPct >= 1 && !columnasPorcentaje.Any(c => !string.IsNullOrWhiteSpace(hojaPorcentaje.Cells[ultimaFilaPct, c].Text)))
                ultimaFilaPct--;

            // --- Última fila con datos de nivel ---
            int[] columnasNivel = { 4, 14, 25, 36 }; // B, C, E, F
            int ultimaFilaNivel = hojaNivel.Dimension.End.Row;
            while (ultimaFilaNivel >= 1 && !columnasNivel.Any(c => !string.IsNullOrWhiteSpace(hojaNivel.Cells[ultimaFilaNivel, c].Text)))
                ultimaFilaNivel--;

            string hora = hojaPorcentaje.Cells[ultimaFilaPct, 2].Text?.Trim() ?? "N/A"; // hora tomada de hoja porcentaje

            Func<ExcelWorksheet, int, int, double> leerDouble = (hoja, fila, col) =>
            {
                string texto = hoja.Cells[fila, col].Text?.Trim();
                if (string.IsNullOrEmpty(texto)) return 0.0;
                if (double.TryParse(texto, out double val)) return val;
                texto = texto.Replace(',', '.');
                if (double.TryParse(texto, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
                    return val;
                return 0.0;
            };

            // --- Leer porcentajes ---
            double peaPct = leerDouble(hojaPorcentaje, ultimaFilaPct, 12);  // Peñitas
            double angPct = leerDouble(hojaPorcentaje, ultimaFilaPct, 24);  // Angostura
            double mmtPct = leerDouble(hojaPorcentaje, ultimaFilaPct, 35);  // Chicoasén
            double malPct = leerDouble(hojaPorcentaje, ultimaFilaPct, 46);  // Malpaso
            double jGrijPct = leerDouble(hojaPorcentaje, ultimaFilaPct, 47); // Juan de Grijalva

            // --- Leer niveles m.s.n.m ---
            double peaNivel = leerDouble(hojaNivel, ultimaFilaNivel, 3);   // B: Peñitas
            double angNivel = leerDouble(hojaNivel, ultimaFilaNivel, 14);   // B: Angostura (Columna B)
            double mmtNivel = leerDouble(hojaNivel, ultimaFilaNivel, 25);   // C: Chicoasén
            double malNivel = leerDouble(hojaNivel, ultimaFilaNivel, 36);   // E: Malpaso
           

            return new
            {
                ultimaActualizacionArchivo,
                presas = new
                {
                    penitas = new { nivel = peaNivel, porcentaje = peaPct, hora },
                    angostura = new { nivel = angNivel, porcentaje = angPct, hora },
                    chicoasen = new { nivel = mmtNivel, porcentaje = mmtPct, hora },
                    malpaso = new { nivel = malNivel, porcentaje = malPct, hora },
                    juanDeGrijalva = new { nivel = (double?)null, porcentaje = jGrijPct, hora }
                }
            };
        }
    }
}