using System.Text.Json.Serialization;

namespace ZenonXmlGenerator.Models;

/// <summary>
/// Topology 요소의 공통 기반 클래스.
/// System.Text.Json 다형성: "type" 필드 값으로 구체 타입 결정.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(LineElement),      typeDiscriminator: "line")]
[JsonDerivedType(typeof(RectangleElement), typeDiscriminator: "rectangle")]
[JsonDerivedType(typeof(TextElement),      typeDiscriminator: "text")]
[JsonDerivedType(typeof(SymbolElement),    typeDiscriminator: "symbol")]
public abstract class TopologyElement
{
    /// <summary>요소 고유 ID (XML 코멘트 및 디버그용).</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>회전 각도 (도 단위: 0, 90, 180, 270 등; 비전/CAD 연계용).</summary>
    [JsonPropertyName("rotation")]
    public double Rotation { get; set; } = 0.0;

    /// <summary>식별 태그 라벨 (예: "CB101", "DS102" 등; 비전/CAD 연계용).</summary>
    [JsonPropertyName("tagLabel")]
    public string? TagLabel { get; set; }
}
