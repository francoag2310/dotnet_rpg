using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers;

[ApiController]
[Route(("[Controller]"))]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _authRepository;

    public AuthController(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ServiceResponse<int>>> Register(UserRegisterDto request)
    {
        var response = await _authRepository.Register(
            new User() { Username = request.Username }, request.Password);

        return response.Success ? Ok(response) : BadRequest(response);
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<ServiceResponse<int>>> Login(UserLoginDto request)
    {
        var response = await _authRepository.Login(
            request.Username, request.Password);

        return response.Success ? Ok(response) : BadRequest(response);
    }

}