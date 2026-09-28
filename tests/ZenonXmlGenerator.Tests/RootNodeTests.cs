using System.IO;
using System.Xml;
using ZenonXmlGenerator;

namespace ZenonXmlGenerator.Tests;

/// <summary>
/// zenon Screen XML의 루트 노드 계층 구조 및 메타데이터 검증.
///
/// 검증 대상 구조:
/// &lt;Subject ShortName="zenOn(R) exported project" MainVersion="15000"&gt;
///   &lt;Apartment ShortName="zenOn(R) pictures list" Version="15000"&gt;
///     &lt;Picture ShortName="{ScreenName}"&gt;
///       &lt;Title&gt;{ScreenName}&lt;/Title&gt;
///       &lt;Template&gt;MAIN&lt;/Template&gt;
///       &lt;Type&gt;0&lt;/Type&gt;
///       &lt;SizeFromTemplate&gt;FALSE&lt;/SizeFromTemplate&gt;
///       &lt;Width&gt;...&lt;/Width&gt;
///       &lt;Height&gt;...&lt;/Height&gt;
///       &lt;BackgroundColor&gt;1C1007&lt;/BackgroundColor&gt;
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

    // ─── Subject 노드 검증 ───────────────────────────────────────────────

    [Fact]
    public void RootNode_IsSubject()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        Assert.Equal("Subject", xml.DocumentElement!.Name);
    }

    [Fact]
    public void Subject_HasCorrectShortNameAttribute()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var attr = xml.DocumentElement!.GetAttribute("ShortName");
        Assert.Equal("zenOn(R) exported project", attr);
    }

    [Fact]
    public void Subject_HasMainVersion15000()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var attr = xml.DocumentElement!.GetAttribute("MainVersion");
        Assert.Equal("15000", attr);
    }

    // ─── Apartment 노드 검증 ─────────────────────────────────────────────

    [Fact]
    public void Subject_ContainsSingleApartmentNode()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var apartmentNodes = xml.DocumentElement!.SelectNodes("Apartment");
        Assert.NotNull(apartmentNodes);
        Assert.Equal(1, apartmentNodes.Count);
    }

    [Fact]
    public void Apartment_HasCorrectShortNameAttribute()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var apt = xml.DocumentElement!.SelectSingleNode("Apartment") as XmlElement;
        Assert.NotNull(apt);
        Assert.Equal("zenOn(R) pictures list", apt.GetAttribute("ShortName"));
    }

    [Fact]
    public void Apartment_HasVersion15000()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var apt = xml.DocumentElement!.SelectSingleNode("Apartment") as XmlElement;
        Assert.NotNull(apt);
        Assert.Equal("15000", apt.GetAttribute("Version"));
    }

    // ─── Picture 노드 검증 ───────────────────────────────────────────────

    [Fact]
    public void Apartment_ContainsSinglePictureNode()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var pictureNodes = xml.DocumentElement!.SelectNodes("Apartment/Picture");
        Assert.NotNull(pictureNodes);
        Assert.Equal(1, pictureNodes.Count);
    }

    [Fact]
    public void Picture_HasShortNameFromScreenName()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var picture = xml.DocumentElement!.SelectSingleNode("Apartment/Picture") as XmlElement;
        Assert.NotNull(picture);
        Assert.Equal("Main_SLD", picture.GetAttribute("ShortName"));
    }

    [Fact]
    public void Picture_HasTitleMetadata_MatchingScreenName()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var title = xml.DocumentElement!.SelectSingleNode("Apartment/Picture/Title")!.InnerText;
        Assert.Equal("Main_SLD", title);
    }

    [Fact]
    public void Picture_HasTemplateMetadata_DefaultMain()
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
        Assert.Equal("FALSE", sizeFromTemplate);
    }

    [Fact]
    public void Picture_HasBackgroundColorMetadata()
    {
        var xml = LoadXml(new ZenonXmlGenerator().GenerateFromJson(SampleJson));
        var bg = xml.DocumentElement!.SelectSingleNode("Apartment/Picture/BackgroundColor")!.InnerText;
        Assert.Equal("07101C", bg);
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
        Assert.Equal("0", rect.SelectSingleNode("AlphaBackColor")!.InnerText);
        Assert.Equal("0", rect.SelectSingleNode("FillPattern")!.InnerText);
        Assert.Equal("1", rect.SelectSingleNode("LineWidth")!.InnerText);
        Assert.Equal("5C6C75", rect.SelectSingleNode("LineColorEx")!.InnerText);
    }
}
