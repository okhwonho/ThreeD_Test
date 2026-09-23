using System.IO;
using System.Xml;
using ZenonXmlGenerator;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Tests;

/// <summary>
/// 전력 단선도(SLD) 기기 DTO, Topology 확장 및 ALC 속성 검증 테스트.
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
        Assert.Equal("0", picture.SelectSingleNode("Type")!.InnerText);
        Assert.Equal("TRUE", picture.SelectSingleNode("SizeFromTemplate")!.InnerText);
    }

    [Fact]
    public void BusbarLine_OutputsCorrectLineWidthAndALCUseColor()
    {
        var gen = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromJson(SampleSldJson);
        var xml = LoadXml(bytes);

        // BUS_1 is Elements_4
        var bus1 = xml.DocumentElement!.SelectSingleNode("Apartment/Picture/Elements_4");
        Assert.NotNull(bus1);
        Assert.Equal("101", bus1.Attributes!["TYPE"]!.Value);
        Assert.Equal("5", bus1.SelectSingleNode("LineWidth")!.InnerText);
        Assert.Equal("TRUE", bus1.SelectSingleNode("ALCUseColor")!.InnerText);
    }

    [Fact]
    public void CircuitBreaker_OutputsALCType2_AndDynEleVar()
    {
        var gen = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromJson(SampleSldJson);
        var xml = LoadXml(bytes);

        // CB11 is Elements_16 (Elements_0..1 Bay Frames, Elements_2..3 TXT, Elements_4 BUS1, Elements_5..6 TXT/BUS2, Elements_7 TXT, Elements_8 L, Elements_9 DS11, Elements_10 L, Elements_11 DS12, Elements_12..15 Lines, Elements_16 CB11)
        var cb11 = xml.DocumentElement!.SelectSingleNode("//Elements_16");
        Assert.NotNull(cb11);
        Assert.Equal("16", cb11.Attributes!["TYPE"]!.Value);
        Assert.Equal("2", cb11.SelectSingleNode("ALCType")!.InnerText);
        Assert.Equal("SS1.BAY1.CB11.Status", cb11.SelectSingleNode("DynEleVar_0/ProjectVar")!.InnerText);
    }

    [Fact]
    public void Disconnector_OutputsALCType7()
    {
        var gen = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromJson(SampleSldJson);
        var xml = LoadXml(bytes);

        // DS11 is Elements_9
        var ds11 = xml.DocumentElement!.SelectSingleNode("//Elements_9");
        Assert.NotNull(ds11);
        Assert.Equal("16", ds11.Attributes!["TYPE"]!.Value);
        Assert.Equal("7", ds11.SelectSingleNode("ALCType")!.InnerText);
        Assert.Equal("SS1.BAY1.DS11.Status", ds11.SelectSingleNode("DynEleVar_0/ProjectVar")!.InnerText);
    }

    [Fact]
    public void Transformer_OutputsALCType4()
    {
        var gen = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromJson(SampleSldJson);
        var xml = LoadXml(bytes);

        // TR1 is Elements_24
        var tr1 = xml.DocumentElement!.SelectSingleNode("//Elements_24");
        Assert.NotNull(tr1);
        Assert.Equal("16", tr1.Attributes!["TYPE"]!.Value);
        Assert.Equal("4", tr1.SelectSingleNode("ALCType")!.InnerText);
        Assert.Equal("SS1.BAY1.TR1.Status", tr1.SelectSingleNode("DynEleVar_0/ProjectVar")!.InnerText);
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
}
