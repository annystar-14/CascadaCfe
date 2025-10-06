using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;

// NOTA: Asegúrate de tener instalado el paquete NuGet 'EPPlus' en tu proyecto.
//       ExcelPackage.LicenseContext = LicenseContext.NonCommercial; debe establecerse una vez 
//       al inicio de la aplicación (por ejemplo, en Program.cs o Startup.cs) si usas la versión gratuita de EPPlus.

public class ServicioExcel
{
    private readonly string _rutaArchivoExcel;

    private const string NOMBRE_HOJA = "Niveles";

    // Constructor: La ruta al archivo Excel debe inyectarse o definirse aquí.
    public ServicioExcel(string rutaArchivoExcel)
    {
        _rutaArchivoExcel = rutaArchivoExcel;
    }

    // Definición de las columnas de la hoja "Niveles" (EPPlus usa índice base 1)
    // ============== DEFINICIONES DE COLUMNAS CORRECTAS (AJUSTADAS) ==============

    // ============== DEFINICIONES DE COLUMNAS CORRECTAS (ÚLTIMO AJUSTE) ==============

    private const int COL_HORA = 2; // B: Hora

    // NIVELES m.s.n.m. (COLUMNA DE ELEVACIÓN)
    private const int COL_PEA_NIVEL_MSNM = 3;   // C: Peñitas
    private const int COL_ANG_NIVEL_MSNM = 14;  // N: Angostura
    private const int COL_MMT_NIVEL_MSNM = 26;  // Z: Chicoasén (MMT)
    private const int COL_MAL_NIVEL_MSNM = 38;  // AL: Malpaso
    private const int COL_JGRIJ_NIVEL_MSNM = 50; // AX: Juan de Grijalva

    // PORCENTAJES DE LLENADO ÚTIL AL NAMO % 
    private const int COL_PEA_PORCENTAJE = 12; // L: Peñitas
    private const int COL_ANG_PORCENTAJE = 25; // Y: Angostura
    private const int COL_MMT_PORCENTAJE = 37; // AK: Chicoasén (MMT)
    private const int COL_MAL_PORCENTAJE = 49; // AW: Malpaso
    private const int COL_JGRIJ_PORCENTAJE = 61; // BI: Juan de Grijalva
    // ============================================================================

    public object LeerDatos()
    {
        if (!File.Exists(_rutaArchivoExcel))
            throw new FileNotFoundException($"No se encontró el archivo en: {_rutaArchivoExcel}");

        using (var package = new ExcelPackage(new FileInfo(_rutaArchivoExcel)))
        {
            var worksheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals(NOMBRE_HOJA, StringComparison.OrdinalIgnoreCase));
            if (worksheet == null)
            {
                var hojas = string.Join(", ", package.Workbook.Worksheets.Select(w => w.Name));
                throw new Exception($"No se encontró la hoja 'Niveles'. Hojas disponibles: {hojas}");
            }

            // Columnas de porcentaje que quieres usar
            int[] columnasPorcentaje = { 12, 24, 35, 46, 47 }; // L, X, AI, AT, AU

            // Encontrar la última fila que tenga datos en alguna de esas columnas
            int ultimaFila = worksheet.Dimension.End.Row;
            while (ultimaFila >= 1)
            {
                bool hayDatos = columnasPorcentaje.Any(col => !string.IsNullOrWhiteSpace(worksheet.Cells[ultimaFila, col].Text));
                if (hayDatos) break;
                ultimaFila--;
            }

            if (ultimaFila < 5)
                throw new Exception("No se encontraron datos en las columnas de porcentaje.");

            string hora = worksheet.Cells[ultimaFila, COL_HORA].Text?.Trim() ?? "N/A";

            Func<int, double> leerDouble = (col) =>
            {
                string texto = worksheet.Cells[ultimaFila, col].Text?.Trim();
                if (string.IsNullOrEmpty(texto)) return 0.0;
                if (double.TryParse(texto, out double val)) return val;
                texto = texto.Replace(',', '.');
                if (double.TryParse(texto, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
                    return val;
                return 0.0;
            };

            // Leer porcentajes exactos de las columnas que mencionaste
            double peaPct = leerDouble(12);  // Peñitas L
            double angPct = leerDouble(24);  // Angostura X
            double mmtPct = leerDouble(35);  // Manuel Moreno Torres AI
            double malPct = leerDouble(46);  // Malpaso AT
            double jGrijPct = leerDouble(47); // Juan de Grijalva AU

            return new
            {
                presas = new
                {
                    penitas = new { nivelActual = peaPct, min = 0.0, max = 100.0, hora },
                    angostura = new { nivelActual = angPct, min = 0.0, max = 100.0, hora },
                    chicoasen = new { nivelActual = mmtPct, min = 0.0, max = 100.0, hora },
                    malpaso = new { nivelActual = malPct, min = 0.0, max = 100.0, hora },
                    juanDeGrijalva = new { nivelActual = jGrijPct, min = 0.0, max = 100.0, hora }
                }
            };
        }
    }



}