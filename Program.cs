using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Habilitar CORS para tu HTML
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5270") // Cambia si abres con otra URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configurar licencia de EPPlus 8+
ExcelPackage.License.SetNonCommercialPersonal("Ana Cristina Lara Grajales");

// Ruta del Excel (configurable desde appsettings.json)
var config = builder.Configuration;
string excelPath = config["ExcelPath"];
builder.Services.AddSingleton(new ServicioExcel(excelPath));


var app = builder.Build();

// Habilitar archivos estáticos (wwwroot)
app.UseStaticFiles();

// Habilitar CORS
app.UseCors();

// Endpoint que devuelve JSON con los datos de las presas
app.MapGet("/api/data", (ServicioExcel servicio) =>
{
    var datos = servicio.LeerDatos();
    // Retorna JSON con la clave "presas" que tu canvas.js espera
    return Results.Json(datos);
});

// Fallback → sirve index.html si no hay ruta
app.MapFallbackToFile("index.html");

app.UseStaticFiles();

// Ejecutar la app
app.Run();
