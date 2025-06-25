using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Car_Sales_Management_System.DataModels
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("fullName")]
        public string Fullname { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("password")]
        public string Password { get; set; } = string.Empty;

        [BsonElement("phone")]
        public string Phone { get; set; } = string.Empty;

        [BsonElement("role")]
        public string Role { get; set; } = "User"; // Default role is User

        [BsonElement("profilePicture")]
        public byte[] ProfilePicture { get; set; }

        [BsonElement("carsViewed")]
        public int CarsViewed { get; set; } = 0;

        [BsonElement("inquiriesMade")]
        public int InquiriesMade { get; set; } = 0;

        [BsonElement("lastLogin")]
        public DateTime? LastLogin { get; set; }



    }
}