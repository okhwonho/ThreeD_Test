using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 사각형(Rectangle) 요소를 zenon XML로 출력한다. TYPE="102"
/// 2D 플랫 SCADA 규격을 준수하여 3D/그라데이션 효과를 비활성화하고,
/// FillPattern=0 (투명) / FillPattern=1 (단색 채움)을 정확히 처리한다.
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

        var cleanBorder = rect.BorderColor.TrimStart('#');
        writer.WriteElementString("ForeColor",       ColorConverter.ToColorRefString(rect.BorderColor));
        writer.WriteElementString("LineColorEx",     cleanBorder);

        if (!string.IsNullOrWhiteSpace(rect.FillColor))
        {
            var fillRef = ColorConverter.ToColorRefString(rect.FillColor);
            writer.WriteElementString("BackColor",   fillRef);
            writer.WriteElementString("FillColor",   fillRef);
            writer.WriteElementString("FillColorEx", rect.FillColor.TrimStart('#'));
        }

        if (rect.FillPattern == XmlConstants.FillPatternHollow)
        {
            // 완전 투명 (테두리만 있는 Bay 구역 박스 등)
            writer.WriteElementString("AlphaBackColor", "0");
            writer.WriteElementString("Transparent",    "TRUE");
        }
        else
        {
            // 단색 채움 (Solid Fill - 차단기, 계측 카드 배경 등)
            writer.WriteElementString("AlphaBackColor", "100");
            writer.WriteElementString("Transparent",    "FALSE");
            writer.WriteElementString("Lightning",      "0");
            writer.WriteElementString("LightIntensity", "0");
            writer.WriteElementString("GradientDirection", "0");
            writer.WriteElementString("Brightness",     "FALSE");
        }

        writer.WriteEndElement(); // Elements_n
    }
}
