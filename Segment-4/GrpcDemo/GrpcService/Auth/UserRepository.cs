using GrpcService.Models;
using System.Collections.Generic;
using System.Linq;

namespace GrpcService.Auth
{
    internal static class UserRepository
    {
        private static readonly List<User> users = new List<User>
        {
            new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Username = "john_smith",
                Password = "Pa$$w0rd!",
                Role = "user"
            },
            new User
            {
                Id = 1,
                FirstName = "Mike",
                LastName = "London",
                Username = "mike_london",
                Password = "Pa$$w0rd!",
                Role = "admin"
            }
        };

        internal static User Authenticate(string username, string password)
        {
            return users
                .Where(u => u.Username == username && u.Password == password)
                .SingleOrDefault();
        }
    }
}
