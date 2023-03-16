using AutoMapper;
using ProductShop.Data;
using ProductShop.DTOs.Export;
using ProductShop.DTOs.Import;
using ProductShop.Models;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main()
        {
            ProductShopContext context = new ProductShopContext();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            string xml = "";
            string result = "";

            // Problem 01
            xml = File.ReadAllText("../../../Datasets/users.xml");
            ImportUsers(context, xml);

            // Problem 02
            xml = File.ReadAllText("../../../Datasets/products.xml");
            ImportProducts(context, xml);

            // Problem 03
            xml = File.ReadAllText("../../../Datasets/categories.xml");
            ImportCategories(context, xml);

            // Problem 04
            xml = File.ReadAllText("../../../Datasets/categories-products.xml");
            ImportCategoryProducts(context, xml);

            // Problem 05
            GetProductsInRange(context);

            // Problem 06
            GetSoldProducts(context);

            // Problem 07
            GetCategoriesByProductsCount(context);

            // Problem 08
            result = GetUsersWithProducts(context);

            Console.WriteLine(result);
        }

        // Problem 08
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context
                .Users
                .Where(u => u.ProductsSold.Any())
                .OrderByDescending(u => u.ProductsSold.Count)
                .Select(u => new ExportUsersWithProductDto()
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                    SoldProducts = new ExportUsersWithProductCountDto()
                    {
                        Count = u.ProductsSold.Count,
                        Products = u.ProductsSold.Select(p => new SoldProduct()
                        {
                            Name = p.Name,
                            Price = p.Price
                        })
                        .OrderByDescending(p => p.Price)
                        .ToArray()
                    }
                })
                .Take(10)
                .ToArray();

            ExportUserCountDto result = new ExportUserCountDto()
            {
                Count = context.Users.Count(u => u.ProductsSold.Any()),
                Users = users
            };

            return SerializaObjects<ExportUserCountDto>(result, "Users");
        }

        // Problem 07
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            ExportCategoryRevenueDto[] categories = context.Categories
                .Select(c => new ExportCategoryRevenueDto()
                {
                    Name = c.Name,
                    Count = c.CategoryProducts.Count,
                    AgeragePrice = c.CategoryProducts.Average(cp => cp.Product.Price),
                    TotalRevenue = c.CategoryProducts.Sum(cp => cp.Product.Price)
                })
                .OrderByDescending(c => c.Count)
                .ThenBy(c => c.TotalRevenue)
                .ToArray();

            return SerializaObjects<ExportCategoryRevenueDto[]>(categories, "Categories");
        }

        // Problem 06
        public static string GetSoldProducts(ProductShopContext context)
        {
            ExportUsersProductsDto[] users = context.Users
                .Where(u => u.ProductsSold.Count > 0)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Take(5)
                .Select(u => new ExportUsersProductsDto()
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    ProductsSold = u.ProductsSold.Select(p => new ProductDto()
                    {
                        Name = p.Name,
                        Price = p.Price,
                    }).ToArray(),
                }).ToArray();

            return SerializaObjects<ExportUsersProductsDto[]>(users, "Users");
        }

        // Problem 05
        public static string GetProductsInRange(ProductShopContext context)
        {
            ExportCategoryDto[] products = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .OrderBy(p => p.Price)
                .Take(10)
                .Select(p => new ExportCategoryDto()
                {
                    FullName = p.Name,
                    Price = p.Price,
                    BuyeerFullName = $"{p.Buyer.FirstName} {p.Buyer.LastName}"
                }).ToArray();

            string result = SerializaObjects<ExportCategoryDto[]>(products, "Products");
            return result;
        }

        // Problem 04
        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            var categoriesProductDtos = DeserializeObjects<ImportCateogiresProductsDto[]>(inputXml, "CategoryProducts");

            int[] categoriesIDs = context.Categories.Select(c => c.Id).ToArray();
            int[] productsIDs = context.Products.Select(c => c.Id).ToArray();

            var categoryProduct = categoriesProductDtos
                .Where(cp => categoriesIDs.Contains(cp.CategoryId) && productsIDs.Contains(cp.ProductId))
                .Select(cp => new CategoryProduct()
                {
                    ProductId = cp.ProductId,
                    CategoryId = cp.CategoryId,
                }).ToArray();

            context.CategoryProducts.AddRange(categoryProduct);
            context.SaveChanges();

            return $"Successfully imported {categoryProduct.Length}";
        }

        // Problem 03
        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            var categorieDtos = DeserializeObjects<ImportCategoryDto[]>(inputXml, "Categories");

            var categories = categorieDtos
                .Where(c => c.Name != null)
                .Select(c => new Category()
                {
                    Name = c.Name,
                }).ToArray();

            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Length}";
        }

        // Problem 02
        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            XmlRootAttribute root = new XmlRootAttribute("Products");
            XmlSerializer serializer = new XmlSerializer(typeof(ImportProductDto[]), root);

            StringReader reader = new StringReader(inputXml);
            var productDtos = (ImportProductDto[])serializer.Deserialize(reader);

            Product[] products = productDtos
                .Select(p => new Product()
                {
                    Name = p.Name,
                    Price = p.Price,
                    BuyerId = p.BuyerId,
                    SellerId = p.SellerId,
                }).ToArray();

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        // Problem 01
        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            XmlRootAttribute root = new XmlRootAttribute("Users");
            XmlSerializer serializer = new XmlSerializer(typeof(UserDto[]), root);

            StringReader reader = new StringReader(inputXml);
            UserDto[] userDtos = (UserDto[])serializer.Deserialize(reader);

            User[] users = userDtos
                .Select(u => new User()
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                }).ToArray();

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Length}";
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