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

    /// <summary>zenon Picture Template(프레임) 이름. 기본값 "Standard".</summary>
    [JsonPropertyName("template")]
    public string Template { get; set; } = "Standard";

    [JsonPropertyName("width")]
    public int Width { get; set; } = 1920;

    [JsonPropertyName("height")]
    public int Height { get; set; } = 1080;

    [JsonPropertyName("elements")]
    public List<TopologyElement> Elements { get; set; } = new();
}
