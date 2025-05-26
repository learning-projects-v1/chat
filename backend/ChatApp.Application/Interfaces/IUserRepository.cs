using ChatApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Get User by email or null if doesn't exist
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<User?> GetUserByEmail(string email);
    Task<User?> GetUserByUsername(string username);
}
