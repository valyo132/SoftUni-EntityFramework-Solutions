namespace Boardgames.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using System.Xml.Serialization;
    using Boardgames.Data;
    using Boardgames.Data.Models;
    using Boardgames.Data.Models.Enums;
    using Boardgames.DataProcessor.ImportDto;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedCreator
            = "Successfully imported creator – {0} {1} with {2} boardgames.";

        private const string SuccessfullyImportedSeller
            = "Successfully imported seller - {0} with {1} boardgames.";

        public static string ImportCreators(BoardgamesContext context, string xmlString)
        {
            var deserializedCreatorDto = DeserializeObjects<ImportCreatorDto[]>(xmlString, "Creators");

            StringBuilder sb = new StringBuilder();

            var creatorsToAdd = new List<Creator>();

            foreach (var creatorDto in deserializedCreatorDto)
            {
                if (!IsValid(creatorDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Creator creator = new Creator()
                {
                    FirstName = creatorDto.FirstName,
                    LastName = creatorDto.LastName
                };

                foreach (var boardgameDto in creatorDto.Boardgames)
                {
                    if (!IsValid(boardgameDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Boardgame boardgame = new Boardgame()
                    {
                        Name = boardgameDto.Name,
                        Rating = boardgameDto.Rating,
                        YearPublished = boardgameDto.YearPublished,
                        CategoryType = (CategoryType)Enum.Parse(typeof(CategoryType), boardgameDto.CategoryType.ToString()),
                        Mechanics = boardgameDto.Mechanics,
                    };

                    creator.Boardgames.Add(boardgame);
                }

                creatorsToAdd.Add(creator);
                sb.AppendLine(string.Format(SuccessfullyImportedCreator, creator.FirstName, creator.LastName, creator.Boardgames.Count));
            }

            context.Creators.AddRange(creatorsToAdd);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportSellers(BoardgamesContext context, string jsonString)
        {
            var deserializedSellerDto = JsonConvert.DeserializeObject<ImportSellerDto[]>(jsonString);

            StringBuilder sb = new StringBuilder();

            var sellersToAdd = new List<Seller>();

            foreach (var sellerDto in deserializedSellerDto)
            {
                if (!IsValid(sellerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Seller seller = new Seller()
                {
                    Name = sellerDto.Name,
                    Address = sellerDto.Address,
                    Website = sellerDto.Website,
                    Country = sellerDto.Country
                };

                foreach (var boardgameId in sellerDto.Boardgames.Distinct())
                {
                    var currentBoardgame = context.Boardgames.Find(boardgameId);

                    if (currentBoardgame == null)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    seller.BoardgamesSellers.Add(new BoardgameSeller()
                    {
                        BoardgameId = boardgameId
                    });
                }

                sellersToAdd.Add(seller);
                sb.AppendLine(string.Format(SuccessfullyImportedSeller, seller.Name, seller.BoardgamesSellers.Count));
            }

            context.Sellers.AddRange(sellersToAdd);
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
