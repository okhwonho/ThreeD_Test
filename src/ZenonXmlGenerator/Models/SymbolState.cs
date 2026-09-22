using System.Text.Json.Serialization;

namespace ZenonXmlGenerator.Models;

/// <summary>
/// Symbol 요소의 개별 상태 정의 (zenon States_n에 1:1 대응).
/// ValueMask = 0          → default/wildcard 상태 (States_0)
/// ValueMask = 4294967295 → exact-match 상태 (States_1 이후)
/// </summary>
public sealed class SymbolState
{
    /// <summary>변수 값 (0 = OFF, 1 = ON, …)</summary>
    [JsonPropertyName("value")]
    public long Value { get; set; }

    /// <summary>
    /// 비교 마스크. 0이면 wildcard(default), 0xFFFFFFFF이면 exact-match.
    /// JSON에서 생략 시 auto 규칙으로 채움.
    /// </summary>
    [JsonPropertyName("valueMask")]
    public long ValueMask { get; set; }

    /// <summary>이 상태에서 표시할 zenon Symbol Library 심볼 이름.</summary>
    [JsonPropertyName("symbolName")]
    public string SymbolName { get; set; } = string.Empty;
}
