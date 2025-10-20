using Microsoft.AspNetCore.Mvc;
using SampleETag.Attributes;

namespace SampleETag.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MainController : ControllerBase
{
    [ETag]
    [HttpGet]
    public IActionResult Index()
    {
        return Ok("Hello from SampleETag!");
    }
}