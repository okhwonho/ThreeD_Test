using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 선(Line) 요소를 zenon XML로 출력한다. TYPE="101"
/// 다크 테마에서 검은색 선이 묻히지 않도록 검은색(#000000)인 경우 백색(#FFFFFF)으로 자동 보정하며,
/// zenon 15 필수 속성인 LineColorEx를 BGR 16진수(BBGGRR) 형식으로 출력한다.
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

        // 다크 테마 가독성 보장: 검은색(#000000) 또는 빈 색상은 백색(#FFFFFF)으로 강제 보정
        var effectiveColor = string.IsNullOrWhiteSpace(line.Color) || line.Color.TrimStart('#').Equals("000000", System.StringComparison.OrdinalIgnoreCase)
            ? XmlConstants.ColorTextPrimary // #FFFFFF
            : line.Color;

        writer.WriteElementString("ForeColor",   ColorConverter.ToColorRefString(effectiveColor));
        // zenon 15 BGR 16진수 포맷: BBGGRR (예: #E53935 -> 3539E5, #00E676 -> 76E600)
        writer.WriteElementString("LineColorEx", ColorConverter.ToBgrHexString(effectiveColor));
        writer.WriteElementString("LineWidth",   line.EffectiveLineWidth.ToString());

        if (!string.IsNullOrWhiteSpace(line.ALCUseColor))
        {
            writer.WriteElementString("ALCUseColor", line.ALCUseColor);
        }

        writer.WriteEndElement(); // Elements_n
    }
}
