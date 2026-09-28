using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 원(Ellipse/Circle) 요소를 zenon XML로 출력한다. TYPE="103"
///
/// zenon 15 Ground Truth 구조:
/// - 투명 원환 (Transformer 권선 등):
///   AlphaBackColor="0", Transparent="TRUE", BackColor="0"
/// - 단색 채움 (EarthSwitch 녹색 원 등):
///   AlphaBackColor="255", Transparent="FALSE", BackColor/FillColor/FillColorEx 설정
/// - FillPattern 태그는 해치(줄무늬) 브러시를 유발하므로 단색/투명 제어 시 생략.
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

        var cleanBorder = circle.BorderColor.TrimStart('#');
        writer.WriteElementString("ForeColor",       ColorConverter.ToColorRefString(circle.BorderColor));
        writer.WriteElementString("LineColorEx",     cleanBorder);

        bool isFilled = !string.IsNullOrWhiteSpace(circle.FillColor) && circle.FillColor != "transparent";

        if (isFilled)
        {
            var fillRef = ColorConverter.ToColorRefString(circle.FillColor!);
            writer.WriteElementString("BackColor",       fillRef);
            writer.WriteElementString("FillColor",       fillRef);
            writer.WriteElementString("FillColorEx",     circle.FillColor!.TrimStart('#'));
            writer.WriteElementString("AlphaBackColor",  "255");
            writer.WriteElementString("Transparent",     "FALSE");
        }
        else
        {
            // 완전 투명 (변압기 권선 등)
            writer.WriteElementString("BackColor",       "0");
            writer.WriteElementString("AlphaBackColor",  "0");
            writer.WriteElementString("Transparent",     "TRUE");
        }

        writer.WriteEndElement(); // Elements_n
    }
}
