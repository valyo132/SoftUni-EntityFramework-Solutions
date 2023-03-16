using CarDealer.Data;
using CarDealer.DTOs.Export;
using CarDealer.DTOs.Import;
using CarDealer.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Xml.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main()
        {
            CarDealerContext context = new CarDealerContext();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            string xml = "";
            string result = "";

            // Problem 09
            xml = File.ReadAllText("../../../Datasets/suppliers.xml");
            ImportSuppliers(context, xml);

            // Problem 10
            xml = File.ReadAllText("../../../Datasets/parts.xml");
            ImportParts(context, xml);

            // Problem 11
            xml = File.ReadAllText("../../../Datasets/cars.xml");
            ImportCars(context, xml);

            // Problem 12
            xml = File.ReadAllText("../../../Datasets/customers.xml");
            ImportCustomers(context, xml);

            // Problem 13
            xml = File.ReadAllText("../../../Datasets/sales.xml");
            ImportSales(context, xml);

            // Problem 14
            GetCarsWithDistance(context);

            // Problem 15
            GetCarsFromMakeBmw(context);

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
                .Select(s => new ExportSalesWithDiscountDto()
                {
                    Car = new CarDto()
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TraveledDistance = s.Car.TraveledDistance
                    },
                    Discount = (int)s.Discount,
                    CustomerName = s.Customer.Name,
                    Price = s.Car.PartsCars.Sum(p => p.Part.Price),
                    PriceWithDiscount = Math.Round((double)(s.Car.PartsCars.Sum(p => p.Part.Price) * (1 - (s.Discount / 100))), 4)
                }).ToArray();

            return SerializaObjects<ExportSalesWithDiscountDto[]>(sales, "sales");
        }

        // Problem 18
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Customers
                .Where(c => c.Sales.Any())
                .Select(c => new
                {
                    FullName = c.Name,
                    BoughtCars = c.Sales.Count,
                    Sales = c.Sales.Select(s => new
                    {
                        Prices = c.IsYoungDriver
                          ? s.Car.PartsCars.Sum(p => Math.Round((double)p.Part.Price * 0.95, 2))
                          : s.Car.PartsCars.Sum(p => (double)p.Part.Price)
                    }).ToArray(),
                }).ToArray();

            var result = customers
                .OrderByDescending(c => c.Sales.Sum(s => s.Prices))
                .Select(c => new ExportSalesByCustomer()
                {
                    FullName = c.FullName,
                    BoughtCars = c.BoughtCars,
                    SpentMoney = c.Sales.Sum(s => s.Prices).ToString("F2")
                })
                .ToArray();

            return SerializaObjects<ExportSalesByCustomer[]>(result, "customers");
        }

        // Problem 17
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var asd = context.Cars;

            var cars = context.Cars
                .OrderByDescending(c => c.TraveledDistance)
                .ThenBy(c => c.Model)
                .Take(5)
                .Select(c => new ExportCarsWithParts()
                {
                    Make = c.Make,
                    Model = c.Model,
                    TraveledDistance = c.TraveledDistance,
                    CarParts = c.PartsCars.OrderByDescending(p => p.Part.Price).Select(p => new CarPart()
                    {
                        Name = p.Part.Name,
                        Price = p.Part.Price,
                    })
                    .ToArray()
                })
                .ToArray();

            return SerializaObjects<ExportCarsWithParts[]>(cars, "cars");
        }

        // Problem 16
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .Where(s => s.IsImporter == false)
                .Select(s => new ExportNotImporterSuppliersDto()
                {
                    Id = s.Id,
                    Name = s.Name,
                    PartsCount = s.Parts.Count(),
                }).ToArray();

            return SerializaObjects<ExportNotImporterSuppliersDto[]>(suppliers, "suppliers");
        }

        // Problem 15
        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            var cars = context.Cars
                .Where(c => c.Make == "BMW")
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TraveledDistance)
                .Select(c => new ExportBMWCarDto()
                {
                    Id = c.Id,
                    Model = c.Model,
                    TraveledDistance = c.TraveledDistance
                }).ToArray();

            return SerializaObjects<ExportBMWCarDto[]>(cars, "cars");
        }

        // Problem 14
        public static string GetCarsWithDistance(CarDealerContext context)
        {
            ExportCarsWithDistanceDto[] cars = context.Cars
                .Where(c => c.TraveledDistance > 2_000_000)
                .OrderBy(c => c.Make)
                .ThenBy(c => c.Model)
                .Take(10)
                .Select(c => new ExportCarsWithDistanceDto()
                {
                    Make = c.Make,
                    Model = c.Model,
                    TraveledDistance = c.TraveledDistance,
                }).ToArray();

            return SerializaObjects<ExportCarsWithDistanceDto[]>(cars, "cars");
        }

        // Problem 13
        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            var saledDtos = DeserializeObjects<ImportSalesDto[]>(inputXml, "Sales");

            int[] carIds = context.Cars.Select(c => c.Id).ToArray();

            Sale[] sales = saledDtos
                .Where(s => carIds.Contains(s.CarId))
                .Select(s => new Sale()
                {
                    CarId = s.CarId,
                    CustomerId = s.CustomerId,
                    Discount = s.Discount
                }).ToArray();

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count()}";
        }


        // Problem 12
        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            var customersDtos = DeserializeObjects<ImportCustomerDro[]>(inputXml, "Customers");

            Customer[] customers = customersDtos
                .Select(c => new Customer()
                {
                    Name = c.Name,
                    BirthDate = c.BirthDate,
                    IsYoungDriver = c.IsYoungDriver,
                }).ToArray();

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count()}";
        }

        // Problem 11
        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            var carsDtos = DeserializeObjects<ImportCarDto[]>(inputXml, "Cars");

            int[] parts = context.Parts.Select(cp => cp.Id).ToArray();

            ICollection<Car> cars = new List<Car>();
            ICollection<PartCar> carParts = new List<PartCar>();

            foreach (var carDto in carsDtos)
            {
                var car = new Car()
                {
                    Make = carDto.Make,
                    Model = carDto.Model,
                    TraveledDistance = carDto.TraveledDistance
                };

                cars.Add(car);

                foreach (var carPartDto in carDto.ImportCarPartDto.Where(p => parts.Contains(p.Id)).Select(p => p.Id).Distinct())
                {
                    var carPart = new PartCar()
                    {
                        Car = car,
                        PartId = carPartDto
                    };

                    carParts.Add(carPart);
                }
            }

            context.Cars.AddRange(cars);
            context.PartsCars.AddRange(carParts);
            context.SaveChanges();

            return $"Successfully imported {cars.Count()}";
        }

        // Problem 10
        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            int[] suppliersIDs = context.Suppliers.Select(s => s.Id).ToArray();

            var partsDtos = DeserializeObjects<ImportPartDto[]>(inputXml, "Parts");

            Part[] parts = partsDtos
                .Where(p => suppliersIDs.Contains(p.SupplierId))
                .Select(p => new Part()
                {
                    Name = p.Name,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    SupplierId = p.SupplierId
                }).ToArray();

            context.Parts.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Count()}";
        }

        // Problem 09
        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            var dtos = DeserializeObjects<ImportSupplierDto[]>(inputXml, "Suppliers");

            Supplier[] suppliers = dtos
                .Select(d => new Supplier()
                {
                    Name = d.Name,
                    IsImporter = d.IsImporter
                }).ToArray();

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Count()}";
        }
        public static T DeserializeObjects<T>(string inputXml, string rootName)
        {
            XmlRootAttribute root = new XmlRootAttribute(rootName);
            XmlSerializer serializer = new XmlSerializer(typeof(T), root);

            StringReader reader = new StringReader(inputXml);
            T dto = (T)serializer.Deserialize(reader);

            return dto;
        }

        public static string SerializaObjects<T>(T collection, string rootName)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute root = new XmlRootAttribute(rootName);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            XmlSerializer serializer = new XmlSerializer(typeof(T), root);
            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, collection, namespaces);
            }

            return sb.ToString().TrimEnd();
        }
    }
}