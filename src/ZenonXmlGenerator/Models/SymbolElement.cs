using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ZenonXmlGenerator.Models;

/// <summary>
/// 심볼(Combined element) 요소. zenon TYPE="16"
/// - variableName: 변수 바인딩 (DynEleVar_0)
/// - librarySymbolName: 기본 심볼 이름 (states 생략 시 자동 생성에 사용)
/// - states: zenon States_n 배열. null/비어 있으면 기본 3-state 자동 생성.
/// </summary>
public sealed class SymbolElement : TopologyElement
{
    [JsonPropertyName("x")]      public int X      { get; set; }
    [JsonPropertyName("y")]      public int Y      { get; set; }
    [JsonPropertyName("width")]  public int Width  { get; set; }
    [JsonPropertyName("height")] public int Height { get; set; }

    /// <summary>zenon Symbol Library 기본 심볼 이름.</summary>
    [JsonPropertyName("librarySymbolName")]
    public string LibrarySymbolName { get; set; } = string.Empty;

    /// <summary>바인딩할 zenon 프로젝트 변수 전체 경로.</summary>
    [JsonPropertyName("variableName")]
    public string VariableName { get; set; } = string.Empty;

    /// <summary>전력 기기 유형 (선택적: CircuitBreaker, Disconnector, Transformer 등).</summary>
    [JsonPropertyName("deviceType")]
    public DeviceType? DeviceType { get; set; }

    /// <summary>
    /// ALC 타입 (선택적: CircuitBreaker="2", Disconnector="7", Transformer="4" 등).
    /// </summary>
    [JsonPropertyName("alcType")]
    public string? ALCType { get; set; }

    /// <summary>
    /// States 배열. null 또는 빈 배열이면 LibrarySymbolName 기반 기본 3-state 자동 생성:
    ///   States_0: Value=0, ValueMask=0            (wildcard/default)
    ///   States_1: Value=0, ValueMask=4294967295   (OFF exact)
    ///   States_2: Value=1, ValueMask=4294967295   (ON exact)
    /// 커스텀 상태 지정 시 해당 배열을 그대로 사용.
    /// </summary>
    [JsonPropertyName("states")]
    public List<SymbolState>? States { get; set; }

    /// <summary>실제 사용할 States 목록 (auto-fallback 포함).</summary>
    [JsonIgnore]
    public IReadOnlyList<SymbolState> EffectiveStates =>
        (States is { Count: > 0 }) ? States : BuildDefaultStates();

    /// <summary>
    /// 유효 ALCType 값 (명시적 ALCType 우선, 없으면 DeviceType 기반 자동 도출).
    /// </summary>
    [JsonIgnore]
    public string? EffectiveALCType => ALCType ?? DeviceType switch
    {
        Models.DeviceType.CircuitBreaker => Xml.XmlConstants.ALCTypeCircuitBreaker,
        Models.DeviceType.Disconnector   => Xml.XmlConstants.ALCTypeDisconnector,
        Models.DeviceType.Transformer    => Xml.XmlConstants.ALCTypeTransformer,
        _ => null,
    };

    private List<SymbolState> BuildDefaultStates() =>
    [
        new() { Value = 0, ValueMask = 0,          SymbolName = LibrarySymbolName },
        new() { Value = 0, ValueMask = 4294967295L, SymbolName = LibrarySymbolName },
        new() { Value = 1, ValueMask = 4294967295L, SymbolName = LibrarySymbolName },
    ];
}
