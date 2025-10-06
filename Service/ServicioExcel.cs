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
    private const int COL_PEA_NIVEL_MSNM = 6;   // C: Peñitas
    private const int COL_ANG_NIVEL_MSNM = 2;  // N: Angostura
    private const int COL_MMT_NIVEL_MSNM = 3;  // Z: Chicoasén (MMT)
    private const int COL_MAL_NIVEL_MSNM = 5;  // AL: Malpaso
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
            // --- Hoja de porcentajes ---
            var hojaPorcentaje = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals("Niveles", StringComparison.OrdinalIgnoreCase));
            if (hojaPorcentaje == null) throw new Exception("No se encontró la hoja 'Niveles'.");

            // --- Hoja de niveles m.s.n.m ---
            var hojaNivel = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name.Equals("ElevSHG", StringComparison.OrdinalIgnoreCase));
            if (hojaNivel == null) throw new Exception("No se encontró la hoja 'ElevSHG'.");

            // --- Última fila con datos de porcentaje ---
            int[] columnasPorcentaje = { 12, 24, 35, 46, 47 }; // L, X, AI, AT, AU
            int ultimaFilaPct = hojaPorcentaje.Dimension.End.Row;
            while (ultimaFilaPct >= 1 && !columnasPorcentaje.Any(c => !string.IsNullOrWhiteSpace(hojaPorcentaje.Cells[ultimaFilaPct, c].Text)))
                ultimaFilaPct--;

            // --- Última fila con datos de nivel ---
            int[] columnasNivel = { 2, 3, 5, 6 }; // B, C, E, F
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
            double peaNivel = leerDouble(hojaNivel, ultimaFilaNivel, 6);   // B: Peñitas
            double angNivel = leerDouble(hojaNivel, ultimaFilaNivel, 2);   // B: Angostura (Columna B)
            double mmtNivel = leerDouble(hojaNivel, ultimaFilaNivel, 3);   // C: Chicoasén
            double malNivel = leerDouble(hojaNivel, ultimaFilaNivel, 5);   // E: Malpaso
           

            return new
            {
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