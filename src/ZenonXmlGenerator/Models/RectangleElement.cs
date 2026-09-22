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

    /// <summary>채우기 색상 (#RRGGBB). 기본 흰색.</summary>
    [JsonPropertyName("fillColor")]
    public string FillColor { get; set; } = "#FFFFFF";

    /// <summary>테두리 색상 (#RRGGBB). 기본 검정.</summary>
    [JsonPropertyName("borderColor")]
    public string BorderColor { get; set; } = "#000000";
}
