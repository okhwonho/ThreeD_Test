using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 사각형(Rectangle) 요소를 zenon XML로 출력한다. TYPE="102"
///
/// Golden.XML 정밀 분석 결과 복제 구조:
/// - 단색 채움 사각형 (Solid Fill):
///   FillPattern="6", BackColor="RRGGBB", LineColorEx="RRGGBB", AlphaBackColor="0", FillStyle/
/// - 투명 구역 사각형 (Bay Frame):
///   FillPattern="0", BackColor="000000", LineColorEx="RRGGBB", AlphaBackColor="0", FillStyle/
/// </summary>
public sealed class RectangleWriter : IElementWriter
{
    public void Write(XmlWriter writer, TopologyElement element, int index)
    {
        var rect = (RectangleElement)element;

        writer.WriteStartElement($"Elements_{index}");
        writer.WriteAttributeString("NODE", XmlConstants.NodeEmbeddedObject);
        writer.WriteAttributeString("TYPE", XmlConstants.TypeRectangle);

        writer.WriteElementString("StartX",          rect.X.ToString());
        writer.WriteElementString("StartY",          rect.Y.ToString());
        writer.WriteElementString("Width",           rect.Width.ToString());
        writer.WriteElementString("Height",          rect.Height.ToString());
        writer.WriteElementString("LineWidth",       rect.LineWidth.ToString());
        writer.WriteElementString("LineType",        "0");

        var borderRgbHex = ColorConverter.ToRgbHexString(rect.BorderColor);
        writer.WriteElementString("LineColorEx",     borderRgbHex);
        writer.WriteElementString("AlphaLineColor",  "0");

        writer.WriteStartElement("FillStyle");
        writer.WriteEndElement(); // <FillStyle/>

        bool isFilled = !string.IsNullOrWhiteSpace(rect.FillColor) && rect.FillColor != "transparent";

        if (isFilled)
        {
            var fillRgbHex = ColorConverter.ToRgbHexString(rect.FillColor!);
            // Golden.XML 단색 채움 규격: FillPattern=6, BackColor=RRGGBB
            writer.WriteElementString("FillPattern",     "6");
            writer.WriteElementString("BackColor",       fillRgbHex);
            writer.WriteElementString("AlphaBackColor",  "0");
        }
        else
        {
            // 투명 (Bay Frame 등): FillPattern=0
            writer.WriteElementString("FillPattern",     "0");
            writer.WriteElementString("BackColor",       "000000");
            writer.WriteElementString("AlphaBackColor",  "0");
        }

        writer.WriteEndElement(); // Elements_n
    }
}
