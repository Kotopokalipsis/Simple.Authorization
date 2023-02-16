using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Common.Interfaces.Application.Services;

public interface ITokenHelper
{
    void SetAccessToken(User user);
    Task SetRefreshToken(User user);
    Task<User> GetUserByRefreshToken(string refreshToken);
}