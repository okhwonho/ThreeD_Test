using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 사각형(Rectangle) 요소를 zenon XML로 출력한다. TYPE="102"
/// </summary>
public sealed class RectangleWriter : IElementWriter
{
    public void Write(XmlWriter writer, TopologyElement element, int index)
    {
        var rect = (RectangleElement)element;

        writer.WriteStartElement($"Elements_{index}");
        writer.WriteAttributeString("NODE", XmlConstants.NodeEmbeddedObject);
        writer.WriteAttributeString("TYPE", XmlConstants.TypeRectangle);

        writer.WriteElementString("StartX",    rect.X.ToString());
        writer.WriteElementString("StartY",    rect.Y.ToString());
        writer.WriteElementString("Width",     rect.Width.ToString());
        writer.WriteElementString("Height",    rect.Height.ToString());
        writer.WriteElementString("ForeColor", ColorConverter.ToColorRefString(rect.BorderColor));
        writer.WriteElementString("FillColor", ColorConverter.ToColorRefString(rect.FillColor));

        writer.WriteEndElement(); // GrafEle_n
    }
}
