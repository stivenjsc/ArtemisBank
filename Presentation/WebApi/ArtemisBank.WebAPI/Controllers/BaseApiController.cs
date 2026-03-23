using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebAPI.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {

    }
}
