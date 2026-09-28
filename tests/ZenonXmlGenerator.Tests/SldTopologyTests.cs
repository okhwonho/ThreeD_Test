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

    // ─── 3440x1440 Layout Redesign & Orthogonal Routing Tests ────────────

    [Fact]
    public void TopologyDocument_DefaultResolution_Is3440x1440()
    {
        var doc = new TopologyDocument();
        Assert.Equal(3440, doc.Width);
        Assert.Equal(1440, doc.Height);
    }

    [Fact]
    public void HvdcFullSystem_Resolution_Is3440x1440_InJsonAndXml()
    {
        var fullJsonPath = Path.Combine("Samples", "hvdc_full_system_topology.json");
        var jsonText = File.ReadAllText(fullJsonPath);
        var gen = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromJson(jsonText);
        var xml = LoadXml(bytes);

        var picture = xml.DocumentElement!.SelectSingleNode("Apartment/Picture");
        Assert.NotNull(picture);
        Assert.Equal("3440", picture.SelectSingleNode("Width")!.InnerText);
        Assert.Equal("1440", picture.SelectSingleNode("Height")!.InnerText);
    }

    [Fact]
    public void HvdcFullSystem_AllElements_BottomWithin1400()
    {
        var fullJsonPath = Path.Combine("Samples", "hvdc_full_system_topology.json");
        var jsonText = File.ReadAllText(fullJsonPath);
        var doc = System.Text.Json.JsonSerializer.Deserialize<TopologyDocument>(jsonText)!;

        foreach (var elem in doc.Elements)
        {
            int bottom = elem switch
            {
                RectangleElement r => r.Y + r.Height,
                LineElement l      => Math.Max(l.Y1, l.Y2),
                SymbolElement s    => (s.CenterY ?? (s.Y + s.Height / 2)) + Math.Max(s.Height, 32) / 2,
                TextElement t      => t.Y + t.EffectiveHeight,
                _                  => 0,
            };
            Assert.True(bottom <= 1400,
                $"Element '{elem.Id}' ({elem.GetType().Name}) exceeds bottom boundary: bottom={bottom} > 1400");
        }
    }

    [Fact]
    public void HvdcFullSystem_AllLines_StrictlyOrthogonal()
    {
        var fullJsonPath = Path.Combine("Samples", "hvdc_full_system_topology.json");
        var jsonText = File.ReadAllText(fullJsonPath);
        var doc = System.Text.Json.JsonSerializer.Deserialize<TopologyDocument>(jsonText)!;

        foreach (var elem in doc.Elements)
        {
            if (elem is LineElement line)
            {
                bool isOrthogonal = line.X1 == line.X2 || line.Y1 == line.Y2;
                Assert.True(isOrthogonal,
                    $"Line '{line.Id}' is diagonal: ({line.X1},{line.Y1}) -> ({line.X2},{line.Y2})");
            }
        }
    }

    [Fact]
    public void HvdcFullSystem_ElectricalContinuity_EverySymbolTouchesLineEndpoint()
    {
        var fullJsonPath = Path.Combine("Samples", "hvdc_full_system_topology.json");
        var jsonText = File.ReadAllText(fullJsonPath);
        var doc = System.Text.Json.JsonSerializer.Deserialize<TopologyDocument>(jsonText)!;

        var lineEndpoints = new System.Collections.Generic.HashSet<(int, int)>();
        foreach (var elem in doc.Elements)
        {
            if (elem is LineElement line)
            {
                lineEndpoints.Add((line.X1, line.Y1));
                lineEndpoints.Add((line.X2, line.Y2));
            }
        }

        int symbolCount = 0;
        foreach (var elem in doc.Elements)
        {
            if (elem is SymbolElement sym)
            {
                symbolCount++;
                int cx = sym.CenterX ?? (sym.X + sym.Width / 2);
                int cy = sym.CenterY ?? (sym.Y + sym.Height / 2);

                Assert.True(lineEndpoints.Contains((cx, cy)),
                    $"Floating symbol detected: '{sym.Id}' ({sym.DeviceType}, tag={sym.TagLabel}) at ({cx},{cy}) does not touch any line endpoint.");
            }
        }
        Assert.True(symbolCount >= 30, $"Expected >=30 symbols, found {symbolCount}");
    }

    [Fact]
    public void HvdcFullSystem_YAxisRedistribution_FollowsSpecification()
    {
        var fullJsonPath = Path.Combine("Samples", "hvdc_full_system_topology.json");
        var jsonText = File.ReadAllText(fullJsonPath);
        var doc = System.Text.Json.JsonSerializer.Deserialize<TopologyDocument>(jsonText)!;

        // Positive pole devices centered at Y=360
        string[] posPoleIds = ["ST1_MMC_POS", "ST1_DCR_POS", "ST1_P1_ES", "ST1_PLD_DS_POS",
                               "ST2_PLD_DS_POS", "ST2_P1_ES", "ST2_DCR_POS", "ST2_MMC_POS"];
        foreach (var id in posPoleIds)
        {
            var sym = doc.Elements.Find(e => e.Id == id) as SymbolElement;
            Assert.NotNull(sym);
            Assert.Equal(360, sym.CenterY);
        }

        // DMR switches centered at Y=720
        string[] dmrIds = ["DMR_SW1", "DMR_GND", "DMR_SW2"];
        foreach (var id in dmrIds)
        {
            var sym = doc.Elements.Find(e => e.Id == id) as SymbolElement;
            Assert.NotNull(sym);
            Assert.Equal(720, sym.CenterY);
        }

        // Negative pole devices centered at Y=1080
        string[] negPoleIds = ["ST1_MMC_NEG", "ST1_DCR_NEG", "ST1_N1_ES", "ST1_PLD_DS_NEG",
                               "ST2_PLD_DS_NEG", "ST2_N1_ES", "ST2_DCR_NEG", "ST2_MMC_NEG"];
        foreach (var id in negPoleIds)
        {
            var sym = doc.Elements.Find(e => e.Id == id) as SymbolElement;
            Assert.NotNull(sym);
            Assert.Equal(1080, sym.CenterY);
        }
    }

    [Fact]
    public void HvdcFullSystem_MeteringCardBounds_AllTextsFitComfortablyWithAtLeast10PxMargin()
    {
        var fullJsonPath = Path.Combine("Samples", "hvdc_full_system_topology.json");
        var jsonText = File.ReadAllText(fullJsonPath);
        var doc = System.Text.Json.JsonSerializer.Deserialize<TopologyDocument>(jsonText)!;
        var elemDict = System.Linq.Enumerable.ToDictionary(doc.Elements, e => e.Id);

        var cardGroups = new (string BoxId, string[] TextIds)[]
        {
            ("DC_SPEC_BOX", ["TXT_DC_DIR", "TXT_DC_POWER", "TXT_DC_VDC", "TXT_DC_IDC"]),
            ("DC_POS_MONITOR", ["TXT_DC_POS_MONITOR_V", "TXT_DC_POS_MONITOR_I", "TXT_DC_POS_MONITOR_P"]),
            ("DC_NEG_MONITOR", ["TXT_DC_NEG_MONITOR_V", "TXT_DC_NEG_MONITOR_I", "TXT_DC_NEG_MONITOR_P"]),
            ("ST1_PCC_BOX", ["TXT_ST1_PCC"]),
            ("ST2_PCC_BOX", ["TXT_ST2_PCC"]),
        };

        foreach (var (boxId, textIds) in cardGroups)
        {
            var box = (RectangleElement)elemDict[boxId];
            int boxLeft = box.X;
            int boxTop = box.Y;
            int boxRight = box.X + box.Width;
            int boxBottom = box.Y + box.Height;

            foreach (var textId in textIds)
            {
                var text = (TextElement)elemDict[textId];
                int textLeft = text.X;
                int textTop = text.Y;
                int textRight = text.X + text.EffectiveWidth;
                int textBottom = text.Y + text.EffectiveHeight;

                int marginLeft = textLeft - boxLeft;
                int marginRight = boxRight - textRight;
                int marginTop = textTop - boxTop;
                int marginBottom = boxBottom - textBottom;

                Assert.True(marginLeft >= 10, $"{textId} left margin in {boxId} is {marginLeft} < 10");
                Assert.True(marginRight >= 10, $"{textId} right margin in {boxId} is {marginRight} < 10");
                Assert.True(marginTop >= 10, $"{textId} top margin in {boxId} is {marginTop} < 10");
                Assert.True(marginBottom >= 10, $"{textId} bottom margin in {boxId} is {marginBottom} < 10");
            }
        }
    }
}
