using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 정적 텍스트(Static Text) 요소를 zenon XML로 출력한다. TYPE="107"
/// Width/Height가 0이면 fontSize 기반 추정치를 사용한다.
/// 배경은 항상 투명(AlphaBackColor=0, FillStyle/Transparent=TRUE) 처리.
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

        // 텍스트 배경 투명화 (zenon Ground Truth)
        writer.WriteElementString("AlphaBackColor", "0");

        // FillStyle/Transparent = TRUE
        writer.WriteStartElement("FillStyle");
        writer.WriteElementString("Transparent", "TRUE");
        writer.WriteEndElement(); // FillStyle

        writer.WriteEndElement(); // Elements_n
    }
}
