using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 원(Ellipse/Circle) 요소를 zenon XML로 출력한다. TYPE="103"
///
/// zenon Ground Truth 구조:
/// <code>
/// &lt;Elements_n NODE="zenOn(R) embedded object" TYPE="103"&gt;
///   &lt;StartX&gt;cx - r&lt;/StartX&gt;
///   &lt;StartY&gt;cy - r&lt;/StartY&gt;
///   &lt;Width&gt;r * 2&lt;/Width&gt;
///   &lt;Height&gt;r * 2&lt;/Height&gt;
///   &lt;ForeColor&gt;{border COLORREF}&lt;/ForeColor&gt;
///   &lt;BackColor&gt;{fill COLORREF}&lt;/BackColor&gt;
///   &lt;LineWidth&gt;2&lt;/LineWidth&gt;
///   &lt;FillPattern&gt;8&lt;/FillPattern&gt;
///   &lt;AlphaBackColor&gt;0&lt;/AlphaBackColor&gt;
/// &lt;/Elements_n&gt;
/// </code>
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

        writer.WriteElementString("StartX",       circle.StartX.ToString());
        writer.WriteElementString("StartY",       circle.StartY.ToString());
        writer.WriteElementString("Width",        circle.Diameter.ToString());
        writer.WriteElementString("Height",       circle.Diameter.ToString());
        writer.WriteElementString("ForeColor",    ColorConverter.ToColorRefString(circle.BorderColor));
        writer.WriteElementString("BackColor",    ColorConverter.ToColorRefString(circle.FillColor));
        writer.WriteElementString("LineWidth",    circle.LineWidth.ToString());
        writer.WriteElementString("FillPattern",  circle.FillPattern.ToString());
        writer.WriteElementString("AlphaBackColor", "0");

        writer.WriteEndElement(); // Elements_n
    }
}
