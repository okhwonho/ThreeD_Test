using System.IO;
using System.Text;
using System.Xml;
using ZenonXmlGenerator;

namespace ZenonXmlGenerator.Tests;

/// <summary>
/// 출력 XML의 계층 구조 및 루트/컨테이너 노드가 zenon 15 Ground Truth를 만족하는지 검증.
///
/// 정식 계층:
///   Subject (ShortName / MainVersion)
///     └ Apartment (ShortName / Version)
///         └ Picture (ShortName) + 메타데이터
/// </summary>
public sealed class RootNodeTests
{
    private static readonly string SampleJson = File.ReadAllText(
        Path.Combine("Samples", "sample_topology.json"));

    private static XmlDocument LoadXml(byte[] bytes)
    {
        var doc = new XmlDocument();
        using var ms = new MemoryStream(bytes);
        doc.Load(ms);
        return doc;
    }

    // ─── Subject 검증 ────────────────────────────────────────────────────

    [Fact]
    public void RootElement_IsSubject()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        Assert.Equal("Subject", xml.DocumentElement!.LocalName);
    }

    [Fact]
    public void RootElement_HasCorrectShortName()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        Assert.Equal("zenOn(R) exported project",
            xml.DocumentElement!.GetAttribute("ShortName"));
    }

    [Fact]
    public void RootElement_HasMainVersion15000()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        Assert.Equal("15000", xml.DocumentElement!.GetAttribute("MainVersion"));
    }

    // ─── Apartment 검증 ──────────────────────────────────────────────────

    [Fact]
    public void Apartment_Exists()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var apt = xml.DocumentElement!.SelectSingleNode("Apartment");
        Assert.NotNull(apt);
    }

    [Fact]
    public void Apartment_HasCorrectShortName()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var apt = (XmlElement)xml.DocumentElement!.SelectSingleNode("Apartment")!;
        Assert.Equal("zenOn(R) pictures list", apt.GetAttribute("ShortName"));
    }

    [Fact]
    public void Apartment_HasVersion15000()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var apt = (XmlElement)xml.DocumentElement!.SelectSingleNode("Apartment")!;
        Assert.Equal("15000", apt.GetAttribute("Version"));
    }

    // ─── Picture 검증 ────────────────────────────────────────────────────

    [Fact]
    public void Picture_Exists()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var pic = xml.DocumentElement!.SelectSingleNode("Apartment/Picture");
        Assert.NotNull(pic);
    }

    [Fact]
    public void Picture_HasCorrectShortName()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var pic = (XmlElement)xml.DocumentElement!.SelectSingleNode("Apartment/Picture")!;
        Assert.Equal("Main_SLD", pic.GetAttribute("ShortName"));
    }

    [Fact]
    public void Picture_HasTitleMetadata()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var title = xml.DocumentElement!.SelectSingleNode("Apartment/Picture/Title")!.InnerText;
        Assert.Equal("Main_SLD", title);
    }

    [Fact]
    public void Picture_HasTemplateMetadata()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var template = xml.DocumentElement!.SelectSingleNode("Apartment/Picture/Template")!.InnerText;
        Assert.Equal("MAIN", template);
    }

    [Fact]
    public void Picture_HasTypeMetadata_StandardScreen0()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var type = xml.DocumentElement!.SelectSingleNode("Apartment/Picture/Type")!.InnerText;
        Assert.Equal("0", type);
    }

    [Fact]
    public void Picture_HasSizeFromTemplateMetadata()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var sizeFromTemplate = xml.DocumentElement!.SelectSingleNode("Apartment/Picture/SizeFromTemplate")!.InnerText;
        Assert.Equal("TRUE", sizeFromTemplate);
    }

    [Fact]
    public void Picture_HasBackgroundColorMetadata()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var bg = xml.DocumentElement!.SelectSingleNode("Apartment/Picture/BackgroundColor")!.InnerText;
        Assert.Equal("80000037", bg);
    }

    [Fact]
    public void Picture_HasWidthAndHeightMetadata()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var w = xml.DocumentElement!.SelectSingleNode("Apartment/Picture/Width")!.InnerText;
        var h = xml.DocumentElement!.SelectSingleNode("Apartment/Picture/Height")!.InnerText;
        Assert.Equal("1920", w);
        Assert.Equal("1080", h);
    }

    [Fact]
    public void RectangleElement_OutputsTransparentAttributes()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var rect = xml.DocumentElement!.SelectSingleNode("Apartment/Picture/Elements_0");
        Assert.NotNull(rect);
        Assert.Equal("102", rect.Attributes!["TYPE"]!.Value);
        Assert.Equal("0", rect.SelectSingleNode("FillPattern")!.InnerText);
        Assert.Equal("0", rect.SelectSingleNode("AlphaBackColor")!.InnerText);
        Assert.Equal("1", rect.SelectSingleNode("LineWidth")!.InnerText);
        Assert.Equal("5C6C75", rect.SelectSingleNode("LineColorEx")!.InnerText);
    }
}
