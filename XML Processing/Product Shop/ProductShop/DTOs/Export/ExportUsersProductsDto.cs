using ProductShop.Models;
using System.Xml.Serialization;

namespace ProductShop.DTOs.Export
{
    [XmlType("User")]
    public class ExportUsersProductsDto
    {
        [XmlElement("firstName")]
        public string FirstName { get; set; } = null!;

        [XmlElement("lastName")]
        public string LastName { get; set; } = null!;

        [XmlArray("soldProducts")]
        public ProductDto[] ProductsSold { get; set; } = null!;
    }
}
