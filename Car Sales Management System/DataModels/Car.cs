using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

//public enum CarCondition
//{
//    New,
//    Used
//}

//public enum CarStatus
//{
//    Available,
//    Reserved,
//    Sold
//}

public class Car
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    [BsonElement("make")]
    public string Make { get; set; } = string.Empty;

    [BsonElement("model")]
    public string Model { get; set; } = string.Empty;

    [BsonElement("year")]
    public int Year { get; set; } 
    [BsonElement("vin")]
    public string VIN { get; set; } = string.Empty;

    [BsonElement("mileage")]
    public int Mileage { get; set; }

    [BsonElement("color")]
    public string Color { get; set; } = string.Empty;

    [BsonElement("price")]
    public int Price { get; set; }

    [BsonElement("condition")]
    public string Condition { get; set; } = string.Empty;// New or Used 

    [BsonElement("status")]
    public string Status { get; set; } = string.Empty; // Available, Reserved, Sold

    [BsonElement("listedDate")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime ListedDate { get; set; }

}
