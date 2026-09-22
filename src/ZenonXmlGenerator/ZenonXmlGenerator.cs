using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZenonXmlGenerator.Models;
using ZenonXmlGenerator.Xml;

namespace ZenonXmlGenerator;

/// <summary>
/// ZenonXmlGenerator 라이브러리의 공개 진입점(Facade).
///
/// 사용 예시:
/// <code>
/// var gen = new ZenonXmlGenerator();
/// byte[] xmlBytes = gen.GenerateFromJson(jsonString);
/// gen.GenerateToFile(jsonString, "output_test_screen.xml");
/// </code>
/// </summary>
public sealed class ZenonXmlGenerator
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly ZenonXmlBuilder _builder;

    /// <summary>기본 설정으로 인스턴스를 생성한다.</summary>
    public ZenonXmlGenerator()
    {
        _builder = new ZenonXmlBuilder();
    }

    /// <summary>
    /// Topology JSON 문자열을 파싱하여 zenon Screen XML의 UTF-16 BOM 바이트 배열을 반환.
    /// </summary>
    /// <param name="topologyJson">Topology JSON 문자열.</param>
    /// <returns>UTF-16 LE BOM 포함 XML 바이트 배열.</returns>
    /// <exception cref="JsonException">JSON 파싱 실패 시.</exception>
    /// <exception cref="InvalidOperationException">필수 필드 누락 시.</exception>
    public byte[] GenerateFromJson(string topologyJson)
    {
        var document = ParseJson(topologyJson);
        return _builder.Build(document);
    }

    /// <summary>
    /// Topology JSON 문자열을 파싱하여 <paramref name="outputFilePath"/>에 zenon XML을 저장.
    /// </summary>
    /// <param name="topologyJson">Topology JSON 문자열.</param>
    /// <param name="outputFilePath">출력 파일 경로 (.xml).</param>
    public void GenerateToFile(string topologyJson, string outputFilePath)
    {
        var document = ParseJson(topologyJson);
        _builder.BuildToFile(document, outputFilePath);
    }

    /// <summary>
    /// 이미 역직렬화된 <see cref="TopologyDocument"/>로부터 XML 바이트 배열을 생성.
    /// </summary>
    public byte[] GenerateFromDocument(TopologyDocument document) =>
        _builder.Build(document);

    // ─── Private ────────────────────────────────────────────────────────

    private static TopologyDocument ParseJson(string json)
    {
        var doc = JsonSerializer.Deserialize<TopologyDocument>(json, s_jsonOptions)
            ?? throw new InvalidOperationException("JSON 역직렬화 결과가 null입니다.");
        return doc;
    }
}
