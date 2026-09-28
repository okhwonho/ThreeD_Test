using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 사각형(Rectangle) 요소를 zenon XML로 출력한다. TYPE="102"
///
/// zenon 15 Strict XML 규격:
/// - 투명 구역 (Bay Frame, 투명 박스):
///   FillPattern="0", Transparent="TRUE", BackColor="0", AlphaBackColor="0", LineColorEx={BGR Hex}
/// - 단색 채움 (CircuitBreaker, 계측 패널 등):
///   FillPattern="1", Transparent="FALSE", BackColor/FillColor={COLORREF}, FillColorEx={BGR Hex}, AlphaBackColor="255", LineColorEx={BGR Hex}
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

        var borderRef = ColorConverter.ToColorRefString(rect.BorderColor);
        var borderBgrHex = ColorConverter.ToBgrHexString(rect.BorderColor);
        writer.WriteElementString("ForeColor",       borderRef);
        writer.WriteElementString("LineColorEx",     borderBgrHex);

        bool isFilled = !string.IsNullOrWhiteSpace(rect.FillColor) && rect.FillColor != "transparent";

        if (isFilled)
        {
            var fillRef = ColorConverter.ToColorRefString(rect.FillColor!);
            var fillBgrHex = ColorConverter.ToBgrHexString(rect.FillColor!);

            writer.WriteElementString("FillPattern",     "1");
            writer.WriteElementString("Transparent",     "FALSE");
            writer.WriteElementString("BackColor",       fillRef);
            writer.WriteElementString("FillColor",       fillRef);
            writer.WriteElementString("FillColorEx",     fillBgrHex);
            writer.WriteElementString("AlphaBackColor",  "255");
        }
        else
        {
            // 완전 투명 (Bay Frame 등)
            writer.WriteElementString("FillPattern",     "0");
            writer.WriteElementString("Transparent",     "TRUE");
            writer.WriteElementString("BackColor",       "0");
            writer.WriteElementString("AlphaBackColor",  "0");
        }

        writer.WriteEndElement(); // Elements_n
    }
}
