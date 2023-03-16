using System.Xml.Serialization;

namespace CarDealer.DTOs.Import
{
    [XmlType("Car")]
    public class ImportCarDto
    {
        [XmlElement("make")]
        public string Make { get; set; } = null!;

        [XmlElement("model")]
        public string Model { get; set; } = null!;

        [XmlElement("traveledDistance")]
        public long TraveledDistance { get; set; }

        [XmlArray("parts")]
        public ImportCarPartDto[] ImportCarPartDto { get; set; }
    }

    [XmlType("partId")]
    public class ImportCarPartDto
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
    }
}
