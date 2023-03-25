namespace Artillery.DataProcessor
{
    using Artillery.Data;
    using Artillery.Data.Models;
    using Artillery.Data.Models.Enums;
    using Artillery.DataProcessor.ImportDto;
    using Microsoft.VisualBasic;
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {
        private const string ErrorMessage =
            "Invalid data.";
        private const string SuccessfulImportCountry =
            "Successfully import {0} with {1} army personnel.";
        private const string SuccessfulImportManufacturer =
            "Successfully import manufacturer {0} founded in {1}.";
        private const string SuccessfulImportShell =
            "Successfully import shell caliber #{0} weight {1} kg.";
        private const string SuccessfulImportGun =
            "Successfully import gun {0} with a total weight of {1} kg. and barrel length of {2} m.";

        public static string ImportCountries(ArtilleryContext context, string xmlString)
        {
            var deserializedCountires = DeserializeObjects<ImportCountiresDto[]>(xmlString, "Countries");

            StringBuilder sb = new StringBuilder();

            var countiresToAdd = new List<Country>();

            foreach (var countryDto in deserializedCountires)
            {
                if (!IsValid(countryDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Country country = new Country()
                {
                    CountryName = countryDto.CountryName,
                    ArmySize = countryDto.ArmySize,
                };

                countiresToAdd.Add(country);
                sb.AppendLine(string.Format(SuccessfulImportCountry, country.CountryName, country.ArmySize));
            }

            context.Countries.AddRange(countiresToAdd);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportManufacturers(ArtilleryContext context, string xmlString)
        {
            var deserializedManufacturers = DeserializeObjects<ImportManufacturerDto[]>(xmlString, "Manufacturers");

            StringBuilder sb = new StringBuilder();

            var manufacturersToAdd = new List<Manufacturer>();

            foreach (var manufacturerDto in deserializedManufacturers)
            {
                if (!IsValid(manufacturerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var existingManufacturers = manufacturersToAdd
                    .Select(m => m.ManufacturerName)
                    .ToList();

                if (existingManufacturers.Contains(manufacturerDto.ManufacturerName))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Manufacturer manufacturer = new Manufacturer()
                {
                    ManufacturerName = manufacturerDto.ManufacturerName,
                    Founded = manufacturerDto.Founded,
                };

                string[] tokens = manufacturerDto.Founded.Split(", ");
                string founded = $"{tokens[tokens.Length - 2]}, {tokens[tokens.Length - 1]}";

                manufacturersToAdd.Add(manufacturer);
                sb.AppendLine(string.Format(SuccessfulImportManufacturer, manufacturer.ManufacturerName, founded));
            }

            context.Manufacturers.AddRange(manufacturersToAdd); 
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportShells(ArtilleryContext context, string xmlString)
        {
            var deserializedShellDtos = DeserializeObjects<ImportShellDto[]>(xmlString, "Shells");

            var sb = new StringBuilder();

            var shellsToAdd = new List<Shell>();

            foreach (var shellDto in deserializedShellDtos)
            {
                if (!IsValid(shellDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Shell shell = new Shell()
                {
                    ShellWeight = shellDto.ShellWeight,
                    Caliber = shellDto.Caliber,
                };

                shellsToAdd.Add(shell);
                sb.AppendLine(string.Format(SuccessfulImportShell, shell.Caliber, shell.ShellWeight));
            }

            context.Shells.AddRange(shellsToAdd);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportGuns(ArtilleryContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            var deserializedGuns = JsonConvert.DeserializeObject<ImportGunDto[]>(jsonString);

            int gunCount = 0;
            int countryCount = 0;

            var gunsToAdd = new List<Gun>();

            foreach (var gunDto in deserializedGuns)
            {
                if (!IsValid(gunDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (!Enum.IsDefined(typeof(GunType), gunDto.GunType))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Gun gun = new Gun()
                {
                    ManufacturerId = gunDto.ManufacturerId,
                    GunWeight = gunDto.GunWeight,
                    BarrelLength = gunDto.BarrelLength,
                    NumberBuild = gunDto.NumberBuild,
                    Range = gunDto.Range,
                    GunType = Enum.Parse<GunType>(gunDto.GunType),
                    ShellId = gunDto.ShellId
                };

                foreach (var countryId in gunDto.Countries)
                {
                    var country = context.Countries.Find(countryId.Id);

                    gun.CountriesGuns.Add(new CountryGun()
                    {
                        CountryId = countryId.Id
                    });
                }

                gunsToAdd.Add(gun);
                sb.AppendLine(string.Format(SuccessfulImportGun, gun.GunType, gun.GunWeight, gun.BarrelLength));
            }

            context.Guns.AddRange(gunsToAdd);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }
        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var result = Validator.TryValidateObject(obj, validator, validationRes, true);
            return result;
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