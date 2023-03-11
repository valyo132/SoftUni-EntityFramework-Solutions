using CarDealer.Data;
using CarDealer.DTOs;
using CarDealer.Models;
using Newtonsoft.Json;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main()
        {
            // TODO: Chage the connectionstring and remake the database

            CarDealerContext context = new CarDealerContext();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            string json = "";
            string result = "";

            // Problem 09
            json = File.ReadAllText(@"../../../Datasets/suppliers.json");
            ImportSuppliers(context, json);

            // Problem 10
            json = File.ReadAllText(@"../../../Datasets/parts.json");
            ImportParts(context, json);

            // Problem 11
            json = File.ReadAllText(@"../../../Datasets/cars.json");
            result = ImportCars(context, json);

            Console.WriteLine(result);
        }

        // Problem 11
        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            var deserializedCarDtos = JsonConvert.DeserializeObject<List<CarDto>>(inputJson);

            var cars = new List<Car>();
            var partCars = new List<PartCar>();

            foreach (var car in deserializedCarDtos)
            {
                var carToAdd = new Car()
                {
                    Make = car.Make,
                    Model = car.Model,
                    TravelledDistance = car.TravelledDistance
                };

                cars.Add(carToAdd);

                foreach (var part in car.PartsId)
                {
                    var partCar = new PartCar()
                    {
                        Car = carToAdd,
                        PartId = part
                    };

                    partCars.Add(partCar);
                }
            }

            context.Cars.AddRange(cars);
            context.PartsCars.AddRange(partCars);

            context.SaveChanges();

            return $"Successfully imported {deserializedCarDtos.Count}.";
        }

        // Problem 10
        public static string ImportParts(CarDealerContext context, string inputJson)
        {
            int[] supplierIds = context.Suppliers
                .Select(x => x.Id)
                .ToArray();

            var deserializedParts = JsonConvert.DeserializeObject<List<Part>>(inputJson)
                .Where(x => supplierIds.Contains(x.SupplierId))
                .ToList();
            context.Parts.AddRange(deserializedParts);
            context.SaveChanges();

            return $"Successfully imported {deserializedParts.Count}.";
        }

        // Problem 09
        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            var deserializedSuppliers = JsonConvert.DeserializeObject<List<Supplier>>(inputJson);
            context.Suppliers.AddRange(deserializedSuppliers);
            context.SaveChanges();

            return $"Successfully imported {deserializedSuppliers.Count}.";
        }
    }
}