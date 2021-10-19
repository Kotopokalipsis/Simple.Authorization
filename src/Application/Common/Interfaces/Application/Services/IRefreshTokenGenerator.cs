using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Common.Interfaces.Application.Services
{
    public interface IRefreshTokenGenerator
    {
        Task<string> Generate(User user);
    }
}