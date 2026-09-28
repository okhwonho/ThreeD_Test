using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 정적 텍스트(Static Text) 요소를 zenon XML로 출력한다. TYPE="107"
///
/// zenon 15 standard.XML Ground Truth 구조:
/// <code>
/// &lt;Elements_n NODE="zenOn(R) embedded object" TYPE="107"&gt;
///   &lt;StartX&gt;…&lt;/StartX&gt;
///   &lt;StartY&gt;…&lt;/StartY&gt;
///   &lt;Width&gt;…&lt;/Width&gt;
///   &lt;Height&gt;…&lt;/Height&gt;
///   &lt;Text&gt;…&lt;/Text&gt;
///   &lt;FontSize&gt;…&lt;/FontSize&gt;
///   &lt;ForeColor&gt;…&lt;/ForeColor&gt;
///   &lt;BackColor&gt;0&lt;/BackColor&gt;
///   &lt;LineColor&gt;FFFFFF&lt;/LineColor&gt;
///   &lt;AlphaForeColor&gt;0&lt;/AlphaForeColor&gt;
///   &lt;AlphaLineColor&gt;FFFFFF&lt;/AlphaLineColor&gt;
///   &lt;AlphaBackColor&gt;0&lt;/AlphaBackColor&gt;
///   &lt;Transparent&gt;TRUE&lt;/Transparent&gt;
/// &lt;/Elements_n&gt;
/// </code>
///
/// BackColor=0, LineColor=FFFFFF, AlphaBackColor=0, Transparent=TRUE 조합으로
/// 텍스트 박스가 불투명 회색으로 채워지는 현상을 제거한다.
/// </summary>
public sealed class TextWriter : IElementWriter
{
    public void Write(XmlWriter writer, TopologyElement element, int index)
    {
        var text = (TextElement)element;

        writer.WriteStartElement($"Elements_{index}");
        writer.WriteAttributeString("NODE", XmlConstants.NodeEmbeddedObject);
        writer.WriteAttributeString("TYPE", XmlConstants.TypeText);

        writer.WriteElementString("StartX",        text.X.ToString());
        writer.WriteElementString("StartY",        text.Y.ToString());
        writer.WriteElementString("Width",         text.EffectiveWidth.ToString());
        writer.WriteElementString("Height",        text.EffectiveHeight.ToString());
        writer.WriteElementString("Text",          text.Text);
        writer.WriteElementString("FontSize",      text.FontSize.ToString());
        writer.WriteElementString("ForeColor",     ColorConverter.ToColorRefString(text.Color));

        // ─── 정품 zenon standard.XML 투명 배경 구조 ─────────────────────
        // BackColor = 0 (DWORD 검정 계열, 실질적으로 투명처리)
        writer.WriteElementString("BackColor",      "0");
        // LineColor = FFFFFF (흰색 = 테두리 비표시)
        writer.WriteElementString("LineColor",      "FFFFFF");
        // AlphaForeColor = 0 (전경 alpha: 0 = 완전 적용)
        writer.WriteElementString("AlphaForeColor", "0");
        // AlphaLineColor = FFFFFF (라인 알파 흰색 = 비표시)
        writer.WriteElementString("AlphaLineColor", "FFFFFF");
        // AlphaBackColor = 0 (배경 완전 투명)
        writer.WriteElementString("AlphaBackColor", "0");
        // Transparent = TRUE (배경 투명 플래그)
        writer.WriteElementString("Transparent",    "TRUE");

        writer.WriteEndElement(); // Elements_n
    }
}
