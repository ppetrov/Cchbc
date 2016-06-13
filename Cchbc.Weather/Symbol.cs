using System.Xml.Serialization;

namespace Cchbc.Weather
{
    public sealed class Symbol
    {
        [XmlAttribute("number")]
        public int Number { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("var")]
        public string Var { get; set; }
    }
}
