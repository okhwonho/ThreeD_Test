using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ZenonXmlGenerator.Models;

/// <summary>
/// Topology JSON 최상위 문서. zenon Screen 1개에 대응한다.
/// </summary>
public sealed class TopologyDocument
{
    [JsonPropertyName("screenName")]
    public string ScreenName { get; set; } = "Screen1";

    /// <summary>zenon Picture Template(프레임) 이름. 기본값 "MAIN".</summary>
    [JsonPropertyName("template")]
    public string Template { get; set; } = "MAIN";

    /// <summary>
    /// 프레임으로부터 크기 자동 적용 여부.
    /// FALSE = Width/Height 기준 렌더링 (3840 와이드 등). TRUE = MAIN 프레임 크기 강제.
    /// </summary>
    [JsonPropertyName("sizeFromTemplate")]
    public string SizeFromTemplate { get; set; } = "FALSE";

    [JsonPropertyName("width")]
    public int Width { get; set; } = 3440;

    [JsonPropertyName("height")]
    public int Height { get; set; } = 1440;

    [JsonPropertyName("elements")]
    public List<TopologyElement> Elements { get; set; } = new();
}
