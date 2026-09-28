using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 원(Ellipse/Circle) 요소를 zenon XML로 출력한다. TYPE="103"
/// 2D 플랫 SCADA 규격을 준수하여 3D/그라데이션 효과를 비활성화하고,
/// FillPattern=0 (투명) / FillPattern=1 (단색 채움)을 정확히 처리한다.
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
        writer.WriteElementString("FillPattern",     circle.FillPattern.ToString());

        var cleanBorder = circle.BorderColor.TrimStart('#');
        writer.WriteElementString("ForeColor",       ColorConverter.ToColorRefString(circle.BorderColor));
        writer.WriteElementString("LineColorEx",     cleanBorder);

        if (!string.IsNullOrWhiteSpace(circle.FillColor))
        {
            var fillRef = ColorConverter.ToColorRefString(circle.FillColor);
            writer.WriteElementString("BackColor",   fillRef);
            writer.WriteElementString("FillColor",   fillRef);
            writer.WriteElementString("FillColorEx", circle.FillColor.TrimStart('#'));
        }

        if (circle.FillPattern == XmlConstants.FillPatternHollow)
        {
            // 완전 투명 (변압기 권선 등)
            writer.WriteElementString("AlphaBackColor", "0");
            writer.WriteElementString("Transparent",    "TRUE");
        }
        else
        {
            // 단색 채움 (접지기 녹색 원 등)
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
