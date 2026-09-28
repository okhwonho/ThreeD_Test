using System;

namespace ZenonXmlGenerator.Xml;

/// <summary>
/// HTML #RRGGBB 색상 코드를 zenon 15 XML 규격에 맞는 형식으로 변환.
/// Golden.XML 정밀 분석 결과:
/// zenon 15의 모든 16진수 색상 태그(TextColor, LineColorEx, BackColor 등)는
/// BGR이 아닌 순수 대문자 6자리 RGB 16진수("RRGGBB")이다.
/// </summary>
public static class ColorConverter
{
    /// <summary>
    /// HTML #RRGGBB 색상 문자열을 zenon 15 정품 XML용 RGB 16진수 문자열("RRGGBB")로 변환.
    /// 예: #00C853 -> 00C853, #E53935 -> E53935, #00E676 -> 00E676, #07101C -> 07101C
    /// </summary>
    public static string ToRgbHexString(string htmlColor)
    {
        if (string.IsNullOrWhiteSpace(htmlColor))
            return "FFFFFF";

        var hex = htmlColor.TrimStart('#');
        if (hex.Length != 6)
            throw new FormatException($"지원하지 않는 색상 형식: '{htmlColor}'. '#RRGGBB' 형식이어야 합니다.");

        return hex.ToUpperInvariant();
    }

    /// <summary>
    /// "#RRGGBB" 형식의 HTML 색상 문자열을 Windows COLORREF 십진수 정수(0x00BBGGRR)로 반환.
    /// </summary>
    public static int ToColorRef(string htmlColor)
    {
        if (string.IsNullOrWhiteSpace(htmlColor))
            return 0;

        var hex = htmlColor.TrimStart('#');
        if (hex.Length != 6)
            throw new FormatException($"지원하지 않는 색상 형식: '{htmlColor}'. '#RRGGBB' 형식이어야 합니다.");

        var r = Convert.ToInt32(hex.Substring(0, 2), 16);
        var g = Convert.ToInt32(hex.Substring(2, 2), 16);
        var b = Convert.ToInt32(hex.Substring(4, 2), 16);

        return (b << 16) | (g << 8) | r;
    }

    /// <summary>HTML 색상 → COLORREF 십진수 문자열</summary>
    public static string ToColorRefString(string htmlColor) =>
        ToColorRef(htmlColor).ToString();

    /// <summary>레거시 호환용 BGR 16진수 문자열 변환</summary>
    public static string ToBgrHexString(string htmlColor)
    {
        if (string.IsNullOrWhiteSpace(htmlColor))
            return "000000";

        var hex = htmlColor.TrimStart('#');
        if (hex.Length != 6)
            return "000000";

        var r = hex.Substring(0, 2);
        var g = hex.Substring(2, 2);
        var b = hex.Substring(4, 2);
        return $"{b}{g}{r}".ToUpperInvariant();
    }
}
