using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
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

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }


        public class PasswordResetToken
        {
            public string? Email { get; set; }
            public string? Token { get; set; }
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

        public List<User> GetAllUsers()
        {
            try
            {
                return _users.Find(user => true).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MongoDB error: " + ex.Message);
                return new List<User>();
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

        public bool UpdateUserDetails(string email, string fullname, string phone)
        {
            try
            {
                if (!IsValidEmail(email))
                    throw new ArgumentException("Invalid email format.", nameof(email));

                var update = Builders<User>.Update
                    .Set(u => u.Fullname, fullname)
                    .Set(u => u.Phone, phone);

                var result = _users.UpdateOne(u => u.Email == email, update);
                Debug.WriteLine($"[UpdateUserDetails] {DateTime.UtcNow}: User {email} updated: {result.ModifiedCount > 0}");
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateUserDetails] {DateTime.UtcNow}: MongoDB error: {ex.Message}");
                throw;
            }
        }

        public bool UpdateUserPassword(string email, string newPassword)
        {
            try
            {
                if (!IsValidEmail(email))
                    throw new ArgumentException("Invalid email format.", nameof(email));
                if (string.IsNullOrWhiteSpace(newPassword))
                    throw new ArgumentException("New password cannot be empty.", nameof(newPassword));

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                var update = Builders<User>.Update.Set(u => u.Password, hashedPassword);
                var result = _users.UpdateOne(u => u.Email == email, update);
                Debug.WriteLine($"[UpdateUserPassword] {DateTime.UtcNow}: Password updated for {email}: {result.ModifiedCount > 0}");
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateUserPassword] {DateTime.UtcNow}: MongoDB error: {ex.Message}");
                throw;
            }
        }

        public bool UpdateUserRole(string email, string newRole)
        {
            try
            {
                if (!IsValidEmail(email))
                    throw new ArgumentException("Invalid email format.", nameof(email));
                if (email == "admin@gmail.com")
                    throw new InvalidOperationException("The default admin account role cannot be modified.");
                if (string.IsNullOrWhiteSpace(newRole) || (!newRole.Equals("User", StringComparison.OrdinalIgnoreCase) && !newRole.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
                    throw new ArgumentException("Role must be 'User' or 'Admin'.", nameof(newRole));

                // Normalize role to standard casing
                newRole = newRole.Equals("user", StringComparison.OrdinalIgnoreCase) ? "User" : "Admin";

                // Check if user exists
                var user = _users.Find(u => u.Email == email).FirstOrDefault();
                if (user == null)
                {
                    Debug.WriteLine($"[UpdateUserRole] {DateTime.UtcNow}: User {email} not found.");
                    return false;
                }

                // Check if existing role is the same as new role
                string currentRole = user.Role ?? "User"; // Default to "User" if null
                if (currentRole.Equals(newRole, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"[UpdateUserRole] {DateTime.UtcNow}: Role for {email} is already {newRole}. No update needed.");
                    return true;
                }

                var update = Builders<User>.Update.Set(u => u.Role, newRole);
                var result = _users.UpdateOne(u => u.Email == email, update);
                Debug.WriteLine($"[UpdateUserRole] {DateTime.UtcNow}: Role updated for {email} to {newRole}: {result.ModifiedCount > 0}");
                return result.ModifiedCount > 0 || currentRole.Equals(newRole, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateUserRole] {DateTime.UtcNow}: MongoDB error: {ex.Message}");
                throw;
            }
        }

        public bool DeleteUser(string email)
        {
            try
            {
                if (!IsValidEmail(email))
                    throw new ArgumentException("Invalid email format.", nameof(email));
                if (email == "admin@gmail.com")
                    throw new InvalidOperationException("The default admin account cannot be deleted.");

                var result = _users.DeleteOne(u => u.Email == email);
                Debug.WriteLine($"[DeleteUser] {DateTime.UtcNow}: User {email} deletion attempt: {result.DeletedCount} documents deleted");

                if (result.DeletedCount == 0)
                {
                    Debug.WriteLine($"[DeleteUser] {DateTime.UtcNow}: No user found with email {email}");
                    return false;
                }

                if (CurrentUser != null && CurrentUser.Email == email)
                    Logout();

                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DeleteUser] {DateTime.UtcNow}: MongoDB error: {ex.Message}");
                throw;
            }
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

        // User activity tracking
        public void IncrementCarsViewed(string email)
        {
            try
            {
                var update = Builders<User>.Update.Inc(u => u.CarsViewed, 1);
                _users.UpdateOne(u => u.Email == email, update);

                if (CurrentUser?.Email == email)
                {
                    CurrentUser.CarsViewed++;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error incrementing cars viewed: {ex.Message}");
            }
        }

        public void IncrementInquiriesMade(string email)
        {
            try
            {
                var update = Builders<User>.Update.Inc(u => u.InquiriesMade, 1);
                _users.UpdateOne(u => u.Email == email, update);

                if (CurrentUser?.Email == email)
                {
                    CurrentUser.InquiriesMade++;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error incrementing inquiries made: {ex.Message}");
            }
        }

        //public void IncrementCarsAdded(string email)
        //{
        //    try
        //    {
        //        var update = Builders<User>.Update.Inc(u => u.CarsAdded, 1);
        //        _users.UpdateOne(u => u.Email == email, update);

        //        if (CurrentUser?.Email == email)
        //        {
        //            CurrentUser.CarsAdded++;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Error incrementing cars added: {ex.Message}");
        //    }
        //}

        public bool UpdateProfilePicture(string email, byte[] profilePicture)
        {
            try
            {
                var update = Builders<User>.Update.Set(u => u.ProfilePicture, profilePicture);
                var result = _users.UpdateOne(u => u.Email == email, update);

                if (CurrentUser?.Email == email)
                {
                    CurrentUser.ProfilePicture = profilePicture;
                }

                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating profile picture: {ex.Message}");
                return false;
            }
        }

        public User GetUserById(string id)
        {
            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    Debug.WriteLine($"Invalid ObjectId format: {id}");
                    return null;
                }
                return _users.Find(u => u.Id == objectId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MongoDB error in GetUserById: {ex.Message}");
                return null;
            }
        }
    }
}

