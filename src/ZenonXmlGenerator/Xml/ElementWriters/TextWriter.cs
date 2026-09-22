using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 정적 텍스트(Static Text) 요소를 zenon XML로 출력한다. TYPE="107"
/// Width/Height가 0이면 fontSize 기반 추정치를 사용한다.
/// </summary>
public sealed class TextWriter : IElementWriter
{
    public void Write(XmlWriter writer, TopologyElement element, int index)
    {
        var text = (TextElement)element;

        writer.WriteStartElement($"Elements_{index}");
        writer.WriteAttributeString("NODE", XmlConstants.NodeEmbeddedObject);
        writer.WriteAttributeString("TYPE", XmlConstants.TypeText);

        writer.WriteElementString("StartX",    text.X.ToString());
        writer.WriteElementString("StartY",    text.Y.ToString());
        writer.WriteElementString("Width",     text.EffectiveWidth.ToString());
        writer.WriteElementString("Height",    text.EffectiveHeight.ToString());
        writer.WriteElementString("Text",      text.Text);
        writer.WriteElementString("FontSize",  text.FontSize.ToString());
        writer.WriteElementString("ForeColor", ColorConverter.ToColorRefString(text.Color));

        writer.WriteEndElement(); // GrafEle_n
    }
}
