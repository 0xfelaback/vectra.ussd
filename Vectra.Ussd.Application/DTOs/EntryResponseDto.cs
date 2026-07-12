using System.Xml.Serialization;

[XmlRoot("response")]
public class EntryResponseDto
{
    public EntryResponseDto() { }
    public EntryResponseDto(string msg, UssdMessageType type)
    {
        this.msg = msg;
        this.type = type;
    }

    [XmlElement("msg")]
    public string msg { get; set; } = default!;

    [XmlElement("type")]
    public UssdMessageType type { get; set; }
}
