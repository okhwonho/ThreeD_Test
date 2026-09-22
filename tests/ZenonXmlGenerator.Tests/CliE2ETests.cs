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
            Assert.Equal("2", picture.SelectSingleNode("Type")!.InnerText);
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
            Assert.Equal("Standard", picture.SelectSingleNode("Template")!.InnerText);
            Assert.Equal("2", picture.SelectSingleNode("Type")!.InnerText);
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
}
