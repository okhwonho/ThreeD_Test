using System.IO;
using System.Text;
using ZenonXmlGenerator;

namespace ZenonXmlGenerator.Tests;

/// <summary>
/// 출력 XML이 UTF-16 LE BOM (0xFF 0xFE)로 시작하는지 검증.
/// </summary>
public sealed class EncodingTests
{
    private static readonly string SampleJson = File.ReadAllText(
        Path.Combine("Samples", "sample_topology.json"));

    [Fact]
    public void GenerateFromJson_OutputStartsWithUtf16LeBom()
    {
        var gen   = new ZenonXmlGenerator();
        byte[] bytes = gen.GenerateFromJson(SampleJson);

        // UTF-16 LE BOM: 0xFF 0xFE
        Assert.True(bytes.Length >= 2, "출력 바이트가 너무 짧습니다.");
        Assert.Equal(0xFF, bytes[0]);
        Assert.Equal(0xFE, bytes[1]);
    }

    [Fact]
    public void GenerateFromJson_OutputIsDecodableAsUtf16()
    {
        var gen    = new ZenonXmlGenerator();
        byte[] bytes = gen.GenerateFromJson(SampleJson);

        // BOM을 건너뛴 뒤 UTF-16 LE 문자열로 디코딩 가능해야 함
        // (BOM은 Encoding.Unicode가 처리하므로 그대로 디코딩)
        var xmlText = Encoding.Unicode.GetString(bytes);
        Assert.Contains("<?xml", xmlText);
        Assert.Contains("utf-16", xmlText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateToFile_FileStartsWithUtf16LeBom()
    {
        var gen      = new ZenonXmlGenerator();
        var filePath = Path.Combine(Path.GetTempPath(), $"zenon_enc_test_{System.Guid.NewGuid():N}.xml");
        try
        {
            gen.GenerateToFile(SampleJson, filePath);
            byte[] fileBytes = File.ReadAllBytes(filePath);

            Assert.Equal(0xFF, fileBytes[0]);
            Assert.Equal(0xFE, fileBytes[1]);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }
}
