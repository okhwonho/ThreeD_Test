using System.Text.Json.Serialization;

namespace ZenonXmlGenerator.Models;

/// <summary>
/// 원(Ellipse/Circle) 요소. zenon TYPE="103"
/// VectorSymbolRenderer가 심볼(EarthSwitch, Transformer 등)을 벡터로 분해할 때 생성.
/// JSON 직렬화 대상이 아닌 내부 렌더링 전용 타입.
/// </summary>
public sealed class CircleElement : TopologyElement
{
    /// <summary>원 중심 X 좌표.</summary>
    public int CenterX { get; set; }

    /// <summary>원 중심 Y 좌표.</summary>
    public int CenterY { get; set; }

    /// <summary>반지름 (px).</summary>
    public int Radius { get; set; }

    /// <summary>채움 색상 (#RRGGBB). null이면 투명 원환.</summary>
    public string? FillColor { get; set; }

    /// <summary>테두리 색상 (#RRGGBB).</summary>
    public string BorderColor { get; set; } = Xml.XmlConstants.ColorSymbolBorder;

    /// <summary>선 굵기.</summary>
    public int LineWidth { get; set; } = 2;

    /// <summary>
    /// 채우기 패턴 (8 = 단색 채움, 0 = 빈 원).
    /// </summary>
    public int FillPattern { get; set; } = Xml.XmlConstants.FillPatternSolid;

    // ─── 파생 좌표 ─────────────────────────────────────────────────────
    [JsonIgnore] public int StartX   => CenterX - Radius;
    [JsonIgnore] public int StartY   => CenterY - Radius;
    [JsonIgnore] public int Diameter => Radius * 2;
}
