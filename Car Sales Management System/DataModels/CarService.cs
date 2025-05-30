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


    }


    //    public void UpdateCar(string id, Car updatedCar) =>  
    //        _cars.ReplaceOne(car => car.Id == new MongoDB.Bson.ObjectId(id), updatedCar);  

    //    public void DeleteCar(string id) =>  
    //        _cars.DeleteOne(car => car.Id == new MongoDB.Bson.ObjectId(id));  
}
