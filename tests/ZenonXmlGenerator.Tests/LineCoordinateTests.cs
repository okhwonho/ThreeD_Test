using System.Collections.Generic;
using System.IO;
using System.Xml;
using ZenonXmlGenerator;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Tests;

/// <summary>
/// Line 요소의 좌표 계산 검증.
/// Ground Truth 규칙: StartX=x1, StartY=y1, Width=(x2-x1), Height=(y2-y1)
/// 요소 경로: Subject/Apartment/Picture/Elements_n
/// </summary>
public sealed class LineCoordinateTests
{
    // ─── Model 레벨 단위 테스트 ────────────────────────────────────────

    [Fact]
    public void LineElement_VerticalUp_HasNegativeHeight()
    {
        // 수직 상향선: (100,350) → (100,200)  dy = -150
        var line = new LineElement { X1 = 100, Y1 = 350, X2 = 100, Y2 = 200 };
        Assert.Equal(100,  line.StartX);
        Assert.Equal(350,  line.StartY);
        Assert.Equal(0,    line.Dx);
        Assert.Equal(-150, line.Dy);
    }

    [Fact]
    public void LineElement_Horizontal_HasZeroHeight()
    {
        // 수평선: (50,100) → (300,100)  dx=250, dy=0
        var line = new LineElement { X1 = 50, Y1 = 100, X2 = 300, Y2 = 100 };
        Assert.Equal(50,  line.StartX);
        Assert.Equal(100, line.StartY);
        Assert.Equal(250, line.Dx);
        Assert.Equal(0,   line.Dy);
    }

    [Fact]
    public void LineElement_Diagonal_HasPositiveDxAndDy()
    {
        // 대각선: (100,100) → (300,300)  dx=200, dy=200
        var line = new LineElement { X1 = 100, Y1 = 100, X2 = 300, Y2 = 300 };
        Assert.Equal(200, line.Dx);
        Assert.Equal(200, line.Dy);
    }

    // ─── XML 출력 레벨 통합 테스트 ────────────────────────────────────

    /// <summary>새 계층: Subject/Apartment/Picture/Elements_{index}</summary>
    private static XmlElement GetElement(XmlDocument doc, int index)
    {
        var picture = (XmlElement)doc.DocumentElement!
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
        var ele  = GetElement(xml, 0);

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
        var ele  = GetElement(xml, 0);

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
        // 수평선: (100,100) → (300,100)  Width=200, Height=0
        // 수직선: (300,100) → (300,300)  Width=0,   Height=200
        var line = new LineElement { Id = "L3", X1 = 100, Y1 = 100, X2 = 300, Y2 = 300 };
        var xml  = BuildFromElements([line]);

        // Elements_0: 수평 세그먼트
        var seg1 = GetElement(xml, 0);
        Assert.Equal("101", seg1.GetAttribute("TYPE"));
        Assert.Equal("100", seg1.SelectSingleNode("StartX")!.InnerText);
        Assert.Equal("100", seg1.SelectSingleNode("StartY")!.InnerText);
        Assert.Equal("200", seg1.SelectSingleNode("Width")!.InnerText);
        Assert.Equal("0",   seg1.SelectSingleNode("Height")!.InnerText);

        // Elements_1: 수직 세그먼트
        var seg2 = GetElement(xml, 1);
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
        var ele  = GetElement(xml, 0);

        Assert.Equal("Elements_0", ele.Name);
    }

    [Fact]
    public void Xml_LineElement_HasNodeEmbeddedObject()
    {
        var line = new LineElement { Id = "L5", X1 = 0, Y1 = 0, X2 = 10, Y2 = 0 };
        var xml  = BuildFromElements([line]);
        var ele  = GetElement(xml, 0);

        Assert.Equal("zenOn(R) embedded object", ele.GetAttribute("NODE"));
    }
}
