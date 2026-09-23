using System.Collections.Generic;
using System.Linq;
using ZenonXmlGenerator.Models;
using ZenonXmlGenerator.Xml;

namespace ZenonXmlGenerator.Tests;

/// <summary>
/// OrthogonalRouter 유틸리티 — 대각선 분할, 직교선 통과, tagLabel 주입 검증.
/// </summary>
public sealed class OrthogonalRouterTests
{
    // ─── OrthogonalRouter.Route 테스트 ──────────────────────────────────

    [Fact]
    public void Route_HorizontalLine_PassesThrough()
    {
        // 수평선은 분할 대상이 아님
        var line = new LineElement { Id = "H", X1 = 0, Y1 = 100, X2 = 500, Y2 = 100 };
        var result = OrthogonalRouter.Route([line]);

        Assert.Single(result);
        Assert.IsType<LineElement>(result[0]);
        var r = (LineElement)result[0];
        Assert.Equal(0,   r.X1); Assert.Equal(100, r.Y1);
        Assert.Equal(500, r.X2); Assert.Equal(100, r.Y2);
    }

    [Fact]
    public void Route_VerticalLine_PassesThrough()
    {
        // 수직선은 분할 대상이 아님
        var line = new LineElement { Id = "V", X1 = 200, Y1 = 50, X2 = 200, Y2 = 400 };
        var result = OrthogonalRouter.Route([line]);

        Assert.Single(result);
    }

    [Fact]
    public void Route_DiagonalLine_SplitsIntoTwoOrthogonalSegments()
    {
        // (100,200) → (400,500)  → H:(100,200)→(400,200) + V:(400,200)→(400,500)
        var line = new LineElement { Id = "D", X1 = 100, Y1 = 200, X2 = 400, Y2 = 500, Color = "#FF0000", LineWidth = 3 };
        var result = OrthogonalRouter.Route([line]);

        Assert.Equal(2, result.Count);

        var seg1 = (LineElement)result[0];
        Assert.Equal("D_H", seg1.Id);
        Assert.Equal(100, seg1.X1); Assert.Equal(200, seg1.Y1);
        Assert.Equal(400, seg1.X2); Assert.Equal(200, seg1.Y2);  // 수평: Y 고정
        Assert.False(seg1.IsDiagonal);
        Assert.Equal("#FF0000", seg1.Color);

        var seg2 = (LineElement)result[1];
        Assert.Equal("D_V", seg2.Id);
        Assert.Equal(400, seg2.X1); Assert.Equal(200, seg2.Y1);
        Assert.Equal(400, seg2.X2); Assert.Equal(500, seg2.Y2);  // 수직: X 고정
        Assert.False(seg2.IsDiagonal);
    }

    [Fact]
    public void Route_DiagonalLine_PreservesColorAndLineWidth()
    {
        var line = new LineElement { Id = "D2", X1 = 0, Y1 = 0, X2 = 300, Y2 = 200, Color = "#0000FF", LineWidth = 6 };
        var result = OrthogonalRouter.Route([line]);

        Assert.Equal(2, result.Count);
        foreach (var seg in result.Cast<LineElement>())
        {
            Assert.Equal("#0000FF", seg.Color);
            Assert.Equal(6,         seg.LineWidth);
        }
    }

    [Fact]
    public void Route_NonLineElement_PassesThrough()
    {
        // 비(非) LineElement는 변경 없이 통과
        var rect = new RectangleElement { Id = "R", X = 10, Y = 10, Width = 100, Height = 50 };
        var text = new TextElement { Id = "T", X = 20, Y = 20, Text = "Hello" };
        var result = OrthogonalRouter.Route([rect, text]);

        Assert.Equal(2, result.Count);
        Assert.Same(rect, result[0]);
        Assert.Same(text, result[1]);
    }

    [Fact]
    public void Route_MixedElements_OnlyDiagonalLinesAreSplit()
    {
        var hLine = new LineElement { Id = "H", X1 = 0, Y1 = 50, X2 = 200, Y2 = 50 }; // 수평
        var dLine = new LineElement { Id = "D", X1 = 0, Y1 = 0,  X2 = 100, Y2 = 100 }; // 대각선
        var sym   = new SymbolElement { Id = "S", X = 10, Y = 10, Width = 32, Height = 32, LibrarySymbolName = "CB" };

        var result = OrthogonalRouter.Route([hLine, dLine, sym]);

        // 수평선(1) + 대각선 분할(2) + 심볼(1) = 4
        Assert.Equal(4, result.Count);
        Assert.IsType<LineElement>(result[0]);  // 수평선 그대로
        Assert.IsType<LineElement>(result[1]);  // 대각선_H
        Assert.IsType<LineElement>(result[2]);  // 대각선_V
        Assert.IsType<SymbolElement>(result[3]); // 심볼 그대로
    }

    // ─── LineElement.IsDiagonal 및 EffectiveLineWidth 단위 테스트 ─────

    [Fact]
    public void LineElement_IsDiagonal_TrueForDiagonalLine()
    {
        var line = new LineElement { X1 = 0, Y1 = 0, X2 = 100, Y2 = 100 };
        Assert.True(line.IsDiagonal);
    }

    [Fact]
    public void LineElement_IsDiagonal_FalseForHorizontalLine()
    {
        var line = new LineElement { X1 = 0, Y1 = 50, X2 = 200, Y2 = 50 };
        Assert.False(line.IsDiagonal);
    }

    [Fact]
    public void LineElement_IsDiagonal_FalseForVerticalLine()
    {
        var line = new LineElement { X1 = 100, Y1 = 0, X2 = 100, Y2 = 300 };
        Assert.False(line.IsDiagonal);
    }

    [Fact]
    public void LineElement_EffectiveLineWidth_BusbarGets12()
    {
        var busbar = new LineElement { DeviceType = DeviceType.Busbar };
        Assert.Equal(12, busbar.EffectiveLineWidth);
    }

    [Fact]
    public void LineElement_EffectiveLineWidth_DefaultConnectionGets3()
    {
        var conn = new LineElement { LineWidth = 1 }; // 기본값 1
        Assert.Equal(3, conn.EffectiveLineWidth);
    }

    [Fact]
    public void LineElement_EffectiveLineWidth_ExplicitValueTakesPriority()
    {
        var line = new LineElement { LineWidth = 6 };
        Assert.Equal(6, line.EffectiveLineWidth);
    }

    // ─── SymbolElement.EffectiveWidth/Height 단위 테스트 ────────────────

    [Fact]
    public void SymbolElement_EffectiveWidth_CircuitBreakerDefaultIs32()
    {
        var cb = new SymbolElement { DeviceType = DeviceType.CircuitBreaker, LibrarySymbolName = "CB" };
        Assert.Equal(32, cb.EffectiveWidth);
        Assert.Equal(32, cb.EffectiveHeight);
    }

    [Fact]
    public void SymbolElement_EffectiveWidth_DisconnectorDefaultIs24()
    {
        var ds = new SymbolElement { DeviceType = DeviceType.Disconnector, LibrarySymbolName = "DS" };
        Assert.Equal(24, ds.EffectiveWidth);
        Assert.Equal(24, ds.EffectiveHeight);
    }

    [Fact]
    public void SymbolElement_EffectiveWidth_TransformerDefaultIs60()
    {
        var tr = new SymbolElement { DeviceType = DeviceType.Transformer, LibrarySymbolName = "TR" };
        Assert.Equal(60, tr.EffectiveWidth);
        Assert.Equal(60, tr.EffectiveHeight);
    }

    [Fact]
    public void SymbolElement_EffectiveWidth_ExplicitValueOverridesDefault()
    {
        var cb = new SymbolElement { DeviceType = DeviceType.CircuitBreaker, Width = 48, Height = 48, LibrarySymbolName = "CB" };
        Assert.Equal(48, cb.EffectiveWidth);
        Assert.Equal(48, cb.EffectiveHeight);
    }

    [Fact]
    public void SymbolElement_EffectiveX_SnapsToCenterX_UsingEffectiveWidth()
    {
        // CircuitBreaker (32×32), centerX=200 → EffectiveX = 200 - 16 = 184
        var cb = new SymbolElement
        {
            DeviceType        = DeviceType.CircuitBreaker,
            CenterX           = 200,
            CenterY           = 300,
            LibrarySymbolName = "CB"
        };
        Assert.Equal(184, cb.EffectiveX);
        Assert.Equal(284, cb.EffectiveY);
    }
}
