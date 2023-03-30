using System.Xml.Serialization;

namespace SoftJail.DataProcessor.ExportDto
{
    [XmlType("Prisoner")]
    public class ExportPrisonerDto
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; } = null!;

        [XmlElement("IncarcerationDate")]
        public string IncarcerationDate { get; set; }

        [XmlArray("EncryptedMessages")]
        public ExportMessage[] EncryptedMessages { get; set; }
    }

    [XmlType("Message")]
    public class ExportMessage
    {
        [XmlElement("Description")]
        public string Description { get; set; }
    }
}
