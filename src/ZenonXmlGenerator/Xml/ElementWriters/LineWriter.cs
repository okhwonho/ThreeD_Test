using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 선(Line) 요소를 zenon XML로 출력한다. TYPE="101"
///
/// Golden.XML 정밀 분석 결과 복제:
/// LineColorEx는 BGR이 아닌 순수 RGB 16진수(RRGGBB) 형식이다.
/// (예: AC 345kV 적색 모선 = E53935, DC 형광녹색 = 00E676, 백색 = FFFFFF)
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

        var effectiveColor = string.IsNullOrWhiteSpace(line.Color) || line.Color.TrimStart('#').Equals("000000", System.StringComparison.OrdinalIgnoreCase)
            ? XmlConstants.ColorTextPrimary // #FFFFFF
            : line.Color;

        // zenon 15 순수 RGB 16진수 포맷: RRGGBB (적색 모선 = E53935, DC = 00E676)
        writer.WriteElementString("LineColorEx", ColorConverter.ToRgbHexString(effectiveColor));
        writer.WriteElementString("LineWidth",   line.EffectiveLineWidth.ToString());

        if (!string.IsNullOrWhiteSpace(line.ALCUseColor))
        {
            writer.WriteElementString("ALCUseColor", line.ALCUseColor);
        }

        writer.WriteEndElement(); // Elements_n
    }
}
