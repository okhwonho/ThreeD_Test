using System;
using System.Collections.Generic;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml;

/// <summary>
/// 자체 완결형 벡터 렌더러 (Self-Contained Vector Renderer).
///
/// 외부 심볼 라이브러리(TYPE=16) 의존을 완전히 제거하고,
/// 각 SymbolElement를 zenon 기본 도형(Rectangle/Circle/Line/Text)으로 분해한다.
///
/// 기기 유형별 렌더링 규칙:
/// ┌────────────────────────────────────────────────────────────────┐
/// │ CircuitBreaker   → Rectangle(32×32, FillPattern=8, 녹색)       │
/// │ Disconnector     → Rectangle(24×24, FillPattern=0, 흰색 테두리)│
/// │ EarthSwitch      → Circle(d=24, 녹색) + 수직 인출선 + 접지⏚    │
/// │ Transformer(Y-Δ) → 3×Circle(r=22) 삼각 배치 + Y/Y/Δ 텍스트   │
/// │ CurrentTransformer → Rectangle(20×20) + "CT" 텍스트            │
/// │ PotentialTransformer → Rectangle(20×20) + "PT" 텍스트          │
/// │ Default          → Rectangle(28×28, FillPattern=8, 녹색)       │
/// └────────────────────────────────────────────────────────────────┘
/// </summary>
public static class VectorSymbolRenderer
{
    /// <summary>
    /// 요소 목록에서 SymbolElement를 벡터 프리미티브(Rectangle/Circle/Line/Text)로 확장.
    /// 비(非) SymbolElement 요소는 변경 없이 통과(pass-through).
    /// </summary>
    public static List<TopologyElement> Expand(IEnumerable<TopologyElement> elements)
    {
        var result = new List<TopologyElement>();
        foreach (var element in elements)
        {
            if (element is SymbolElement sym)
                result.AddRange(ExpandSymbol(sym));
            else
                result.Add(element);
        }
        return result;
    }

    // ─── 기기 유형별 벡터 분해 ──────────────────────────────────────────

    private static List<TopologyElement> ExpandSymbol(SymbolElement sym)
    {
        int cx = sym.CenterX ?? (sym.EffectiveX + sym.EffectiveWidth / 2);
        int cy = sym.CenterY ?? (sym.EffectiveY + sym.EffectiveHeight / 2);

        return sym.DeviceType switch
        {
            DeviceType.CircuitBreaker    => RenderCircuitBreaker(sym, cx, cy),
            DeviceType.Disconnector      => RenderDisconnector(sym, cx, cy),
            DeviceType.EarthSwitch       => RenderEarthSwitch(sym, cx, cy),
            DeviceType.Transformer       => RenderTransformer(sym, cx, cy),
            DeviceType.CurrentTransformer  => RenderCT(sym, cx, cy),
            DeviceType.PotentialTransformer => RenderPT(sym, cx, cy),
            _ => RenderDefault(sym, cx, cy),
        };
    }

    // ─── CircuitBreaker: 녹색 채움 사각형 (32×32) ────────────────────────
    private static List<TopologyElement> RenderCircuitBreaker(SymbolElement sym, int cx, int cy)
    {
        int w = XmlConstants.SymbolSizeCB; // 32
        int h = XmlConstants.SymbolSizeCB;
        return
        [
            new RectangleElement
            {
                Id          = sym.Id + "_BODY",
                X           = cx - w / 2,
                Y           = cy - h / 2,
                Width       = w,
                Height      = h,
                FillPattern = XmlConstants.FillPatternSolid,
                FillColor   = XmlConstants.ColorSymbolFill,
                BorderColor = XmlConstants.ColorSymbolBorder,
                LineWidth   = 2,
            },
        ];
    }

    // ─── Disconnector: 빈 사각형 테두리 (24×24) ──────────────────────────
    private static List<TopologyElement> RenderDisconnector(SymbolElement sym, int cx, int cy)
    {
        int w = XmlConstants.SymbolSizeDS; // 24
        int h = XmlConstants.SymbolSizeDS;
        var elems = new List<TopologyElement>
        {
            new RectangleElement
            {
                Id          = sym.Id + "_BODY",
                X           = cx - w / 2,
                Y           = cy - h / 2,
                Width       = w,
                Height      = h,
                FillPattern = 0,             // 빈 사각형
                AlphaBackColor = 0,
                BorderColor = XmlConstants.ColorSymbolBorder,
                LineWidth   = 2,
            },
        };

        // 내부 대각선(블레이드 표시): 좌상 → 우하
        elems.Add(new LineElement
        {
            Id        = sym.Id + "_BLADE",
            X1        = cx - w / 2 + 4,
            Y1        = cy - h / 2 + 4,
            X2        = cx + w / 2 - 4,
            Y2        = cy + h / 2 - 4,
            Color     = XmlConstants.ColorSymbolBorder,
            LineWidth = 1,
        });

        return elems;
    }

    // ─── EarthSwitch: 녹색 원 + 수직 인출선 + 접지 사다리(⏚) ───────────
    private static List<TopologyElement> RenderEarthSwitch(SymbolElement sym, int cx, int cy)
    {
        int r      = XmlConstants.ESCircleRadius;   // 12
        int lead   = XmlConstants.ESLeadLength;      // 30

        // 접지 연결점 (원 하단)
        int botY   = cy + r;
        int gndTop = botY + lead;

        var elems = new List<TopologyElement>
        {
            // ① 녹색 채움 원
            new CircleElement
            {
                Id          = sym.Id + "_CIRCLE",
                CenterX     = cx,
                CenterY     = cy,
                Radius      = r,
                FillColor   = XmlConstants.ColorSymbolFill,
                BorderColor = XmlConstants.ColorSymbolBorder,
                FillPattern = XmlConstants.FillPatternSolid,
            },
            // ② 수직 인출선
            new LineElement
            {
                Id        = sym.Id + "_LEAD",
                X1        = cx,
                Y1        = botY,
                X2        = cx,
                Y2        = gndTop,
                Color     = XmlConstants.ColorWire,
                LineWidth = 2,
            },
            // ③ 접지 사다리 3단 (⏚)
            new LineElement { Id = sym.Id + "_G1", X1 = cx - 14, Y1 = gndTop,     X2 = cx + 14, Y2 = gndTop,     Color = XmlConstants.ColorWire, LineWidth = 2 },
            new LineElement { Id = sym.Id + "_G2", X1 = cx - 9,  Y1 = gndTop + 6, X2 = cx + 9,  Y2 = gndTop + 6, Color = XmlConstants.ColorWire, LineWidth = 2 },
            new LineElement { Id = sym.Id + "_G3", X1 = cx - 4,  Y1 = gndTop + 12,X2 = cx + 4,  Y2 = gndTop + 12,Color = XmlConstants.ColorWire, LineWidth = 2 },
        };
        return elems;
    }

    // ─── Transformer Y-Y-Δ: 3개 원 삼각 배치 + 기호 텍스트 ──────────────
    private static List<TopologyElement> RenderTransformer(SymbolElement sym, int cx, int cy)
    {
        int r = XmlConstants.TRCircleRadius; // 22

        // 3개 원 배치: 상단 좌(Y), 상단 우(Y), 하단 중앙(Δ)
        (int x, int y, string label)[] windings =
        [
            (cx - r,  cy - r, "Y"),
            (cx + r,  cy - r, "Y"),
            (cx,      cy + r, "Δ"),
        ];

        var elems = new List<TopologyElement>();
        foreach (var (wcx, wcy, label) in windings)
        {
            elems.Add(new CircleElement
            {
                Id          = sym.Id + "_" + label + "_COIL",
                CenterX     = wcx,
                CenterY     = wcy,
                Radius      = r,
                FillPattern = 0,                           // 빈 원 (권선 표시)
                BorderColor = XmlConstants.ColorBusbar,   // 적색 테두리
                LineWidth   = 3,
            });

            // 권선 기호 텍스트 (Y/Y/Δ)
            elems.Add(new TextElement
            {
                Id       = sym.Id + "_" + label + "_TXT",
                X        = wcx - 5,
                Y        = wcy - 7,
                Text     = label,
                FontSize = 11,
                Color    = XmlConstants.ColorTextPrimary,
            });
        }
        return elems;
    }

    // ─── CurrentTransformer: 소형 사각형 + "CT" 텍스트 ──────────────────
    private static List<TopologyElement> RenderCT(SymbolElement sym, int cx, int cy)
    {
        int w = 20; int h = 20;
        return
        [
            new RectangleElement
            {
                Id          = sym.Id + "_BODY",
                X           = cx - w / 2,
                Y           = cy - h / 2,
                Width       = w,
                Height      = h,
                FillPattern = XmlConstants.FillPatternSolid,
                FillColor   = XmlConstants.ColorDmrLine,
                BorderColor = XmlConstants.ColorSymbolBorder,
                LineWidth   = 1,
            },
            new TextElement
            {
                Id       = sym.Id + "_LBL",
                X        = cx - 8,
                Y        = cy - 6,
                Text     = "CT",
                FontSize = 8,
                Color    = "#000000",
            },
        ];
    }

    // ─── PotentialTransformer: 소형 사각형 + "PT" 텍스트 ────────────────
    private static List<TopologyElement> RenderPT(SymbolElement sym, int cx, int cy)
    {
        int w = 20; int h = 20;
        return
        [
            new RectangleElement
            {
                Id          = sym.Id + "_BODY",
                X           = cx - w / 2,
                Y           = cy - h / 2,
                Width       = w,
                Height      = h,
                FillPattern = XmlConstants.FillPatternSolid,
                FillColor   = XmlConstants.ColorDmrLine,
                BorderColor = XmlConstants.ColorSymbolBorder,
                LineWidth   = 1,
            },
            new TextElement
            {
                Id       = sym.Id + "_LBL",
                X        = cx - 8,
                Y        = cy - 6,
                Text     = "PT",
                FontSize = 8,
                Color    = "#000000",
            },
        ];
    }

    // ─── Default: 녹색 사각형 (28×28) ────────────────────────────────────
    private static List<TopologyElement> RenderDefault(SymbolElement sym, int cx, int cy)
    {
        int w = XmlConstants.SymbolSizeDefault; // 28
        return
        [
            new RectangleElement
            {
                Id          = sym.Id + "_BODY",
                X           = cx - w / 2,
                Y           = cy - w / 2,
                Width       = w,
                Height      = w,
                FillPattern = XmlConstants.FillPatternSolid,
                FillColor   = XmlConstants.ColorSymbolFill,
                BorderColor = XmlConstants.ColorSymbolBorder,
                LineWidth   = 2,
            },
        ];
    }
}
