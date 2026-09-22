using System.Text.Json.Serialization;

namespace ZenonXmlGenerator.Models;

/// <summary>
/// 선(Line) 요소. zenon TYPE="101"
/// 좌표 규칙: StartX=x1, StartY=y1, Width=(x2-x1), Height=(y2-y1)
/// </summary>
public sealed class LineElement : TopologyElement
{
    [JsonPropertyName("x1")] public int X1 { get; set; }
    [JsonPropertyName("y1")] public int Y1 { get; set; }
    [JsonPropertyName("x2")] public int X2 { get; set; }
    [JsonPropertyName("y2")] public int Y2 { get; set; }

    /// <summary>선 색상 (#RRGGBB). 기본 검정.</summary>
    [JsonPropertyName("color")]
    public string Color { get; set; } = "#000000";

    /// <summary>선 두께 (픽셀).</summary>
    [JsonPropertyName("lineWidth")]
    public int LineWidth { get; set; } = 1;

    // --- 파생 좌표 (Ground Truth 규칙) ---
    /// <summary>StartX = x1</summary>
    [JsonIgnore] public int StartX => X1;
    /// <summary>StartY = y1</summary>
    [JsonIgnore] public int StartY => Y1;
    /// <summary>Width = x2 - x1  (dx; 수평이면 0)</summary>
    [JsonIgnore] public int Dx => X2 - X1;
    /// <summary>Height = y2 - y1 (dy; 수직 상향이면 음수)</summary>
    [JsonIgnore] public int Dy => Y2 - Y1;
}
