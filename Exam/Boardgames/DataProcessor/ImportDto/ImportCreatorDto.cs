using Boardgames.Data.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Boardgames.DataProcessor.ImportDto
{
    [XmlType("Creator")]
    public class ImportCreatorDto
    {
        [Required]
        [StringLength(7, MinimumLength = 2)]
        [XmlElement("FirstName")]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(7, MinimumLength = 2)]
        [XmlElement("LastName")]
        public string LastName { get; set; } = null!;

        [XmlArray("Boardgames")]
        public ImportBoardgameDto[] Boardgames { get; set; }
    }

    [XmlType("Boardgame")]
    public class ImportBoardgameDto
    {
        [Required]
        [StringLength(20, MinimumLength = 10)]
        [XmlElement("Name")]
        public string Name { get; set; } = null!;

        [Required]
        [Range(1, 10.00)]
        [XmlElement("Rating")]
        public double Rating { get; set; }

        [Required]
        [Range(2018, 2023)]
        [XmlElement("YearPublished")]
        public int YearPublished { get; set; }

        [Required]
        [XmlElement("CategoryType")]
        public int CategoryType { get; set; }

        [Required]
        [XmlElement("Mechanics")]
        public string Mechanics { get; set; } = null!;
    }
}
