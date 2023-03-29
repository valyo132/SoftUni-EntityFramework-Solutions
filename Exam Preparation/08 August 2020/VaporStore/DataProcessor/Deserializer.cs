namespace VaporStore.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.Data.Models;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.ImportDto;

    public static class Deserializer
    {
        public const string ErrorMessage = "Invalid Data";

        public const string SuccessfullyImportedGame = "Added {0} ({1}) with {2} tags";

        public const string SuccessfullyImportedUser = "Imported {0} with {1} cards";

        public const string SuccessfullyImportedPurchase = "Imported {0} for {1}";

        public static string ImportGames(VaporStoreDbContext context, string jsonString)
        {
            var deserializedGamesDto = JsonConvert.DeserializeObject<ImportGameDto[]>(jsonString);

            StringBuilder sb = new StringBuilder();

            var gamesToAdd = new List<Game>();

            foreach (var gameDto in deserializedGamesDto)
            {
                if (!IsValid(gameDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (!gameDto.Tags.Any())
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime date;
                bool isDateValid = DateTime.TryParseExact(gameDto.ReleaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

                if (!isDateValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var dev = context.Developers
                    .FirstOrDefault(d => d.Name == gameDto.Developer);

                if (dev == null)
                {
                    dev = new Developer()
                    {
                        Name = gameDto.Developer
                    };

                    context.Developers.Add(dev);
                    context.SaveChanges();
                }

                Genre genre = context.Genres
                    .FirstOrDefault(d => d.Name == gameDto.Genre);

                if (genre == null)
                {
                    genre = new Genre()
                    {
                        Name = gameDto.Genre
                    };

                    context.Genres.Add(genre);
                    context.SaveChanges();
                }

                Game game = new Game()
                {
                    Name = gameDto.Name,
                    Price = gameDto.Price,
                    ReleaseDate = date,
                    Developer = dev,
                    Genre = genre
                };

                foreach (var tagDto in gameDto.Tags)
                {
                    Tag tag = context.Tags
                        .FirstOrDefault(t => t.Name == tagDto);

                    if (tag == null)
                    {
                        tag = new Tag()
                        {
                            Name = tagDto
                        };

                        context.Tags.Add(tag);
                        context.SaveChanges();
                    }

                    game.GameTags.Add(new GameTag()
                    {
                        Tag = tag
                    });
                }

                gamesToAdd.Add(game);
                sb.AppendLine(string.Format(SuccessfullyImportedGame, game.Name, game.Genre.Name, game.GameTags.Count()));
            }

            context.Games.AddRange(gamesToAdd);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportUsers(VaporStoreDbContext context, string jsonString)
        {
            var deserializedUsersDto = JsonConvert.DeserializeObject<ImportUserDto[]>(jsonString);

            StringBuilder sb = new StringBuilder();

            var usersToAdd = new List<User>();

            foreach (var userDto in deserializedUsersDto)
            {
                if (!IsValid(userDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                User user = new User()
                {
                    FullName = userDto.FullName,
                    Username = userDto.Username,
                    Email = userDto.Email,
                    Age = userDto.Age,
                };

                foreach (var cardDto in userDto.Cards)
                {
                    if (!IsValid(cardDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (!Enum.IsDefined(typeof(CardType), cardDto.Type))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Card card = new Card()
                    {
                        Number = cardDto.Number,
                        Type = Enum.Parse<CardType>(cardDto.Type),
                        Cvc = cardDto.Cvc,
                    };

                    user.Cards.Add(card);
                }

                usersToAdd.Add(user);
                sb.AppendLine(string.Format(SuccessfullyImportedUser, user.Username, user.Cards.Count()));
            }

            context.Users.AddRange(usersToAdd);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
        {
            var deserializedPurchesesDto = DeserializeObjects<ImportPurchaseDto[]>(xmlString, "Purchases");

            StringBuilder sb = new StringBuilder();

            var purchasesToAdd = new List<Purchase>();

            foreach (var purchaseDto in deserializedPurchesesDto)
            {
                if (!IsValid(purchaseDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime date;
                bool isDateValid = DateTime.TryParseExact(purchaseDto.Date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

                if (!isDateValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (!Enum.IsDefined(typeof(PurchaseType), purchaseDto.Type))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var card = context.Cards
                    .FirstOrDefault(c => c.Number == purchaseDto.Card);

                if (card == null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var game = context.Games
                    .FirstOrDefault(g => g.Name == purchaseDto.title);

                if (game == null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Purchase purchase = new Purchase()
                {
                    Type = Enum.Parse<PurchaseType>(purchaseDto.Type),
                    ProductKey = purchaseDto.Key,
                    Card = card,
                    Game = game,
                    Date = date
                };

                purchasesToAdd.Add(purchase);
                sb.AppendLine(string.Format(SuccessfullyImportedPurchase, purchase.Game.Name, purchase.Card.User.Username));
            }

            context.Purchases.AddRange(purchasesToAdd);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }

        public static T DeserializeObjects<T>(string inputXml, string rootName)
        {
            XmlRootAttribute root = new XmlRootAttribute(rootName);
            XmlSerializer serializer = new XmlSerializer(typeof(T), root);

            StringReader reader = new StringReader(inputXml);
            T dto = (T)serializer.Deserialize(reader);

            return dto;
        }
    }
}