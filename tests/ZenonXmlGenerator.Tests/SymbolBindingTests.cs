using System.Collections.Generic;
using System.IO;
using System.Xml;
using ZenonXmlGenerator;
using ZenonXmlGenerator.Models;
using ZenonXmlGenerator.Xml;

namespace ZenonXmlGenerator.Tests;

/// <summary>
/// Symbol 요소의 자체 완결형 벡터 렌더링(VectorSymbolRenderer) 검증.
///
/// 변경 내역:
///   이전: TYPE=16(library symbol), DynEleVar_0, States_n 구조 검증
///   현재: VectorSymbolRenderer가 SymbolElement → 벡터 프리미티브(Rectangle/Circle/Line)로 분해
///
/// 검증 전략:
///   - 출력에 TYPE=16 없음
///   - CB → TYPE=102 filled rectangle (FillPattern=8)
///   - DS → TYPE=102 hollow rectangle (FillPattern=0)
///   - ES → TYPE=103 circle + lines
///   - TR → 3개 TYPE=103 circles
///   - SymbolElement.EffectiveALCType 모델 속성은 유지
/// </summary>
public sealed class SymbolBindingTests
{
    private static XmlDocument BuildWithSymbol(SymbolElement sym)
    {
        var topDoc = new TopologyDocument
        {
            ScreenName = "TestScreen",
            Elements   = new List<TopologyElement> { sym },
        };
        var gen   = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromDocument(topDoc);
        var doc   = new XmlDocument();
        using var ms = new MemoryStream(bytes);
        doc.Load(ms);
        return doc;
    }

    private static XmlNodeList GetPictureChildren(XmlDocument doc) =>
        doc.DocumentElement!.SelectNodes("Apartment/Picture/*[@TYPE]")!;

    // ─── VectorSymbolRenderer: TYPE=16 없음 검증 ────────────────────────

    [Fact]
    public void Symbol_VectorRenderer_NoType16InOutput()
    {
        var sym = new SymbolElement
        {
            Id = "S1", DeviceType = DeviceType.CircuitBreaker,
            CenterX = 100, CenterY = 100, LibrarySymbolName = "CB_Open",
        };
        var xml    = BuildWithSymbol(sym);
        var type16 = xml.DocumentElement!.SelectNodes("Apartment/Picture/*[@TYPE='16']")!;
        Assert.Equal(0, type16.Count);
    }

    // ─── CircuitBreaker → 녹색 채움 사각형 (TYPE=102, FillPattern=8) ───

    [Fact]
    public void Symbol_CircuitBreaker_RenderedAsFilledRectangle()
    {
        var sym = new SymbolElement
        {
            Id = "CB1", DeviceType = DeviceType.CircuitBreaker,
            CenterX = 200, CenterY = 300, LibrarySymbolName = "CB_Open",
        };
        var xml     = BuildWithSymbol(sym);
        var body    = xml.DocumentElement!.SelectSingleNode("Apartment/Picture/*[@TYPE='102']");
        Assert.NotNull(body);

        var fp = body.SelectSingleNode("FillPattern")?.InnerText;
        Assert.Equal("8", fp);
    }

    [Fact]
    public void Symbol_CircuitBreaker_HasStandardSize32x32()
    {
        var sym = new SymbolElement
        {
            Id = "CB1", DeviceType = DeviceType.CircuitBreaker,
            CenterX = 200, CenterY = 300, LibrarySymbolName = "CB_Open",
        };
        var xml  = BuildWithSymbol(sym);
        var body = xml.DocumentElement!.SelectSingleNode("Apartment/Picture/*[@TYPE='102']");
        Assert.NotNull(body);
        Assert.Equal("32", body.SelectSingleNode("Width")!.InnerText);
        Assert.Equal("32", body.SelectSingleNode("Height")!.InnerText);
    }

    [Fact]
    public void Symbol_CircuitBreaker_CenterXCenterY_SnapsToTopLeft()
    {
        // centerX=200, centerY=300, CB size=32 → StartX=184, StartY=284
        var sym = new SymbolElement
        {
            Id = "CB1", DeviceType = DeviceType.CircuitBreaker,
            CenterX = 200, CenterY = 300, LibrarySymbolName = "CB_Open",
        };
        var xml  = BuildWithSymbol(sym);
        var body = xml.DocumentElement!.SelectSingleNode("Apartment/Picture/*[@TYPE='102']");
        Assert.NotNull(body);
        Assert.Equal("184", body.SelectSingleNode("StartX")!.InnerText);
        Assert.Equal("284", body.SelectSingleNode("StartY")!.InnerText);
    }

    // ─── Disconnector → 빈 사각형 테두리 (TYPE=102, FillPattern=0) ─────

    [Fact]
    public void Symbol_Disconnector_RenderedAsHollowRectangle()
    {
        var sym = new SymbolElement
        {
            Id = "DS1", DeviceType = DeviceType.Disconnector,
            CenterX = 150, CenterY = 250, LibrarySymbolName = "DS_Open",
        };
        var xml  = BuildWithSymbol(sym);
        // First TYPE=102 element should be hollow DS (FillPattern=0)
        var body = xml.DocumentElement!.SelectSingleNode(
            "Apartment/Picture/*[@TYPE='102' and FillPattern[text()='0']]");
        Assert.NotNull(body);
        Assert.Equal("24", body.SelectSingleNode("Width")!.InnerText);
        Assert.Equal("24", body.SelectSingleNode("Height")!.InnerText);
    }

    // ─── EarthSwitch → 원 + 접지 인출선 + 사다리 ──────────────────────

    [Fact]
    public void Symbol_EarthSwitch_RenderedAsCircleAndLines()
    {
        var sym = new SymbolElement
        {
            Id = "ES1", DeviceType = DeviceType.EarthSwitch,
            CenterX = 300, CenterY = 400, LibrarySymbolName = "ES_Open",
        };
        var xml     = BuildWithSymbol(sym);
        var circles = xml.DocumentElement!.SelectNodes("Apartment/Picture/*[@TYPE='103']")!;
        var lines   = xml.DocumentElement!.SelectNodes("Apartment/Picture/*[@TYPE='101']")!;

        Assert.True(circles.Count >= 1, "EarthSwitch should produce at least 1 circle.");
        Assert.True(lines.Count >= 4, "EarthSwitch should produce lead line + 3 ground rungs (>=4 lines).");
    }

    // ─── Transformer → 3개 원 (TYPE=103) + 텍스트 ─────────────────────

    [Fact]
    public void Symbol_Transformer_RenderedAsThreeCircles()
    {
        var sym = new SymbolElement
        {
            Id = "TR1", DeviceType = DeviceType.Transformer,
            CenterX = 500, CenterY = 500, LibrarySymbolName = "TR_YYD",
        };
        var xml     = BuildWithSymbol(sym);
        var circles = xml.DocumentElement!.SelectNodes("Apartment/Picture/*[@TYPE='103']")!;
        Assert.Equal(3, circles.Count);
    }

    [Fact]
    public void Symbol_Transformer_HasWindingTextLabels()
    {
        var sym = new SymbolElement
        {
            Id = "TR1", DeviceType = DeviceType.Transformer,
            CenterX = 500, CenterY = 500, LibrarySymbolName = "TR_YYD",
        };
        var xml   = BuildWithSymbol(sym);
        var texts = xml.DocumentElement!.SelectNodes("Apartment/Picture/*[@TYPE='107']")!;
        // 3 winding labels (Y, Y, Δ)
        Assert.True(texts.Count >= 3, $"Expected >=3 winding text labels (Y/Y/Δ). Got {texts.Count}.");
    }

    // ─── SymbolElement 모델 속성 (VectorSymbolRenderer 무관) ──────────

    [Fact]
    public void SymbolElement_EffectiveALCType_CircuitBreaker_Is2()
    {
        var cb = new SymbolElement { DeviceType = DeviceType.CircuitBreaker, LibrarySymbolName = "CB" };
        Assert.Equal("2", cb.EffectiveALCType);
    }

    [Fact]
    public void SymbolElement_EffectiveALCType_Disconnector_Is7()
    {
        var ds = new SymbolElement { DeviceType = DeviceType.Disconnector, LibrarySymbolName = "DS" };
        Assert.Equal("7", ds.EffectiveALCType);
    }

    [Fact]
    public void SymbolElement_EffectiveALCType_Transformer_Is4()
    {
        var tr = new SymbolElement { DeviceType = DeviceType.Transformer, LibrarySymbolName = "TR" };
        Assert.Equal("4", tr.EffectiveALCType);
    }

    [Fact]
    public void SymbolElement_EffectiveWidth_DefaultsFromDeviceType()
    {
        var cb = new SymbolElement { DeviceType = DeviceType.CircuitBreaker };
        var ds = new SymbolElement { DeviceType = DeviceType.Disconnector };
        var tr = new SymbolElement { DeviceType = DeviceType.Transformer };
        Assert.Equal(32, cb.EffectiveWidth);
        Assert.Equal(24, ds.EffectiveWidth);
        Assert.Equal(60, tr.EffectiveWidth);
    }
}
