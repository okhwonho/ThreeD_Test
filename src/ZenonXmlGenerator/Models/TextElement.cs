using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace ZenonXmlGenerator.Models;

/// <summary>
/// 정적 텍스트(Static Text) 요소. zenon TYPE="107"
/// Width/Height를 JSON에서 명시하지 않으면 fontSize/텍스트 길이 기반으로 추정한다.
/// </summary>
public sealed class TextElement : TopologyElement
{
    [JsonPropertyName("x")] public int X { get; set; }
    [JsonPropertyName("y")] public int Y { get; set; }

    /// <summary>텍스트 박스 너비 (px). 0이면 텍스트 길이 기반으로 추정 (최소 80px).</summary>
    [JsonPropertyName("width")]
    public int Width { get; set; }

    /// <summary>텍스트 박스 높이 (px). 0이면 줄 수/폰트 기반으로 추정 (기본 28px/줄).</summary>
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
    public int EffectiveWidth
    {
        get
        {
            int maxLen = string.IsNullOrEmpty(Text) ? 1 : Text.Split('\n').Max(l => l.Length);
            int calcWidth = Math.Max(80, maxLen * 12);
            return Width > 0 ? Math.Max(Width, calcWidth) : calcWidth;
        }
    }

    [JsonIgnore]
    public int EffectiveHeight
    {
        get
        {
            int lines = string.IsNullOrEmpty(Text) ? 1 : Math.Max(1, Text.Split('\n').Length);
            int singleLine = 28;
            if (FontSize > 14) singleLine = Math.Max(singleLine, FontSize + 12);
            return Height > 0 ? Math.Max(Height, singleLine * lines) : singleLine * lines;
        }
    }
}
