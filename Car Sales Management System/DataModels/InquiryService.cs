using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Diagnostics;

namespace Car_Sales_Management_System.DataModels
{
    public class InquiryService
    {
        //Purchased 5,000+ times
        private readonly IMongoCollection<Inquiry> _inquiries;

        public InquiryService()
        {
            var client = new MongoClient("mongodb+srv://WilsonHuga:WilsonYoobee123@wilsonhugayoobee2025.nyfrb2o.mongodb.net/?retryWrites=true&w=majority&appName=WilsonHugaYoobee2025");
            var db = client.GetDatabase("carSales");
            _inquiries = db.GetCollection<Inquiry>("inquiries");
        }

        public void AddInquiry(Inquiry inquiry)
        {
            try
            {
                _inquiries.InsertOne(inquiry);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MongoDB error in AddInquiry: {ex.Message}");
                throw;
            }
        }

        public List<Inquiry> GetInquiriesByCarId(string carId)
        {
            try
            {
                if (!ObjectId.TryParse(carId, out ObjectId objectId))
                {
                    Debug.WriteLine($"Invalid ObjectId format: {carId}");
                    return new List<Inquiry>();
                }
                return _inquiries.Find(i => i.CarId == objectId).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MongoDB error in GetInquiriesByCarId: {ex.Message}");
                return new List<Inquiry>();
            }
        }

        public List<Inquiry> GetInquiriesByUserId(string userId)
        {
            try
            {
                if (!ObjectId.TryParse(userId, out ObjectId objectId))
                {
                    Debug.WriteLine($"Invalid ObjectId format: {userId}");
                    return new List<Inquiry>();
                }
                return _inquiries.Find(i => i.UserId == objectId).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MongoDB error in GetInquiriesByUserId: {ex.Message}");
                return new List<Inquiry>();
            }
        }

        public List<Inquiry> GetAllInquiries()
        {
            try
            {
                return _inquiries.Find(inquiry => true).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MongoDB error in GetAllInquiries: {ex.Message}");
                return new List<Inquiry>();
            }
        }

        public bool UpdateInquiry(string id, Inquiry updatedInquiry)
        {
            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    Debug.WriteLine($"Invalid ObjectId format: {id}");
                    return false;
                }

                var filter = Builders<Inquiry>.Filter.Eq(i => i.Id, objectId);
                var update = Builders<Inquiry>.Update
                    .Set(i => i.Phone, updatedInquiry.Phone)
                    .Set(i => i.Message, updatedInquiry.Message)
                    .Set(i => i.Status, updatedInquiry.Status);

                var result = _inquiries.UpdateOne(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MongoDB error in UpdateInquiry: {ex.Message}");
                return false;
            }
        }

        public bool DeleteInquiry(string id)
        {
            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    Debug.WriteLine($"Invalid ObjectId format: {id}");
                    return false;
                }

                var filter = Builders<Inquiry>.Filter.Eq(i => i.Id, objectId);
                var result = _inquiries.DeleteOne(filter);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MongoDB error in DeleteInquiry: {ex.Message}");
                return false;
            }
        }



    }
}