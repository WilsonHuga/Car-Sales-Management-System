using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace Car_Sales_Management_System.DataModels
{
    public class UserService
    {

        private static UserService _instance;
        private static readonly object _lock = new object();

        private readonly IMongoCollection<User> _users;
        private User _currentUser;
        private readonly List<PasswordResetToken> _resetTokens = new List<PasswordResetToken>();


        public event EventHandler<UserChangedEventArgs> UserChanged;
        public class UserChangedEventArgs : EventArgs
        {
            public User User { get; set; }
            public bool IsLoggedIn { get; set; }
        }




        public class PasswordResetToken
        {
            public string Email { get; set; }
            public string Token { get; set; }
            public DateTime Expiry { get; set; }
        }

        //Singleton instance for UserService that ensure only one instance exists in the application
        public static UserService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new UserService();
                    }
                }
                return _instance;
            }
        }

        public UserService()
        {
            var client = new MongoClient("mongodb+srv://WilsonHuga:WilsonYoobee123@wilsonhugayoobee2025.nyfrb2o.mongodb.net/?retryWrites=true&w=majority&appName=WilsonHugaYoobee2025");
            var db = client.GetDatabase("carSales");
            _users = db.GetCollection<User>("users");

           // MessageBox.Show("Connected to MongoDB successfully!", "Connection Status", MessageBoxButton.OK, MessageBoxImage.Information);

            SeedAdminAccount();
        }

        // Property to get the current user
        public User CurrentUser
        {
            get => _currentUser;
            private set
            {
                var oldUser = _currentUser;
                _currentUser = value;

                // Trigger event when user changes
                UserChanged?.Invoke(this, new UserChangedEventArgs
                {
                    User = _currentUser,
                    IsLoggedIn = _currentUser != null
                });
            }
        }

        public bool IsLoggedIn => _currentUser != null;
        public bool IsAdmin => _currentUser?.Role == "Admin";
        public bool IsUser => _currentUser?.Role == "User";

        // Set the current user, after login
        public void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }



        public void AddUser(User user)
        {
            try
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                _users.InsertOne(user);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MongoDB error: " + ex.Message);
                throw;
            }
        }

        public User GetUserByEmail(string email)
        {
            try
            {
                return _users.Find(user => user.Email == email).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MongoDB error: " + ex.Message);
                return null;
            }
        }

        public bool ValidateUser(string email, string password)
        {
            try
            {
                var user = GetUserByEmail(email);
                if (user == null)
                    return false;

                // Verify the password
                return BCrypt.Net.BCrypt.Verify(password, user.Password);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MongoDB error: " + ex.Message);
                return false;
            }
        }

        public bool Login(string email, string password)
        {
            if (ValidateUser(email, password))
            {
                var user = GetUserByEmail(email);
                CurrentUser = user;
                return true;
            }
            return false;
        }
        public void Logout()
        {
            CurrentUser = null;
        }

        public string GeneratePasswordResetToken(string email)
        {
            var token = Guid.NewGuid().ToString();
            var resetToken = new PasswordResetToken
            {
                Email = email,
                Token = token,
                Expiry = DateTime.UtcNow.AddHours(1) // Token expires in 1 hour
            };
            _resetTokens.Add(resetToken);
            return token;
        }

        public bool ValidateResetToken(string email, string token)
        {
            var resetToken = _resetTokens.FirstOrDefault(t => t.Email == email && t.Token == token && t.Expiry > DateTime.UtcNow);
            return resetToken != null;
        }

        public void ResetPassword(string email, string token, string newPassword)
        {
            if (ValidateResetToken(email, token))
            {
                var user = GetUserByEmail(email);
                if (user != null)
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    var update = Builders<User>.Update.Set(u => u.Password, user.Password);
                    _users.UpdateOne(u => u.Email == email, update);
                    _resetTokens.RemoveAll(t => t.Email == email); // Invalidate all tokens for this user
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid or expired reset token.");
            }
        }
        private void SeedAdminAccount()
        {
            var adminEmail = "admin@gmail.com";
            var existingAdmin = _users.Find(u => u.Email == adminEmail).FirstOrDefault();
            if (existingAdmin == null)
            {
                var adminUser = new User
                {
                    Fullname = "Admin",
                    Email = adminEmail,
                    Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Phone = "0000000000",
                    Role = "Admin"
                };
                _users.InsertOne(adminUser);
                Debug.WriteLine("Admin account created.");
            }
        }

        public bool UpdateUserRole(string email, string newRole)
        {
            var update = Builders<User>.Update.Set(u => u.Role, newRole);
            var result = _users.UpdateOne(u => u.Email == email, update);
            return result.ModifiedCount > 0;
        }
    }
}

