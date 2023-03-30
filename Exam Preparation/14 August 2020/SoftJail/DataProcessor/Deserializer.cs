namespace SoftJail.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Text;
    using System.Xml.Serialization;
    using Castle.Core.Internal;
    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.Data.Models.Enums;
    using SoftJail.DataProcessor.ImportDto;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid Data";

        private const string SuccessfullyImportedDepartment = "Imported {0} with {1} cells";

        private const string SuccessfullyImportedPrisoner = "Imported {0} {1} years old";

        private const string SuccessfullyImportedOfficer = "Imported {0} ({1} prisoners)";

        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var deserializedDepartmentCellDto = JsonConvert.DeserializeObject<ImportDepartmentCellDto[]>(jsonString);

            StringBuilder sb = new StringBuilder();

            var departmentsToAdd = new List<Department>();
            var cellsToAdd = new List<Cell>();

            foreach (var departmentDto in deserializedDepartmentCellDto)
            {
                bool invalid = false;

                if (!IsValid(departmentDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (!departmentDto.Cells.Any())
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Department departament = new Department()
                {
                    Name = departmentDto.Name,
                };

                foreach (var cellDto in departmentDto.Cells)
                {
                    if (!IsValid(cellDto))
                    {
                        invalid = true;
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Cell cell = new Cell()
                    {
                        CellNumber = cellDto.CellNumber,
                        HasWindow = cellDto.HasWindow,
                    };

                    departament.Cells.Add(cell);
                }

                if (!invalid)
                {
                    departmentsToAdd.Add(departament);
                    sb.AppendLine(string.Format(SuccessfullyImportedDepartment, departament.Name, departament.Cells.Count));
                }
            }

            context.Departments.AddRange(departmentsToAdd);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var deserializedPrisonerMailDto = JsonConvert.DeserializeObject<ImportPrisonerMailDto[]>(jsonString);

            StringBuilder sb = new StringBuilder();

            var prisonersToAdd = new List<Prisoner>();

            foreach (var prisonerDto in deserializedPrisonerMailDto)
            {
                bool isValid = false;

                if (!IsValid(prisonerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime inDate;
                bool isInDateValid = DateTime.TryParseExact(prisonerDto.IncarcerationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out inDate);

                if (!isInDateValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }


                Prisoner prisoner = new Prisoner()
                {
                    FullName = prisonerDto.FullName,
                    Nickname = prisonerDto.Nickname,
                    Age = prisonerDto.Age,
                    IncarcerationDate = inDate,
                    Bail = prisonerDto.Bail,
                    CellId = prisonerDto.CellId,
                };

                if (prisonerDto.ReleaseDate != null)
                {
                    DateTime releaseDate;
                    bool isReleaseDateValid = DateTime.TryParseExact(prisonerDto.ReleaseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out releaseDate);

                    if (!isReleaseDateValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    prisoner.ReleaseDate = releaseDate;
                }

                foreach (var mailDto in prisonerDto.Mails)
                {
                    if (!IsValid(mailDto))
                    {
                        isValid = true;
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Mail mail = new Mail()
                    {
                        Description = mailDto.Description,
                        Address = mailDto.Address,
                        Sender = mailDto.Sender,
                    };

                    prisoner.Mails.Add(mail);
                }

                if (isValid == false)
                {
                    prisonersToAdd.Add(prisoner);
                    sb.AppendLine(string.Format(SuccessfullyImportedPrisoner, prisoner.FullName, prisoner.Age));
                }

            }

            context.Prisoners.AddRange(prisonersToAdd);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var deserializedOfficerDto = DeserializeObjects<ImportOfficerPrisonerDto[]>(xmlString, "Officers");

            StringBuilder sb = new StringBuilder();

            var officersToAdd = new List<Officer>();

            foreach (var officerDto in deserializedOfficerDto)
            {
                if (!IsValid(officerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (!Enum.IsDefined(typeof(Position), officerDto.Position))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (!Enum.IsDefined(typeof(Weapon), officerDto.Weapon))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Officer officer = new Officer()
                {
                    FullName = officerDto.FullName,
                    Salary = officerDto.Salary,
                    Position = Enum.Parse<Position>(officerDto.Position),
                    Weapon = Enum.Parse<Weapon>(officerDto.Weapon),
                    DepartmentId = officerDto.DepartmentId,
                };

                foreach (var prissonerDto in officerDto.Prisoners)
                {
                    officer.OfficerPrisoners.Add(new OfficerPrisoner()
                    {
                        PrisonerId = prissonerDto.Id,
                        OfficerId = officer.Id,
                    });
                }

                officersToAdd.Add(officer);
                sb.AppendLine(string.Format(SuccessfullyImportedOfficer, officer.FullName, officer.OfficerPrisoners.Count));
            }

            context.Officers.AddRange(officersToAdd);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
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