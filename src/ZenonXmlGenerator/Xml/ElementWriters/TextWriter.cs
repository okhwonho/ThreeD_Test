using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 정적 텍스트(Static Text) 요소를 zenon XML로 출력한다. TYPE="107"
/// 다크 테마 가독성을 위해 검은색 폰트는 백색(#FFFFFF)으로 보정하고,
/// BackColor=0, LineColor=FFFFFF, AlphaBackColor=0, Transparent=TRUE로 투명 배경을 보장한다.
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

        // 다크 테마 가독성 보장: 검은색(#000000) 또는 빈 색상은 백색(#FFFFFF)으로 강제 보정
        var effectiveColor = string.IsNullOrWhiteSpace(text.Color) || text.Color.TrimStart('#').Equals("000000", System.StringComparison.OrdinalIgnoreCase)
            ? XmlConstants.ColorTextPrimary // #FFFFFF
            : text.Color;

        var cleanColor = effectiveColor.TrimStart('#');
        writer.WriteElementString("ForeColor",     ColorConverter.ToColorRefString(effectiveColor));
        writer.WriteElementString("LineColorEx",   "FFFFFF");

        // ─── 정품 zenon standard.XML 투명 배경 구조 ─────────────────────
        writer.WriteElementString("BackColor",      "0");
        writer.WriteElementString("LineColor",      "FFFFFF");
        writer.WriteElementString("AlphaForeColor", "0");
        writer.WriteElementString("AlphaLineColor", "FFFFFF");
        writer.WriteElementString("AlphaBackColor", "0");
        writer.WriteElementString("Transparent",    "TRUE");

        writer.WriteEndElement(); // Elements_n
    }
}
