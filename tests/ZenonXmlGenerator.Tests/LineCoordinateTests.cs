using System.Collections.Generic;
using System.IO;
using System.Xml;
using ZenonXmlGenerator;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Tests;

/// <summary>
/// 선(Line) 요소의 좌표 변환 검증 테스트.
/// zenon 규칙:
///   StartX = X1
///   StartY = Y1
///   Width  = X2 - X1 (부호 유지)
///   Height = Y2 - Y1 (부호 유지)
/// Note: Elements_0은 항상 다크 캔버스 전체 배경(CANVAS_BG)이므로,
/// 사용자 지정 요소들은 Elements_1부터 시작한다.
/// </summary>
public sealed class LineCoordinateTests
{
    // ─── 단위 변환 (Unit Conversion) 검증 ───────────────────────────────

    [Theory]
    [InlineData(100, 350, 100, 200, 100, 350, 0, -150)]  // 수직선 위로 (Height < 0)
    [InlineData(100, 200, 100, 350, 100, 200, 0, 150)]   // 수직선 아래로 (Height > 0)
    [InlineData(50,  100, 300, 100, 50,  100, 250, 0)]   // 수평선 오른쪽으로 (Width > 0)
    [InlineData(300, 100, 50,  100, 300, 100, -250, 0)]  // 수평선 왼쪽으로 (Width < 0)
    public void LineElement_CalculatesCoordinatesCorrectly(
        int x1, int y1, int x2, int y2,
        int expectedStartX, int expectedStartY, int expectedWidth, int expectedHeight)
    {
        var line = new LineElement
        {
            Id = "L1",
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
        };

        Assert.Equal(expectedStartX, line.StartX);
        Assert.Equal(expectedStartY, line.StartY);
        Assert.Equal(expectedWidth,  line.Dx);
        Assert.Equal(expectedHeight, line.Dy);
    }

    // ─── XML 출력 검증 ──────────────────────────────────────────────────

    private static XmlElement GetElement(XmlDocument doc, int index)
    {
        var picture = doc.DocumentElement!
            .SelectSingleNode("Apartment/Picture")!;
        return (XmlElement)picture.SelectSingleNode($"Elements_{index}")!;
    }

    private static XmlDocument BuildFromElements(IEnumerable<TopologyElement> elements)
    {
        var topDoc = new TopologyDocument
        {
            ScreenName = "TestScreen",
            Elements   = new List<TopologyElement>(elements),
        };
        var gen    = new ZenonXmlGenerator();
        var bytes  = gen.GenerateFromDocument(topDoc);
        var xmlDoc = new XmlDocument();
        using var ms = new System.IO.MemoryStream(bytes);
        xmlDoc.Load(ms);
        return xmlDoc;
    }

    [Fact]
    public void Xml_VerticalUpLine_CorrectCoordinates()
    {
        var line = new LineElement { Id = "L1", X1 = 100, Y1 = 350, X2 = 100, Y2 = 200 };
        var xml  = BuildFromElements([line]);
        // Elements_0 is CANVAS_BG, Elements_1 is the line
        var ele  = GetElement(xml, 1);

        Assert.Equal("101",  ele.GetAttribute("TYPE"));
        Assert.Equal("100",  ele.SelectSingleNode("StartX")!.InnerText);
        Assert.Equal("350",  ele.SelectSingleNode("StartY")!.InnerText);
        Assert.Equal("0",    ele.SelectSingleNode("Width")!.InnerText);
        Assert.Equal("-150", ele.SelectSingleNode("Height")!.InnerText);
    }

    [Fact]
    public void Xml_HorizontalLine_CorrectCoordinates()
    {
        var line = new LineElement { Id = "L2", X1 = 50, Y1 = 100, X2 = 300, Y2 = 100 };
        var xml  = BuildFromElements([line]);
        var ele  = GetElement(xml, 1);

        Assert.Equal("101", ele.GetAttribute("TYPE"));
        Assert.Equal("50",  ele.SelectSingleNode("StartX")!.InnerText);
        Assert.Equal("100", ele.SelectSingleNode("StartY")!.InnerText);
        Assert.Equal("250", ele.SelectSingleNode("Width")!.InnerText);
        Assert.Equal("0",   ele.SelectSingleNode("Height")!.InnerText);
    }

    [Fact]
    public void Xml_DiagonalLine_SplitIntoTwoOrthogonalSegments()
    {
        // 대각선 (100,100) → (300,300) 은 OrthogonalRouter에 의해 2개의 직교선으로 분할.
        // Elements_0: CANVAS_BG
        // Elements_1: 수평 세그먼트 (100,100) → (300,100) Width=200, Height=0
        // Elements_2: 수직 세그먼트 (300,100) → (300,300) Width=0, Height=200
        var line = new LineElement { Id = "L3", X1 = 100, Y1 = 100, X2 = 300, Y2 = 300 };
        var xml  = BuildFromElements([line]);

        var seg1 = GetElement(xml, 1);
        Assert.Equal("101", seg1.GetAttribute("TYPE"));
        Assert.Equal("100", seg1.SelectSingleNode("StartX")!.InnerText);
        Assert.Equal("100", seg1.SelectSingleNode("StartY")!.InnerText);
        Assert.Equal("200", seg1.SelectSingleNode("Width")!.InnerText);
        Assert.Equal("0",   seg1.SelectSingleNode("Height")!.InnerText);

        var seg2 = GetElement(xml, 2);
        Assert.Equal("101", seg2.GetAttribute("TYPE"));
        Assert.Equal("300", seg2.SelectSingleNode("StartX")!.InnerText);
        Assert.Equal("100", seg2.SelectSingleNode("StartY")!.InnerText);
        Assert.Equal("0",   seg2.SelectSingleNode("Width")!.InnerText);
        Assert.Equal("200", seg2.SelectSingleNode("Height")!.InnerText);
    }

    // ─── Elements_n 태그명 및 NODE 속성 검증 ─────────────────────────

    [Fact]
    public void Xml_LineElement_HasCorrectTagName()
    {
        var line = new LineElement { Id = "L4", X1 = 0, Y1 = 0, X2 = 10, Y2 = 0 };
        var xml  = BuildFromElements([line]);
        var ele  = GetElement(xml, 1);

        Assert.Equal("Elements_1", ele.Name);
    }

    [Fact]
    public void Xml_LineElement_HasNodeEmbeddedObject()
    {
        var line = new LineElement { Id = "L5", X1 = 0, Y1 = 0, X2 = 10, Y2 = 0 };
        var xml  = BuildFromElements([line]);
        var ele  = GetElement(xml, 1);

        Assert.Equal("zenOn(R) embedded object", ele.GetAttribute("NODE"));
    }
}
