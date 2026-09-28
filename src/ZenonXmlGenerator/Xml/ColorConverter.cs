using System;

namespace ZenonXmlGenerator.Xml;

/// <summary>
/// HTML #RRGGBB 색상 코드를 zenon XML의 Windows COLORREF (BGR DWORD) 및 BGR 16진수로 변환.
/// COLORREF = 0x00BBGGRR (R이 최하위 바이트)
/// BGR HEX = BBGGRR (대문자 6자리)
/// </summary>
public static class ColorConverter
{
    /// <summary>
    /// "#RRGGBB" 형식의 HTML 색상 문자열을 zenon COLORREF 십진수 정수로 반환.
    /// </summary>
    /// <param name="htmlColor">"#RRGGBB" 또는 "RRGGBB" 형식.</param>
    /// <returns>COLORREF 십진수 정수 (0x00BBGGRR).</returns>
    /// <exception cref="FormatException">파싱 실패 시.</exception>
    public static int ToColorRef(string htmlColor)
    {
        if (string.IsNullOrWhiteSpace(htmlColor))
            return 0; // 기본 검정

        var (r, g, b) = ParseRgb(htmlColor);

        // Windows COLORREF: 0x00BBGGRR
        return (b << 16) | (g << 8) | r;
    }

    /// <summary>HTML 색상 → COLORREF 십진수 문자열 (XML 직접 삽입용).</summary>
    public static string ToColorRefString(string htmlColor) =>
        ToColorRef(htmlColor).ToString();

    /// <summary>
    /// HTML #RRGGBB 색상 문자열을 zenon 15의 Ex 속성용 BGR 16진수 문자열("BBGGRR")로 변환.
    /// 예: #00C853 -> 53C800, #E53935 -> 3539E5, #00E676 -> 76E600, #07101C -> 1C1007
    /// </summary>
    public static string ToBgrHexString(string htmlColor)
    {
        if (string.IsNullOrWhiteSpace(htmlColor))
            return "000000";

        var (r, g, b) = ParseRgb(htmlColor);
        return $"{b:X2}{g:X2}{r:X2}";
    }

    private static (int r, int g, int b) ParseRgb(string htmlColor)
    {
        var hex = htmlColor.TrimStart('#');
        if (hex.Length != 6)
            throw new FormatException($"지원하지 않는 색상 형식: '{htmlColor}'. '#RRGGBB' 형식이어야 합니다.");

        var r = Convert.ToInt32(hex.Substring(0, 2), 16);
        var g = Convert.ToInt32(hex.Substring(2, 2), 16);
        var b = Convert.ToInt32(hex.Substring(4, 2), 16);

        return (r, g, b);
    }
}
