using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Diagnostics;

namespace Car_Sales_Management_System.DataModels
{
    public class CarService
    {
        private readonly IMongoCollection<Car> _cars;

        public CarService()
        {
            var client = new MongoClient("mongodb+srv://WilsonHuga:WilsonYoobee123@wilsonhugayoobee2025.nyfrb2o.mongodb.net/?retryWrites=true&w=majority&appName=WilsonHugaYoobee2025");
            var db = client.GetDatabase("carSales");
            _cars = db.GetCollection<Car>("cars");
        }

        public List<Car> GetAllCars()
        {
            try
            {
                return _cars.Find(car => true).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MongoDB error: " + ex.Message);
                return new List<Car>();
            }
        }

        public void AddCar(Car car) => _cars.InsertOne(car);

        public bool DeleteCar(string id)
        {
            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    Debug.WriteLine("Invalid ObjectId format: " + id);
                    return false;
                }
                var filter = Builders<Car>.Filter.Eq(c => c.Id, objectId);
                var result = _cars.DeleteOne(filter);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MongoDB error in DeleteCar: " + ex.Message);
                return false;
            }
        }

        public bool UpdateCar(string id, Car updatedCar)
        {
            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    Debug.WriteLine("Invalid ObjectId format: " + id);
                    return false;
                }

                var filter = Builders<Car>.Filter.Eq(c => c.Id, objectId);
                var update = Builders<Car>.Update
                    .Set(c => c.Make, updatedCar.Make)
                    .Set(c => c.Model, updatedCar.Model)
                    .Set(c => c.Year, updatedCar.Year)
                    .Set(c => c.Mileage, updatedCar.Mileage)
                    .Set(c => c.Color, updatedCar.Color)
                    .Set(c => c.Price, updatedCar.Price)
                    .Set(c => c.Condition, updatedCar.Condition)
                    .Set(c => c.Status, updatedCar.Status);
                    //.Set(c => c.Photos, updatedCar.Photos)
                // Note: VIN and ListedDate are not updated to preserve original values

                var result = _cars.UpdateOne(filter, update);
                Debug.WriteLine($"CarService.UpdateCar: ID {id} updated, ModifiedCount: {result.ModifiedCount}");
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MongoDB error in UpdateCar: " + ex.Message);
                return false;
            }
        }

        public Car GetCarById(string id)
        {
            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    Debug.WriteLine($"Invalid ObjectId format: {id}");
                    return null;
                }
                return _cars.Find(c => c.Id == objectId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MongoDB error in GetCarById: {ex.Message}");
                return null;
            }
        }

    }
}
