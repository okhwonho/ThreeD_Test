using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 원(Ellipse/Circle) 요소를 zenon XML로 출력한다. TYPE="103"
///
/// Golden.XML 정밀 분석 결과 복제 구조:
/// - 단색 채움 원 (EarthSwitch 등):
///   FillPattern="6", BackColor="RRGGBB", LineColorEx="RRGGBB", AlphaBackColor="0", FillStyle/
/// - 투명 원환 (Transformer 등):
///   FillPattern="0", BackColor="000000", LineColorEx="RRGGBB", AlphaBackColor="0", FillStyle/
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
        writer.WriteElementString("LineType",        "0");

        var borderRgbHex = ColorConverter.ToRgbHexString(circle.BorderColor);
        writer.WriteElementString("LineColorEx",     borderRgbHex);
        writer.WriteElementString("AlphaLineColor",  "0");

        writer.WriteStartElement("FillStyle");
        writer.WriteEndElement(); // <FillStyle/>

        bool isFilled = !string.IsNullOrWhiteSpace(circle.FillColor) && circle.FillColor != "transparent";

        if (isFilled)
        {
            var fillRgbHex = ColorConverter.ToRgbHexString(circle.FillColor!);
            // Golden.XML 단색 채움 규격: FillPattern=6, BackColor=RRGGBB
            writer.WriteElementString("FillPattern",     "6");
            writer.WriteElementString("BackColor",       fillRgbHex);
            writer.WriteElementString("AlphaBackColor",  "0");
        }
        else
        {
            // 투명 (변압기 권선 등): FillPattern=0
            writer.WriteElementString("FillPattern",     "0");
            writer.WriteElementString("BackColor",       "000000");
            writer.WriteElementString("AlphaBackColor",  "0");
        }

        writer.WriteEndElement(); // Elements_n
    }
}
