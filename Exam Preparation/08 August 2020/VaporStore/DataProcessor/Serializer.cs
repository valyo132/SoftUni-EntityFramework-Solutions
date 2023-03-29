namespace VaporStore.DataProcessor
{
    using Data;
    using Newtonsoft.Json;
    using System.Globalization;
    using System.Text;
    using System.Xml.Serialization;
    using VaporStore.Data.Models;
    using VaporStore.DataProcessor.ExportDto;

    public static class Serializer
    {
        public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
        {
            var games = context.Genres
                .Where(g => genreNames.Contains(g.Name))
                .ToArray()
                .Select(g => new
                {
                    Id = g.Id,
                    Genre = g.Name,
                    Games = g.Games
                        .Where(g => g.Purchases.Any())
                        .Select(ga => new
                        {
                            Id = ga.Id,
                            Title = ga.Name,
                            Developer = ga.Developer.Name,
                            Tags = string.Join(", ", ga.GameTags.Select(ga => ga.Tag.Name)),
                            Players = ga.Purchases.Count()
                        })
                        .OrderByDescending(g => g.Players)
                        .ThenBy(g => g.Id)
                        .ToArray(),
                    TotalPlayers = g.Games.Sum(g => g.Purchases.Count())
                })
                .OrderByDescending(g => g.TotalPlayers)
                .ThenBy(g => g.Id)
                .ToArray();

            return JsonConvert.SerializeObject(games, Formatting.Indented);
        }

        public static string ExportUserPurchasesByType(VaporStoreDbContext context, string purchaseType)
        {
            var users = context.Users
                .AsEnumerable()
                .Where(u => u.Cards.Any(c => c.Purchases.Any(p => p.Type.ToString() == purchaseType)))
                .ToArray()
                .Select(u => new ExportUserDto()
                {
                    TotalSpent = u.Cards
                        .Sum(c => c.Purchases
                        .Where(p => p.Type.ToString() == purchaseType)
                        .Sum(p => p.Game.Price)),
                    Username = u.Username,
                    Purchases = u.Cards
                    .SelectMany(c => c.Purchases.Where(p => p.Type.ToString() == purchaseType))
                    .Select(c => new ExportUserPurchasesDto()
                    {
                        Card = c.Card.Number,
                        Cvc = c.Card.Cvc,
                        Date = c.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                        Game = new ExportGameDto()
                        {
                            Title = c.Game.Name,
                            Genre = c.Game.Genre.Name,
                            Price = c.Game.Price
                        },
                    })
                    .OrderBy(c => c.Date)
                    .ToArray()
                })
                .OrderByDescending(u => u.TotalSpent)
                .ThenBy(u => u.Username)
                .ToArray();

            return SerializaObjects<ExportUserDto[]>(users, "Users");
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