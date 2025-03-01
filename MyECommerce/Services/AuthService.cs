using System.Linq;
using MyECommerce.Data;
using MyECommerce.Models;

namespace MyECommerce.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public User? FindUser(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public User RegisterUser(string name, string email)
        {
            var user = new User { Name = name, Email = email, CreatedDate = DateTime.UtcNow };
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }
    }
}
