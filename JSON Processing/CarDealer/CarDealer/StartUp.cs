using CarDealer.Data;
using CarDealer.DTOs;
using CarDealer.Models;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main()
        {
            CarDealerContext context = new CarDealerContext();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            string json = "";
            string result = "";

            //Problem 09
            json = File.ReadAllText(@"../../../Datasets/suppliers.json");
            ImportSuppliers(context, json);

            // Problem 10
            json = File.ReadAllText(@"../../../Datasets/parts.json");
            ImportParts(context, json);

            // Problem 11
            json = File.ReadAllText(@"../../../Datasets/cars.json");
            ImportCars(context, json);

            // Problem 12
            json = File.ReadAllText(@"../../../Datasets/customers.json");
            ImportCustomers(context, json);

            // Problem 13
            json = File.ReadAllText(@"../../../Datasets/sales.json");
            ImportSales(context, json);

            // Problem 14
            GetOrderedCustomers(context);

            // Problem 15
            GetCarsFromMakeToyota(context);

            // Problem 16
            GetLocalSuppliers(context);

            // Problem 17
            GetCarsWithTheirListOfParts(context);

            // Problem 18
            GetTotalSalesByCustomer(context);

            // Problem 19
            result = GetSalesWithAppliedDiscount(context);

            Console.WriteLine(result);
        }

        // Problem 19
        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                .OrderBy(s => s.Id)
                .Take(10)
                .Select(s => new
                 {
                     car = new
                     {
                         s.Car.Make,
                         s.Car.Model,
                         s.Car.TraveledDistance
                     },
                     customerName = s.Customer.Name,
                     discount = $"{s.Discount:f2}",
                     price = $"{s.Car.PartsCars.Sum(pc => pc.Part.Price):f2}",
                     priceWithDiscount = $"{s.Car.PartsCars.Sum(pc => pc.Part.Price) * (1 - s.Discount / 100):f2}"
                 }).ToArray();

            return JsonConvert.SerializeObject(sales, Formatting.Indented);
        }

        // Problem 18
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Customers
                .Where(c => c.Sales.Any())
                .Select(c => new
                {
                    fullName = c.Name,
                    boughtCars = c.Sales.Count,
                    sales = c.Sales.SelectMany(c => c.Car.PartsCars.Select(p => p.Part.Price))
                }).ToArray();

            var result = customers.Select(c => new
            {
                c.fullName,
                c.boughtCars,
                spentMoney = c.sales.Sum()
            }).OrderByDescending(c => c.spentMoney)
                .ThenByDescending(c => c.boughtCars)
                .ToArray();

            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        //// Problem 17
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context.Cars
                .Select(c => new
                {
                    car = new
                    {
                        Make = c.Make,
                        Model = c.Model,
                        TraveledDistance = c.TraveledDistance,
                        
                    },
                    parts = c.PartsCars
                            .Select(cp => new
                            {
                                Name = cp.Part.Name,
                                Price = $"{cp.Part.Price:f2}"
                            })
                }).ToArray();

            return JsonConvert.SerializeObject(cars, Formatting.Indented);
        }

        //// Problem 16
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .Where(s => !s.IsImporter)
                .Select(s => new
                {
                    Id = s.Id,
                    Name = s.Name,
                    PartsCount = s.Parts.Count
                }).ToArray();

            return JsonConvert.SerializeObject(suppliers, Formatting.Indented);
        }

        // Problem 15
        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var cars = context.Cars
                .Where(c => c.Make == "Toyota")
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TraveledDistance)
                .Select(c => new
                {
                    c.Id,
                    c.Make,
                    c.Model,
                    TraveledDistance = c.TraveledDistance
                }).ToArray();

            return JsonConvert.SerializeObject(cars, Formatting.Indented);
        }

        // Problem 14
        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var customers = context.Customers
                .OrderBy(c => c.BirthDate)
                .ThenBy(c => c.IsYoungDriver)
                .Select(c => new
                {
                    Name = c.Name,
                    BirthDate = c.BirthDate.ToString("dd/MM/yyyy"),
                    IsYoungDriver = c.IsYoungDriver
                })
                .ToArray();

            return JsonConvert.SerializeObject(customers, Formatting.Indented);
        }

        // Problem 13
        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            var deserializedSales = JsonConvert.DeserializeObject<List<Sale>>(inputJson);
            context.Sales.AddRange(deserializedSales);
            context.SaveChanges();

            return $"Successfully imported {deserializedSales.Count}.";
        }

        // Problem 12
        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            var deserializedCustomers = JsonConvert.DeserializeObject<List<Customer>>(inputJson);
            context.Customers.AddRange(deserializedCustomers);
            context.SaveChanges();

            return $"Successfully imported {deserializedCustomers.Count}.";
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
                    TraveledDistance = car.TravelledDistance
                };

                cars.Add(carToAdd);

                foreach (var part in car.PartsId.Distinct())
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