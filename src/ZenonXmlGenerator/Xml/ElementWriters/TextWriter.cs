using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 정적 텍스트(Static Text) 요소를 zenon XML로 출력한다. TYPE="107"
///
/// Golden.XML 정밀 분석 결과 복제 구조:
/// <code>
/// &lt;Elements_n NODE="zenOn(R) embedded object" TYPE="107"&gt;
///   &lt;StartX&gt;…&lt;/StartX&gt;
///   &lt;StartY&gt;…&lt;/StartY&gt;
///   &lt;Width&gt;…&lt;/Width&gt;
///   &lt;Height&gt;…&lt;/Height&gt;
///   &lt;Text&gt;…&lt;/Text&gt;
///   &lt;TextStyle/&gt;
///   &lt;LinkedFont&gt;Default font5&lt;/LinkedFont&gt;
///   &lt;FontSize&gt;…&lt;/FontSize&gt;
///   &lt;TextColor&gt;FFFFFF&lt;/TextColor&gt;
///   &lt;HorizontalAlign&gt;8&lt;/HorizontalAlign&gt;
///   &lt;VerticalAlign&gt;0&lt;/VerticalAlign&gt;
///   &lt;Wordbreak&gt;TRUE&lt;/Wordbreak&gt;
///   &lt;FillStyle/&gt;
///   &lt;Transparent&gt;TRUE&lt;/Transparent&gt;
///   &lt;AlphaBackColor&gt;0&lt;/AlphaBackColor&gt;
///   &lt;BackColor&gt;0&lt;/BackColor&gt;
/// &lt;/Elements_n&gt;
/// </code>
/// </summary>
public sealed class TextWriter : IElementWriter
{
    public void Write(XmlWriter writer, TopologyElement element, int index)
    {
        var text = (TextElement)element;

        writer.WriteStartElement($"Elements_{index}");
        writer.WriteAttributeString("NODE", XmlConstants.NodeEmbeddedObject);
        writer.WriteAttributeString("TYPE", XmlConstants.TypeText);

        writer.WriteElementString("StartX",          text.X.ToString());
        writer.WriteElementString("StartY",          text.Y.ToString());
        writer.WriteElementString("Width",           text.EffectiveWidth.ToString());
        writer.WriteElementString("Height",          text.EffectiveHeight.ToString());
        writer.WriteElementString("Text",            text.Text);

        writer.WriteStartElement("TextStyle");
        writer.WriteEndElement(); // <TextStyle/>

        writer.WriteElementString("LinkedFont",      "Default font5");
        writer.WriteElementString("FontSize",        text.FontSize.ToString());

        // 텍스트 글자 색상: Golden.XML 규격은 <TextColor>RRGGBB</TextColor> (대문자 6자리 RGB 16진수)
        var effectiveColor = string.IsNullOrWhiteSpace(text.Color) || text.Color.TrimStart('#').Equals("000000", System.StringComparison.OrdinalIgnoreCase)
            ? "FFFFFF"
            : ColorConverter.ToRgbHexString(text.Color);

        writer.WriteElementString("TextColor",       effectiveColor);
        writer.WriteElementString("HorizontalAlign", "8");
        writer.WriteElementString("VerticalAlign",   "0");
        writer.WriteElementString("Wordbreak",       "TRUE");

        writer.WriteStartElement("FillStyle");
        writer.WriteEndElement(); // <FillStyle/>

        // 배경은 완전 투명: Transparent=TRUE, AlphaBackColor=0, BackColor=0
        writer.WriteElementString("Transparent",    "TRUE");
        writer.WriteElementString("AlphaBackColor", "0");
        writer.WriteElementString("BackColor",      "0");

        writer.WriteEndElement(); // Elements_n
    }
}
