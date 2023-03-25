using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Artillery.DataProcessor.ImportDto
{
    [XmlType("Manufacturer")]
    public class ImportManufacturerDto
    {
        [Required]
        [StringLength(40, MinimumLength = 4)]
        [XmlElement("ManufacturerName")]
        public string ManufacturerName { get; set; } = null!;

        [Required]
        [StringLength(100, MinimumLength = 10)]
        [XmlElement("Founded")]
        public string Founded { get; set; } = null!;
    }
}
