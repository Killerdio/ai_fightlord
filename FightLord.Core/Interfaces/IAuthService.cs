using System;
using System.Threading.Tasks;
using FightLord.Core.Entities;

namespace FightLord.Core.Interfaces
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);
        Task<User> RegisterAsync(string username, string password);
    }
}
