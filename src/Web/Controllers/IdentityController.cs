using System.Threading.Tasks;
using Application.Users.Commands;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/identity")]
    public class IdentityController : ControllerBase
    {
        private readonly Mediator _mediator;

        public IdentityController(Mediator mediator)
        {
            _mediator = Guard.Against.Null(mediator, nameof(mediator));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            var result = await _mediator.Send(command);
            
            return StatusCode(result.StatusCode, result);
        }
        
        [HttpPost("new")]
        public async Task<ActionResult> Registration(RegistrationCommand command)
        {
            var result = await _mediator.Send(command);
            
            return StatusCode(result.StatusCode, result);
        }
        
        [HttpPost("token/refresh")]
        public async Task<ActionResult> RefreshToken(RefreshTokenCommand command)
        {
            var result = await _mediator.Send(command);
            
            return StatusCode(result.StatusCode, result);
        }
        
        [HttpPost("password/reset")]
        public async Task<ActionResult> RequestResetPasswordLink(RequestResetPasswordLinkCommand command)
        {
            var result = await _mediator.Send(command);
            
            return StatusCode(result.StatusCode, result);
        }
        
        [HttpGet("get-something")]
        [Authorize]
        public async Task<ActionResult> Get()
        {
            return StatusCode(401);
        }
    }
}