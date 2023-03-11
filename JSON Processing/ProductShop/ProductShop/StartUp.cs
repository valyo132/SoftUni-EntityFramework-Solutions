using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main()
        {
            ProductShopContext context = new ProductShopContext();

            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            string json = "";
            string result = "";

            // Problem 01
            json = File.ReadAllText(@"../../../Datasets/users.json");
            ImportUsers(context, json);

            // Problem 02
            json = File.ReadAllText(@"../../../Datasets/products.json");
            ImportProducts(context, json);

            // Problem 03
            json = File.ReadAllText(@"../../../Datasets/categories.json");
            ImportCategories(context, json);

            // Problem 04
            json = File.ReadAllText(@"../../../Datasets/categories-products.json");
            ImportCategoryProducts(context, json);

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

        //Problem 08
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var usersWithProducts = context.Users
                .Where(u => u.ProductsSold.Any(cp => cp.Buyer != null))
                .OrderByDescending(u => u.ProductsSold
                    .Where(cp => cp.Buyer != null)
                    .Count())
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    age = u.Age,
                    soldProducts = new
                    {
                        count = u.ProductsSold
                             .Where(pc => pc.Buyer != null)
                             .Count(),
                        products = u.ProductsSold
                             .Where(p => p.Buyer != null)
                             .Select(cp => new
                             {
                                 name = cp.Name,
                                 price = cp.Price
                             })
                    }

                }).ToArray();

            var usersInfo = new
            {
                usersCount = usersWithProducts.Count(),
                users = usersWithProducts
            };

            string json = JsonConvert.SerializeObject(usersInfo, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            });

            return json;
        }

        // Peoblem 07
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .OrderByDescending(c => c.CategoriesProducts.Count)
                .Select(c => new
                {
                    category = c.Name,
                    productsCount = c.CategoriesProducts.Count,
                    averagePrice = Math.Round((double)c.CategoriesProducts.Average(cp => cp.Product.Price), 2),
                    totalRevenue = Math.Round((double)c.CategoriesProducts.Sum(cp => cp.Product.Price), 2)
                }).ToArray()
                .ToArray();

            return JsonConvert.SerializeObject(categories, Formatting.Indented);
        }

        // Problem 06
        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(u => u.ProductsSold.Any(ps => ps.Buyer != null))
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    soldProducts = u.ProductsSold
                        .Select(ps => new
                        {
                            name = ps.Name,
                            price = ps.Price,
                            buyerFirstName = ps.Buyer.FirstName,
                            buyerLastName = ps.Buyer.LastName
                        }).ToArray()

                }).ToArray();

            string json = JsonConvert.SerializeObject(users, Formatting.Indented);

            return json;
        }

        // Problem 05
        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .OrderBy(p => p.Price)
                .Select(p => new
                {
                    name = p.Name,
                    price = p.Price,
                    seller = $"{p.Seller.FirstName} {p.Seller.LastName}"
                })
                .ToArray();

            string jsonResult = JsonConvert.SerializeObject(products, Formatting.Indented);

            return jsonResult;
        }

        // Problem 04
        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            var deserializedProductCategories = JsonConvert.DeserializeObject<List<CategoryProduct>>(inputJson);
            context.CategoriesProducts.AddRange(deserializedProductCategories);
            context.SaveChanges();

            return $"Successfully imported {deserializedProductCategories.Count}";
        }

        // Problem 03
        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            var deserializedCategories = JsonConvert.DeserializeObject<List<Category>>(inputJson)
                .Where(c => c.Name != null)
                .ToList();
            context.Categories.AddRange(deserializedCategories);
            context.SaveChanges();

            return $"Successfully imported {deserializedCategories.Count}";
        }

        // Problem 02
        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            var deserializedProducts = JsonConvert.DeserializeObject<List<Product>>(inputJson);
            context.Products.AddRange(deserializedProducts);
            context.SaveChanges();
            return $"Successfully imported {deserializedProducts.Count}";
        }


        // Problem 01
        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            var deserializedUsers = JsonConvert.DeserializeObject<List<User>>(inputJson);
            context.Users.AddRange(deserializedUsers);
            context.SaveChanges();
            return $"Successfully imported {deserializedUsers.Count}";
        }
    }
}