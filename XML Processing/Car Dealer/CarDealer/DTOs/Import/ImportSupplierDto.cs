using System.Xml.Serialization;

namespace CarDealer.DTOs.Import
{
    [XmlType("Supplier")]
    public class ImportSupplierDto
    {
        [XmlElement("name")]
        public string Name { get; set; } = null!;

        [XmlElement("IsImporter")]
        public bool IsImporter { get; set; }
    }
}
