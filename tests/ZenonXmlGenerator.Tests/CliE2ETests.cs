using System;
using System.IO;
using System.Xml;
using ZenonXmlGenerator.Cli;

namespace ZenonXmlGenerator.Tests;

/// <summary>
/// CLI 도구(zenon-gen)의 E2E 실행 및 에러 처리 검증 테스트.
/// </summary>
public sealed class CliE2ETests
{
    private static readonly string SampleSldJsonPath = Path.Combine(
        AppContext.BaseDirectory, "Samples", "sample_sld_topology.json");

    private static readonly string SampleHvdcJsonPath = Path.Combine(
        AppContext.BaseDirectory, "Samples", "hvdc_station1_topology.json");

    private static readonly string SampleHvdcFullJsonPath = Path.Combine(
        AppContext.BaseDirectory, "Samples", "hvdc_full_system_topology.json");

    private static readonly string SampleJsonPath = Path.Combine(
        AppContext.BaseDirectory, "Samples", "sample_topology.json");

    [Fact]
    public void Cli_Help_ReturnsSuccess()
    {
        var exitCode = Program.Main(["--help"]);
        Assert.Equal(Program.ExitSuccess, exitCode);
    }

    [Fact]
    public void Cli_NoArgs_ReturnsInvalidArgs()
    {
        var exitCode = Program.Main([]);
        Assert.Equal(Program.ExitInvalidArgs, exitCode);
    }

    [Fact]
    public void Cli_MissingInputArg_ReturnsInvalidArgs()
    {
        var exitCode = Program.Main(["--output", "test.xml"]);
        Assert.Equal(Program.ExitInvalidArgs, exitCode);
    }

    [Fact]
    public void Cli_MissingOutputArg_ReturnsInvalidArgs()
    {
        var exitCode = Program.Main(["--input", SampleSldJsonPath]);
        Assert.Equal(Program.ExitInvalidArgs, exitCode);
    }

    [Fact]
    public void Cli_NonExistentInputFile_ReturnsFileNotFound()
    {
        var exitCode = Program.Main([
            "--input", "non_existent_file_xyz.json",
            "--output", "test.xml"
        ]);
        Assert.Equal(Program.ExitFileNotFound, exitCode);
    }

    [Fact]
    public void Cli_InvalidJsonSyntax_ReturnsParseError()
    {
        var tempJson = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempJson, "{ invalid json content }");
            var tempXml = Path.ChangeExtension(tempJson, ".xml");

            var exitCode = Program.Main([
                "--input", tempJson,
                "--output", tempXml
            ]);
            Assert.Equal(Program.ExitParseError, exitCode);
        }
        finally
        {
            if (File.Exists(tempJson)) File.Delete(tempJson);
        }
    }

    [Fact]
    public void Cli_GenerateSldXml_SucceedsAndProducesValidXml()
    {
        var tempXml = Path.Combine(Path.GetTempPath(), $"cli_test_sld_{Guid.NewGuid():N}.xml");
        try
        {
            var exitCode = Program.Main([
                "--input", SampleSldJsonPath,
                "--output", tempXml
            ]);

            Assert.Equal(Program.ExitSuccess, exitCode);
            Assert.True(File.Exists(tempXml));

            // Validate UTF-16 LE BOM
            var bytes = File.ReadAllBytes(tempXml);
            Assert.True(bytes.Length >= 4);
            Assert.Equal(0xFF, bytes[0]);
            Assert.Equal(0xFE, bytes[1]);

            // Validate XML structure
            var doc = new XmlDocument();
            using (var ms = new MemoryStream(bytes))
            {
                doc.Load(ms);
            }

            var picture = doc.DocumentElement!.SelectSingleNode("Apartment/Picture");
            Assert.NotNull(picture);
            Assert.Equal("Substation_154kV_SLD", picture.Attributes!["ShortName"]!.Value);
            Assert.Equal("0", picture.SelectSingleNode("Type")!.InnerText);
            Assert.Equal("MAIN", picture.SelectSingleNode("Template")!.InnerText);
            Assert.Equal("TRUE", picture.SelectSingleNode("SizeFromTemplate")!.InnerText);
        }
        finally
        {
            if (File.Exists(tempXml)) File.Delete(tempXml);
        }
    }

    [Fact]
    public void Cli_GenerateHvdcStationXml_SucceedsAndProducesValidXml()
    {
        var tempXml = Path.Combine(Path.GetTempPath(), $"cli_test_hvdc_{Guid.NewGuid():N}.xml");
        try
        {
            var exitCode = Program.Main([
                "--input", SampleHvdcJsonPath,
                "--output", tempXml,
                "--screen-name", "HVDC_STATION1_SLD"
            ]);

            Assert.Equal(Program.ExitSuccess, exitCode);
            Assert.True(File.Exists(tempXml));

            // Validate UTF-16 LE BOM
            var bytes = File.ReadAllBytes(tempXml);
            Assert.True(bytes.Length >= 4);
            Assert.Equal(0xFF, bytes[0]);
            Assert.Equal(0xFE, bytes[1]);

            // Validate XML structure
            var doc = new XmlDocument();
            using (var ms = new MemoryStream(bytes))
            {
                doc.Load(ms);
            }

            var picture = doc.DocumentElement!.SelectSingleNode("Apartment/Picture");
            Assert.NotNull(picture);
            Assert.Equal("HVDC_STATION1_SLD", picture.Attributes!["ShortName"]!.Value);
            Assert.Equal("HVDC_STATION1_SLD", picture.SelectSingleNode("Title")!.InnerText);
            Assert.Equal("MAIN", picture.SelectSingleNode("Template")!.InnerText);
            Assert.Equal("0", picture.SelectSingleNode("Type")!.InnerText);
            Assert.Equal("TRUE", picture.SelectSingleNode("SizeFromTemplate")!.InnerText);

            // Validate Bay frame transparent properties (Elements_0)
            var bayAc = doc.DocumentElement!.SelectSingleNode("Apartment/Picture/Elements_0");
            Assert.NotNull(bayAc);
            Assert.Equal("102", bayAc.Attributes!["TYPE"]!.Value);
            Assert.Equal("0", bayAc.SelectSingleNode("AlphaBackColor")!.InnerText);
            Assert.Equal("0", bayAc.SelectSingleNode("FillPattern")!.InnerText);
            Assert.Equal("1", bayAc.SelectSingleNode("LineWidth")!.InnerText);
            Assert.Equal("5C6C75", bayAc.SelectSingleNode("LineColorEx")!.InnerText);
        }
        finally
        {
            if (File.Exists(tempXml)) File.Delete(tempXml);
        }
    }

    [Fact]
    public void Cli_GenerateHvdcFullBipoleXml_SucceedsAndProducesValidXml()
    {
        var tempXml = Path.Combine(Path.GetTempPath(), $"cli_test_hvdc_full_{Guid.NewGuid():N}.xml");
        try
        {
            var exitCode = Program.Main([
                "--input", SampleHvdcFullJsonPath,
                "--output", tempXml,
                "--screen-name", "HVDC_FULL_BIPOLE_SLD"
            ]);

            Assert.Equal(Program.ExitSuccess, exitCode);
            Assert.True(File.Exists(tempXml));

            // Validate UTF-16 LE BOM
            var bytes = File.ReadAllBytes(tempXml);
            Assert.True(bytes.Length >= 4);
            Assert.Equal(0xFF, bytes[0]);
            Assert.Equal(0xFE, bytes[1]);

            // Validate XML structure
            var doc = new XmlDocument();
            using (var ms = new MemoryStream(bytes))
            {
                doc.Load(ms);
            }

            var picture = doc.DocumentElement!.SelectSingleNode("Apartment/Picture");
            Assert.NotNull(picture);
            Assert.Equal("HVDC_FULL_BIPOLE_SLD", picture.Attributes!["ShortName"]!.Value);
            Assert.Equal("MAIN", picture.SelectSingleNode("Template")!.InnerText);
            Assert.Equal("0", picture.SelectSingleNode("Type")!.InnerText);
            Assert.Equal("FALSE", picture.SelectSingleNode("SizeFromTemplate")!.InnerText);
            Assert.Equal("07101C", picture.SelectSingleNode("BackgroundColor")!.InnerText);

            // Validate ST1 positive pole DC busbar is present (Busbar line TYPE=101, lineWidth=5 or 6, ALCUseColor=TRUE)
            var dcPosBus = doc.DocumentElement!.SelectSingleNode(
                "//*[@TYPE='101' and ALCUseColor]");
            Assert.NotNull(dcPosBus);

            // Validate first frame element is transparent rectangle (Elements_0: ST1 AC YARD frame)
            var firstRect = doc.DocumentElement!.SelectSingleNode("Apartment/Picture/Elements_0");
            Assert.NotNull(firstRect);
            Assert.Equal("102", firstRect.Attributes!["TYPE"]!.Value);
            Assert.Equal("0", firstRect.SelectSingleNode("AlphaBackColor")!.InnerText);
            Assert.Equal("0", firstRect.SelectSingleNode("FillPattern")!.InnerText);

            // Vector renderer: no TYPE=16 library symbols should be present
            var type16 = doc.DocumentElement!.SelectNodes("Apartment/Picture/*[@TYPE='16']");
            Assert.NotNull(type16);
            Assert.Equal(0, type16.Count);

            // Validate that vector circles (TYPE=103, from Transformer/EarthSwitch) exist
            var circles = doc.DocumentElement!.SelectNodes("Apartment/Picture/*[@TYPE='103']");
            Assert.NotNull(circles);
            Assert.True(circles.Count > 0, "Expected TYPE=103 circles from Transformer/EarthSwitch vector rendering.");

            // Validate that solid filled vector rectangles (TYPE=102, FillPattern=6) exist (CB/MMC)
            var cbRects = doc.DocumentElement!.SelectNodes(
                "Apartment/Picture/*[@TYPE='102' and FillPattern[text()='6']]");
            Assert.NotNull(cbRects);
            Assert.True(cbRects.Count > 0, "Expected solid filled TYPE=102 rectangles from CB/MMC vector rendering.");
        }
        finally
        {
            if (File.Exists(tempXml)) File.Delete(tempXml);
        }
    }

    [Fact]
    public void Cli_OverrideScreenName_Succeeds()
    {
        const string customName = "Custom_Substation_01";
        var tempXml = Path.Combine(Path.GetTempPath(), $"cli_test_override_{Guid.NewGuid():N}.xml");
        try
        {
            var exitCode = Program.Main([
                "-i", SampleJsonPath,
                "-o", tempXml,
                "--screen-name", customName
            ]);

            Assert.Equal(Program.ExitSuccess, exitCode);
            Assert.True(File.Exists(tempXml));

            var doc = new XmlDocument();
            using (var ms = new MemoryStream(File.ReadAllBytes(tempXml)))
            {
                doc.Load(ms);
            }

            var picture = doc.DocumentElement!.SelectSingleNode("Apartment/Picture");
            Assert.NotNull(picture);
            Assert.Equal(customName, picture.Attributes!["ShortName"]!.Value);
            Assert.Equal(customName, picture.SelectSingleNode("Title")!.InnerText);
        }
        finally
        {
            if (File.Exists(tempXml)) File.Delete(tempXml);
        }
    }

    [Fact]
    public void Cli_SelfValidation_StrictSchemaRules_AllElementsVerified()
    {
        var tempXml = Path.Combine(Path.GetTempPath(), $"cli_test_selfval_{Guid.NewGuid():N}.xml");
        try
        {
            var exitCode = Program.Main([
                "--input", SampleHvdcFullJsonPath,
                "--output", tempXml,
                "--screen-name", "HVDC_FULL_BIPOLE_SLD"
            ]);

            Assert.Equal(Program.ExitSuccess, exitCode);

            var doc = new XmlDocument();
            using (var ms = new MemoryStream(File.ReadAllBytes(tempXml)))
            {
                doc.Load(ms);
            }

            var picture = doc.DocumentElement!.SelectSingleNode("Apartment/Picture");
            Assert.NotNull(picture);

            // 1. Text elements: cloned from Golden.XML (TextColor valid non-black RGB, FillStyle, BackColor=C0C0C0, AlphaBackColor=0)
            var texts = picture.SelectNodes("*[@TYPE='107']");
            Assert.NotNull(texts);
            Assert.True(texts.Count > 0);
            foreach (XmlNode t in texts)
            {
                var tc = t.SelectSingleNode("TextColor")!.InnerText;
                Assert.Equal(6, tc.Length);
                Assert.NotEqual("000000", tc);
                Assert.Equal("C0C0C0", t.SelectSingleNode("BackColor")!.InnerText);
                Assert.Equal("0", t.SelectSingleNode("AlphaBackColor")!.InnerText);
                Assert.NotNull(t.SelectSingleNode("FillStyle"));
            }

            // 2. Solid CircuitBreaker: FillPattern 6, BackColor 00C853 (RGB), AlphaBackColor 0, LineColorEx FFFFFF
            var cbs = picture.SelectNodes("*[@TYPE='102' and Width[text()='32'] and Height[text()='32']]");
            Assert.NotNull(cbs);
            Assert.True(cbs.Count > 0);
            foreach (XmlNode cb in cbs)
            {
                Assert.Equal("6", cb.SelectSingleNode("FillPattern")!.InnerText);
                Assert.Equal("0", cb.SelectSingleNode("AlphaBackColor")!.InnerText);
                Assert.Equal("00C853", cb.SelectSingleNode("BackColor")!.InnerText);
                Assert.Equal("FFFFFF", cb.SelectSingleNode("LineColorEx")!.InnerText);
            }

            // 3. Lines: LineColorEx must be valid 6-char RGB hex and not 000000
            var lines = picture.SelectNodes("*[@TYPE='101']");
            Assert.NotNull(lines);
            Assert.True(lines.Count > 0);
            foreach (XmlNode l in lines)
            {
                var lc = l.SelectSingleNode("LineColorEx")!.InnerText;
                Assert.Equal(6, lc.Length);
                Assert.NotEqual("000000", lc);
            }
        }
        finally
        {
            if (File.Exists(tempXml)) File.Delete(tempXml);
        }
    }
}
