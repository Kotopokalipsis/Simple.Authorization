using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Common.Interfaces.Application.Services;

public interface ITokenHelper
{
    Task<string> GenerateNewRefreshToken(User user);
    Task<string> GenerateNewAccessToken(User user);
    Task<User> GetUserByRefreshToken(string refreshToken);
}