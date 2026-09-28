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
    public void BusbarLine_OutputsCorrectLineWidthAndALCUseColor()
    {
        var gen = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromJson(SampleSldJson);
        var xml = LoadXml(bytes);

        // BUS_1 is Elements_4 (bay frames at 0..1, texts at 2..3)
        var bus1 = xml.DocumentElement!.SelectSingleNode("Apartment/Picture/Elements_4");
        Assert.NotNull(bus1);
        Assert.Equal("101", bus1.Attributes!["TYPE"]!.Value);
        Assert.Equal("5",   bus1.SelectSingleNode("LineWidth")!.InnerText);
        Assert.Equal("TRUE", bus1.SelectSingleNode("ALCUseColor")!.InnerText);
    }

    [Fact]
    public void CircuitBreaker_VectorRendered_AsGreenFilledRectangle()
    {
        var gen = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromJson(SampleSldJson);
        var xml = LoadXml(bytes);

        // CB11 is now rendered as a green rectangle (TYPE=102, FillPattern=8).
        // Variable-name based lookup via any descendant of Picture elements.
        // CB body rectangle has id suffix "_BODY" — search for all TYPE=102 rectangles with FillPattern=8.
        var cbBodies = xml.DocumentElement!.SelectNodes(
            "Apartment/Picture/*[@TYPE='102' and FillPattern[text()='8']]");
        Assert.NotNull(cbBodies);
        Assert.True(cbBodies.Count > 0, "Expected at least one CB body rectangle (FillPattern=8, TYPE=102).");
    }

    [Fact]
    public void Disconnector_VectorRendered_AsHollowRectangle()
    {
        var gen = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromJson(SampleSldJson);
        var xml = LoadXml(bytes);

        // DS elements → TYPE=102, FillPattern=0 (hollow)
        var dsRects = xml.DocumentElement!.SelectNodes(
            "Apartment/Picture/*[@TYPE='102' and FillPattern[text()='0'] and LineWidth[text()='2']]");
        Assert.NotNull(dsRects);
        Assert.True(dsRects.Count > 0, "Expected at least one DS hollow rectangle (FillPattern=0, LineWidth=2).");
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
