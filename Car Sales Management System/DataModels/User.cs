using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Car_Sales_Management_System.DataModels
{
    public enum UserRole
    {
        Admin,
        Salesperson
    }

    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("role")]
        public string Role { get; set; } = string.Empty;
    }
}