using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 사각형(Rectangle) 요소를 zenon XML로 출력한다. TYPE="102"
///
/// zenon 15 Ground Truth 구조:
/// - 투명 구역 (Bay Frame, 투명 박스):
///   AlphaBackColor="0", Transparent="TRUE", BackColor="0"
/// - 단색 채움 (CircuitBreaker, 계측 패널 등):
///   AlphaBackColor="255", Transparent="FALSE", BackColor/FillColor/FillColorEx 설정
/// - FillPattern 태그는 해치(줄무늬) 브러시를 유발하므로 단색/투명 제어 시 생략.
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

        var cleanBorder = rect.BorderColor.TrimStart('#');
        writer.WriteElementString("ForeColor",       ColorConverter.ToColorRefString(rect.BorderColor));
        writer.WriteElementString("LineColorEx",     cleanBorder);

        bool isFilled = !string.IsNullOrWhiteSpace(rect.FillColor) && rect.FillColor != "transparent";

        if (isFilled)
        {
            var fillRef = ColorConverter.ToColorRefString(rect.FillColor!);
            writer.WriteElementString("BackColor",       fillRef);
            writer.WriteElementString("FillColor",       fillRef);
            writer.WriteElementString("FillColorEx",     rect.FillColor!.TrimStart('#'));
            writer.WriteElementString("AlphaBackColor",  "255");
            writer.WriteElementString("Transparent",     "FALSE");
        }
        else
        {
            // 완전 투명 (Bay Frame 등)
            writer.WriteElementString("BackColor",       "0");
            writer.WriteElementString("AlphaBackColor",  "0");
            writer.WriteElementString("Transparent",     "TRUE");
        }

        writer.WriteEndElement(); // Elements_n
    }
}
