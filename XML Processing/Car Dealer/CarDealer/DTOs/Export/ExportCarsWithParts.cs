using System.Xml.Serialization;

namespace CarDealer.DTOs.Export
{
    [XmlType("car")]
    public class ExportCarsWithParts
    {
        [XmlAttribute("make")]
        public string Make { get; set; }

        [XmlAttribute("model")]
        public string Model { get; set; }

        [XmlAttribute("traveled-distance")]
        public long TraveledDistance { get; set; }

        [XmlArray("parts")]
        public CarPart[] CarParts { get; set; }
    }

    [XmlType("part")]
    public class CarPart
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("price")]
        public decimal Price { get; set; }
    }
}
