using System.Security.Claims;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Services.CharacterService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace dotnet_rpg.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CharacterController  : ControllerBase
{
    private readonly ICharacterService _characterService;
    
    public CharacterController(ICharacterService characterService)
    {
        _characterService = characterService;
    }
    // [AllowAnonymous]
    [HttpGet("GetAll")]
    public async Task<ActionResult<ServiceResponse<List<GetCharacterDto>>>> Get()
    {
        int userId = int.Parse(User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier).Value); 
        return Ok(await _characterService.GetAllCharacters(userId));
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceResponse<GetCharacterDto>>> GetSingle(int id)
    {
        return Ok(await _characterService.GetCharacterById(id));
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse<List<GetCharacterDto>>>> AddCharacter(AddCharacterDto character)
    {
        return Ok(await _characterService.AddCharacter(character));
    }
    
    [HttpPut]
    public async Task<ActionResult<ServiceResponse<GetCharacterDto>>> UpdateCharacter(UpdateCharacterDto character)
    {
        var response = await _characterService.UpdateCharacter(character);
        return response.Data != null ? Ok(response) : NotFound(response);
    }
    
    [HttpDelete]
    public async Task<ActionResult<ServiceResponse<List<GetCharacterDto>>>> DeleteCharacter(int id)
    {
        var response = await _characterService.DeleteCharacter(id);
        return response.Data != null ? Ok(response) : NotFound(response);
    }
}