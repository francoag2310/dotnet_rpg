using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace dotnet_rpg.Data;

public class AuthRepository : IAuthRepository
{
    private readonly DataContext _dataContext;
    private readonly IConfiguration _configuration;
    private readonly string TokenEncodingLocation = "AppSettings:Token";

    public AuthRepository(DataContext dataContext, IConfiguration configuration)
    {
        _dataContext = dataContext;
        _configuration = configuration;
    }
    public async Task<ServiceResponse<int>> Register(User user, string password)
    {
        ServiceResponse<int> serviceResponse = new ServiceResponse<int>(){};
        if (await UserExists(user.Username))
        {
            serviceResponse.Success = false;
            serviceResponse.Message = "User Already Exists";
        }
        
        var userWithPassword = CreateWithUserPassword(user, password);
        
        _dataContext.Users.Add(userWithPassword);
        await _dataContext.SaveChangesAsync();
        
        serviceResponse.Data = userWithPassword.Id;
        return serviceResponse;
    
    }

    public async Task<ServiceResponse<string>> Login(string username, string password)
    {
        var serviceResponse = new ServiceResponse<string>();
        var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        
        if(user == null){
            serviceResponse.Success = false;
            serviceResponse.Message = "User Not Found";
        }
        
        else if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
        {
            serviceResponse.Success = false;
            serviceResponse.Message = "Wrong Password";
        }
        else
        {
            serviceResponse.Data = CreateToken(user);
        }
        
        return serviceResponse;
    }

    public async Task<bool> UserExists(string username)
    {
        if(await _dataContext.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
        {
            return true;
        }
        return false;
    }
    

    private User CreateWithUserPassword(User user, string password)
    {
        CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;
        return user;
    }
    
    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new System.Security.Cryptography.HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }

    private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
        { 
            var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computeHash.SequenceEqual(passwordHash);
        }
    }
    
    private string CreateToken(User user)
    {
        List<Claim> claims = new List<Claim>()
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
        };

        SymmetricSecurityKey key = new(System.Text.Encoding.UTF8
            .GetBytes(_configuration.GetSection(TokenEncodingLocation).Value));

        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha512Signature);

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(1),
            SigningCredentials = credentials
        };

        JwtSecurityTokenHandler tokenHandler = new();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        
        return tokenHandler.WriteToken(token);
    }
}