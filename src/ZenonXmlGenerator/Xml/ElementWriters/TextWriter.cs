using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 정적 텍스트(Static Text) 요소를 zenon XML로 출력한다. TYPE="107"
///
/// 다크 테마 가시성 보장:
/// - AlphaForeColor = "255" (글자 완전 불투명 — 0으로 설정 시 글자가 투명해져 블랙아웃 발생)
/// - ForeColor = 백색(#FFFFFF) COLORREF(16777215)
/// - BackColor = 0, AlphaBackColor = 0, Transparent = TRUE (텍스트 배경은 완전 투명)
/// - LineColor = FFFFFF, AlphaLineColor = FFFFFF (텍스트 박스 외곽선 비표시)
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

        writer.WriteElementString("ForeColor",     ColorConverter.ToColorRefString(effectiveColor));
        writer.WriteElementString("LineColorEx",   "FFFFFF");

        // ─── 정품 zenon standard.XML 텍스트 구조 ─────────────────────────
        writer.WriteElementString("BackColor",      "0");
        writer.WriteElementString("LineColor",      "FFFFFF");
        // 핵심 수정: AlphaForeColor는 글자의 불투명도(255 = 100% 선명 표시, 0 = 완전 투명 블랙아웃)
        writer.WriteElementString("AlphaForeColor", "255");
        writer.WriteElementString("AlphaLineColor", "FFFFFF");
        // 배경은 투명 유지
        writer.WriteElementString("AlphaBackColor", "0");
        writer.WriteElementString("Transparent",    "TRUE");

        writer.WriteEndElement(); // Elements_n
    }
}
