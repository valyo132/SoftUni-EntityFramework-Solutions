using System.Xml.Serialization;

namespace ProductShop.DTOs.Export
{
    [XmlType("Product")]
    public class ExportCategoryDto
    {
        [XmlElement("name")]
        public string FullName { get; set; } = null!;

        [XmlElement("price")]
        public decimal Price { get; set; }

        [XmlElement("buyer")]

        public string BuyeerFullName { get; set; } = null!;
    }
}
