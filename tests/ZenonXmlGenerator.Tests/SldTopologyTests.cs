using System.IO;
using System.Xml;
using ZenonXmlGenerator;
using ZenonXmlGenerator.Models;
using ZenonXmlGenerator.Xml;

namespace ZenonXmlGenerator.Tests;

/// <summary>
/// 전력 단선도(SLD) 기기 DTO, Topology 확장 및 벡터 렌더링 검증 테스트.
/// VectorSymbolRenderer가 SymbolElement → 벡터 프리미티브로 변환하는 동작을 검증.
/// </summary>
public sealed class SldTopologyTests
{
    private static readonly string SampleSldJson = File.ReadAllText(
        Path.Combine("Samples", "sample_sld_topology.json"));

    private static XmlDocument LoadXml(byte[] bytes)
    {
        var doc = new XmlDocument();
        using var ms = new MemoryStream(bytes);
        doc.Load(ms);
        return doc;
    }

    [Fact]
    public void GenerateFromJson_SampleSld_SucceedsAndMatchesStructure()
    {
        var gen = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromJson(SampleSldJson);
        var xml = LoadXml(bytes);

        var picture = xml.DocumentElement!.SelectSingleNode("Apartment/Picture");
        Assert.NotNull(picture);
        Assert.Equal("Substation_154kV_SLD", picture.Attributes!["ShortName"]!.Value);
        Assert.Equal("MAIN", picture.SelectSingleNode("Template")!.InnerText);
        Assert.Equal("0",    picture.SelectSingleNode("Type")!.InnerText);
        Assert.Equal("TRUE", picture.SelectSingleNode("SizeFromTemplate")!.InnerText);
    }

    [Fact]
    public void Picture_BackgroundColor_IsDarkNavy()
    {
        var gen = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromJson(SampleSldJson);
        var xml = LoadXml(bytes);

        var bg = xml.DocumentElement!.SelectSingleNode("Apartment/Picture/BackgroundColor")!.InnerText;
        Assert.Equal("07101C", bg);
    }

    [Fact]
    public void BusbarLine_OutputsCorrectLineWidthAndALCUseColor()
    {
        var gen = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromJson(SampleSldJson);
        var xml = LoadXml(bytes);

        // Find busbar line (TYPE=101, LineWidth=5, ALCUseColor=TRUE)
        var bus1 = xml.DocumentElement!.SelectSingleNode(
            "Apartment/Picture/*[@TYPE='101' and LineWidth[text()='5']]");
        Assert.NotNull(bus1);
        Assert.Equal("TRUE", bus1.SelectSingleNode("ALCUseColor")!.InnerText);
    }

    [Fact]
    public void CircuitBreaker_VectorRendered_AsGreenFilledRectangle()
    {
        var gen = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromJson(SampleSldJson);
        var xml = LoadXml(bytes);

        // CB11 is rendered as a green rectangle (TYPE=102, FillPattern=6, BackColor=00C853, Width=32).
        var cbBodies = xml.DocumentElement!.SelectNodes(
            "Apartment/Picture/*[@TYPE='102' and FillPattern[text()='6'] and Width[text()='32']]");
        Assert.NotNull(cbBodies);
        Assert.True(cbBodies.Count > 0, "Expected at least one CB body rectangle (FillPattern=6, Width=32, TYPE=102).");
    }

    [Fact]
    public void Disconnector_VectorRendered_AsHollowRectangle()
    {
        var gen = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromJson(SampleSldJson);
        var xml = LoadXml(bytes);

        // DS elements → TYPE=102, FillPattern=0, AlphaBackColor=0, Width=24
        var dsRects = xml.DocumentElement!.SelectNodes(
            "Apartment/Picture/*[@TYPE='102' and FillPattern[text()='0'] and Width[text()='24']]");
        Assert.NotNull(dsRects);
        Assert.True(dsRects.Count > 0, "Expected at least one DS hollow rectangle (FillPattern=0, Width=24).");
    }

    [Fact]
    public void Transformer_VectorRendered_AsThreeCircles()
    {
        var gen = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromJson(SampleSldJson);
        var xml = LoadXml(bytes);

        // TR1 → 3 circles TYPE=103
        var circles = xml.DocumentElement!.SelectNodes("Apartment/Picture/*[@TYPE='103']");
        Assert.NotNull(circles);
        // Each Transformer generates 3 circles. sample_sld has 1 TR → 3 circles.
        Assert.True(circles.Count >= 3, $"Expected >=3 circles (TYPE=103) for transformer. Got {circles.Count}.");
    }

    [Fact]
    public void EffectiveALCType_InfersFromDeviceType_WhenNotExplicitlySet()
    {
        var cb = new SymbolElement
        {
            DeviceType = DeviceType.CircuitBreaker,
            LibrarySymbolName = "CB_Symbol",
            VariableName = "Var.CB"
        };
        Assert.Equal("2", cb.EffectiveALCType);

        var ds = new SymbolElement
        {
            DeviceType = DeviceType.Disconnector,
            LibrarySymbolName = "DS_Symbol",
            VariableName = "Var.DS"
        };
        Assert.Equal("7", ds.EffectiveALCType);

        var tr = new SymbolElement
        {
            DeviceType = DeviceType.Transformer,
            LibrarySymbolName = "TR_Symbol",
            VariableName = "Var.TR"
        };
        Assert.Equal("4", tr.EffectiveALCType);
    }

    [Fact]
    public void VectorSymbolRenderer_NoType16ElementsInOutput()
    {
        var gen = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromJson(SampleSldJson);
        var xml = LoadXml(bytes);

        // No TYPE=16 (library symbol) elements should exist in output
        var type16 = xml.DocumentElement!.SelectNodes("Apartment/Picture/*[@TYPE='16']");
        Assert.NotNull(type16);
        Assert.Equal(0, type16.Count);
    }
}
