using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Enums;
using dotnet_rpg.Services.CharacterService;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.CharacterService;

public class CharacterService : ICharacterService
{
    private readonly IMapper _mapper;
    private readonly DataContext _dataContext;

    public CharacterService(IMapper mapper, DataContext dataContext)
    {
        _mapper = mapper;
        _dataContext = dataContext;
    }
    
    public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters(int userId)
    {
        var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
        var dbCharacters = await _dataContext.Characters.Where(c => c.User.Id == userId).ToListAsync();
        serviceResponse.Data = AutoMapCharacterList(dbCharacters);
        return serviceResponse;
    }

    public async  Task<ServiceResponse<GetCharacterDto>>GetCharacterById(int id)
    {
        var dbCharacters = await _dataContext.Characters.FirstOrDefaultAsync(c => c.Id ==id);
        var serviceResponse = new ServiceResponse<GetCharacterDto>() { Data = _mapper.Map<GetCharacterDto>(dbCharacters)};
        return serviceResponse;
    }
    public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
    {
        Character character = _mapper.Map<Character>(newCharacter);
        _dataContext.Characters.Add(character);
        await _dataContext.SaveChangesAsync();
        var characters = await _dataContext.Characters.ToListAsync();
        var serviceResponse = new ServiceResponse<List<GetCharacterDto>>() {Data = AutoMapCharacterList(characters)};
        return serviceResponse;
    }

    public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto characterToUpdate)
    {
        var serviceResponse = new ServiceResponse<GetCharacterDto>();
        try
        {
            var dbCharacters = await _dataContext.Characters.FirstOrDefaultAsync(c => c.Id ==characterToUpdate.Id);
            _mapper.Map(characterToUpdate, dbCharacters);
            await _dataContext.SaveChangesAsync();
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(dbCharacters);
            return serviceResponse;
        }
        catch (Exception ex)
        {
            serviceResponse.Success = false;
            serviceResponse.Message = ex.Message;
        }

        return serviceResponse;
    }

    public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
    {
        var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
        try
        {
            var character =  await _dataContext.Characters.FirstAsync(c => c.Id == id);
            _dataContext.Characters.Remove(character);
            await _dataContext.SaveChangesAsync();
            var characters = await _dataContext.Characters.ToListAsync();
            serviceResponse.Data = AutoMapCharacterList(characters);
        }
        catch (Exception ex)
        {
            serviceResponse.Success = false;
            serviceResponse.Message = ex.Message;
        }
        
        return serviceResponse;
    }

    private List<GetCharacterDto> AutoMapCharacterList(List<Character> data)
    {
        return data.Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
    }
}