using UserManagmentAPI.Models;

namespace UserManagmentAPI.Services
{
    public class UserRepository
    {
        private readonly List<User> _users = new();
        private int _nextId = 1;

        public IEnumerable<User> GetAll() => _users;

        public User? GetById(int id) => _users.FirstOrDefault(u => u.Id == id);

        public User Create(User user)
        {
            user.Id = _nextId++;
            _users.Add(user);
            return user;
        }

        public bool Update(int id, User updatedUser)
        {
            var user = GetById(id);
            if (user == null) return false;

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Department = updatedUser.Department;
            user.Email = updatedUser.Email;
            return true;
        }

        public bool Delete(int id)
        {
            var user = GetById(id);
            if (user == null) return false;
            _users.Remove(user);
            return true;
        }
    }
}
