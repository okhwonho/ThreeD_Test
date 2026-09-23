using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 사각형(Rectangle) 요소를 zenon XML로 출력한다. TYPE="102"
/// 구역/베이 박스의 경우 투명 배경(FillPattern=0, AlphaBackColor=0) 및 테두리(LineWidth=1, LineColorEx)를 지원한다.
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
        writer.WriteElementString("FillPattern",     rect.FillPattern.ToString());
        writer.WriteElementString("AlphaBackColor",  rect.AlphaBackColor.ToString());
        writer.WriteElementString("ForeColor",       ColorConverter.ToColorRefString(rect.BorderColor));

        // LineColorEx (zenon 16진수 색상: 5C6C75 등)
        var cleanBorder = rect.BorderColor.TrimStart('#');
        writer.WriteElementString("LineColorEx",     cleanBorder);

        if (!string.IsNullOrWhiteSpace(rect.FillColor))
        {
            writer.WriteElementString("FillColor",   ColorConverter.ToColorRefString(rect.FillColor));
            writer.WriteElementString("FillColorEx", rect.FillColor.TrimStart('#'));
        }

        writer.WriteEndElement(); // Elements_n
    }
}
