using System.Xml.Serialization;

[XmlRoot("request")]
public class EntryRequestDto
{
     [XmlElement("msisdn")]
     public string msisdn { get; set; } = default!;

     [XmlElement("network")]
     public int network { get; set; }

     [XmlElement("sessionid")]
     public string sessionid { get; set; } = default!;

     [XmlElement("msg")]
     public string msg { get; set; } = default!;

     public UssdMessageType type { get; set; }


}


public enum UssdMessageType
{
     [XmlEnum("1")]
     InitialRequest = 1,
     [XmlEnum("2")]
     ContinueSession = 2,
     [XmlEnum("3")]
     EndSession = 3,
}

