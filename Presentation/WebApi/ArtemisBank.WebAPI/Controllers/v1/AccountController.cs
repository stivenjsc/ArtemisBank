using ArtemisBank.Core.Application.Interfaces.IServices;
using Asp.Versioning;

namespace ArtemisBank.WebAPI.Controllers.v1
{
    [ApiVersion("1.0")]
    public class AccountController : BaseApiController
    {
        private readonly IUserService userService;
    }
}
