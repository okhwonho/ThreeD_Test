using System;

namespace ZenonXmlGenerator.Xml;

/// <summary>
/// HTML #RRGGBB 색상 코드를 zenon XML의 Windows COLORREF (BGR DWORD) 십진수로 변환.
/// COLORREF = 0x00BBGGRR (R이 최하위 바이트)
/// </summary>
public static class ColorConverter
{
    /// <summary>
    /// "#RRGGBB" 형식의 HTML 색상 문자열을 zenon COLORREF 십진수로 반환.
    /// </summary>
    /// <param name="htmlColor">"#RRGGBB" 또는 "RRGGBB" 형식.</param>
    /// <returns>COLORREF 십진수 문자열 (XML 삽입용).</returns>
    /// <exception cref="FormatException">파싱 실패 시.</exception>
    public static int ToColorRef(string htmlColor)
    {
        if (string.IsNullOrWhiteSpace(htmlColor))
            return 0; // 기본 검정

        var hex = htmlColor.TrimStart('#');
        if (hex.Length != 6)
            throw new FormatException($"지원하지 않는 색상 형식: '{htmlColor}'. '#RRGGBB' 형식이어야 합니다.");

        var r = Convert.ToInt32(hex.Substring(0, 2), 16);
        var g = Convert.ToInt32(hex.Substring(2, 2), 16);
        var b = Convert.ToInt32(hex.Substring(4, 2), 16);

        // Windows COLORREF: 0x00BBGGRR
        return (b << 16) | (g << 8) | r;
    }

    /// <summary>HTML 색상 → COLORREF 십진수 문자열 (XML 직접 삽입용).</summary>
    public static string ToColorRefString(string htmlColor) =>
        ToColorRef(htmlColor).ToString();
}
