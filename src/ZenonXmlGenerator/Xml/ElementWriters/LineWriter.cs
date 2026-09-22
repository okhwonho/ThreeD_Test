using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 선(Line) 요소를 zenon XML로 출력한다. TYPE="101"
/// Ground Truth 좌표 규칙:
///   StartX = x1, StartY = y1
///   Width  = x2 - x1  (Dx)
///   Height = y2 - y1  (Dy, 수직 상향선이면 음수)
/// </summary>
public sealed class LineWriter : IElementWriter
{
    public void Write(XmlWriter writer, TopologyElement element, int index)
    {
        var line = (LineElement)element;

        writer.WriteStartElement($"Elements_{index}");
        writer.WriteAttributeString("NODE", XmlConstants.NodeEmbeddedObject);
        writer.WriteAttributeString("TYPE", XmlConstants.TypeLine);

        writer.WriteElementString("StartX",    line.StartX.ToString());
        writer.WriteElementString("StartY",    line.StartY.ToString());
        writer.WriteElementString("Width",     line.Dx.ToString());
        writer.WriteElementString("Height",    line.Dy.ToString());
        writer.WriteElementString("ForeColor", ColorConverter.ToColorRefString(line.Color));
        writer.WriteElementString("LineWidth", line.LineWidth.ToString());

        if (!string.IsNullOrWhiteSpace(line.ALCUseColor))
        {
            writer.WriteElementString("ALCUseColor", line.ALCUseColor);
        }

        writer.WriteEndElement(); // Elements_n
    }
}
