using System.Xml.Serialization;

namespace ProductShop.DTOs.Export
{
    [XmlType("Users")]
    public class ExportUserCountDto
    {
        [XmlElement("count")]
        public int Count { get; set; }

        [XmlArray("users")]
        public ExportUsersWithProductDto[] Users { get; set; }
    }

    [XmlType("User")]
    public class ExportUsersWithProductDto
    {
        [XmlElement("firstName")]
        public string FirstName { get; set; } = null!;

        [XmlElement("lastName")]
        public string LastName { get; set; } = null!;

        [XmlElement("age")]
        public int? Age { get; set; }

        public ExportUsersWithProductCountDto SoldProducts { get; set; }
    }

    [XmlType("SoldProducts")]
    public class ExportUsersWithProductCountDto
    {
        [XmlElement("count")]
        public int Count { get; set; }

        [XmlArray("products")]
        public SoldProduct[] Products { get; set; }
    }

    [XmlType("Product")]
    public class SoldProduct
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("price")]
        public decimal Price { get; set; }
    }
}
