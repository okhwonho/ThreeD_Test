using System.Text.Json.Serialization;

namespace ZenonXmlGenerator.Models;

/// <summary>
/// 사각형(Rectangle) 요소. zenon TYPE="102"
/// </summary>
public sealed class RectangleElement : TopologyElement
{
    [JsonPropertyName("x")]      public int X      { get; set; }
    [JsonPropertyName("y")]      public int Y      { get; set; }
    [JsonPropertyName("width")]  public int Width  { get; set; }
    [JsonPropertyName("height")] public int Height { get; set; }

    /// <summary>채우기 색상 (#RRGGBB 또는 16진수). 기본 null(투명).</summary>
    [JsonPropertyName("fillColor")]
    public string? FillColor { get; set; }

    /// <summary>테두리 색상 (#RRGGBB 또는 16진수). 기본 슬레이트 그레이(#5C6C75).</summary>
    [JsonPropertyName("borderColor")]
    public string BorderColor { get; set; } = "#5C6C75";

    /// <summary>선 두께 (기본 1).</summary>
    [JsonPropertyName("lineWidth")]
    public int LineWidth { get; set; } = 1;

    /// <summary>채우기 패턴 (0 = 채우기 없음/투명, 1 = 단색 채우기 등). 기본 0.</summary>
    [JsonPropertyName("fillPattern")]
    public int FillPattern { get; set; } = 0;

    /// <summary>배경 투명도/알파 (0 = 완전 투명, 255 = 완전 불투명). 기본 0.</summary>
    [JsonPropertyName("alphaBackColor")]
    public int AlphaBackColor { get; set; } = 0;
}
