using System.Text.Json.Serialization;

namespace ZenonXmlGenerator.Models;

/// <summary>
/// 정적 텍스트(Static Text) 요소. zenon TYPE="107"
/// Width/Height를 JSON에서 명시하지 않으면 fontSize 기반으로 추정한다.
/// </summary>
public sealed class TextElement : TopologyElement
{
    [JsonPropertyName("x")] public int X { get; set; }
    [JsonPropertyName("y")] public int Y { get; set; }

    /// <summary>텍스트 박스 너비 (px). 0이면 fontSize × 문자수 × 0.6 으로 추정.</summary>
    [JsonPropertyName("width")]
    public int Width { get; set; }

    /// <summary>텍스트 박스 높이 (px). 0이면 fontSize × 1.5 으로 추정.</summary>
    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("fontSize")]
    public int FontSize { get; set; } = 12;

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#000000";

    // --- 추정 치수 ---
    [JsonIgnore]
    public int EffectiveWidth  => Width  > 0 ? Width  : (int)(FontSize * (Text.Length + 1) * 0.6);
    [JsonIgnore]
    public int EffectiveHeight => Height > 0 ? Height : (int)(FontSize * 1.5);
}
