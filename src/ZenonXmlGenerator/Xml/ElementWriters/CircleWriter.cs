using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 원(Ellipse/Circle) 요소를 zenon XML로 출력한다. TYPE="103"
///
/// zenon 15 Strict XML 규격:
/// - 투명 원환 (Transformer 권선 등):
///   FillPattern="0", Transparent="TRUE", BackColor="0", AlphaBackColor="0", LineColorEx
/// - 단색 채움 (EarthSwitch 녹색 원 등):
///   FillPattern="1", Transparent="FALSE", BackColor/FillColor/FillColorEx, AlphaBackColor="255", LineColorEx
/// </summary>
public sealed class CircleWriter : IElementWriter
{
    /// <summary>zenon TYPE 코드: Ellipse = "103"</summary>
    public const string TypeEllipse = "103";

    public void Write(XmlWriter writer, TopologyElement element, int index)
    {
        var circle = (CircleElement)element;

        writer.WriteStartElement($"Elements_{index}");
        writer.WriteAttributeString("NODE", XmlConstants.NodeEmbeddedObject);
        writer.WriteAttributeString("TYPE", TypeEllipse);

        writer.WriteElementString("StartX",          circle.StartX.ToString());
        writer.WriteElementString("StartY",          circle.StartY.ToString());
        writer.WriteElementString("Width",           circle.Diameter.ToString());
        writer.WriteElementString("Height",          circle.Diameter.ToString());
        writer.WriteElementString("LineWidth",       circle.LineWidth.ToString());

        var cleanBorder = string.IsNullOrWhiteSpace(circle.BorderColor) ? "FFFFFF" : circle.BorderColor.TrimStart('#');
        writer.WriteElementString("ForeColor",       ColorConverter.ToColorRefString(circle.BorderColor));
        writer.WriteElementString("LineColorEx",     cleanBorder);

        bool isFilled = !string.IsNullOrWhiteSpace(circle.FillColor) && circle.FillColor != "transparent";

        if (isFilled)
        {
            var fillRef = ColorConverter.ToColorRefString(circle.FillColor!);
            var cleanFill = circle.FillColor!.TrimStart('#');

            writer.WriteElementString("FillPattern",     "1");
            writer.WriteElementString("Transparent",     "FALSE");
            writer.WriteElementString("BackColor",       fillRef);
            writer.WriteElementString("FillColor",       fillRef);
            writer.WriteElementString("FillColorEx",     cleanFill);
            writer.WriteElementString("AlphaBackColor",  "255");
        }
        else
        {
            // 완전 투명 (변압기 권선 등)
            writer.WriteElementString("FillPattern",     "0");
            writer.WriteElementString("Transparent",     "TRUE");
            writer.WriteElementString("BackColor",       "0");
            writer.WriteElementString("AlphaBackColor",  "0");
        }

        writer.WriteEndElement(); // Elements_n
    }
}
