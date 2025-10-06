using grijalvaApi.Models;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DatosController : ControllerBase
{
    private readonly ServicioExcel _servicioExcel;

    public DatosController(ServicioExcel servicioExcel)
    {
        _servicioExcel = servicioExcel;
    }

    [HttpGet]
    public IActionResult GetDatos()
    {
        return Ok(_servicioExcel.LeerDatos());
    }
}
