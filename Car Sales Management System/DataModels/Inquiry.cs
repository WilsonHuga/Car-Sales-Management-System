using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Car_Sales_Management_System.DataModels
{
    [BsonIgnoreExtraElements]
    public class Inquiry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        [BsonElement("carId")]
        public ObjectId CarId { get; set; }

        [BsonElement("userId")]
        public ObjectId UserId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("phone")]
        public string Phone { get; set; } = string.Empty;

        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;

        [BsonElement("inquiryDate")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime InquiryDate { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Pending"; // e.g., Pending, Responded, Closed
    }
}