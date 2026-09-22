using System;
using System.IO;

var baseDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var testsSamplesDir = Path.Combine(baseDir, "tests", "ZenonXmlGenerator.Tests", "Samples");

var sample1Json = Path.Combine(testsSamplesDir, "sample_topology.json");
var output1Xml  = Path.Combine(baseDir, "output_test_screen.xml");

var sampleSldJson = Path.Combine(testsSamplesDir, "sample_sld_topology.json");
var outputSldXml  = Path.Combine(baseDir, "output_sld_full_test.xml");

var gen = new ZenonXmlGenerator.ZenonXmlGenerator();

// 1. Generate output_test_screen.xml
gen.GenerateToFile(File.ReadAllText(sample1Json), output1Xml);
var info1 = new FileInfo(output1Xml);
Console.WriteLine($"Generated : {output1Xml} ({info1.Length} bytes)");

// 2. Generate output_sld_full_test.xml
gen.GenerateToFile(File.ReadAllText(sampleSldJson), outputSldXml);
var infoSld = new FileInfo(outputSldXml);
var bom = File.ReadAllBytes(outputSldXml)[..4];
Console.WriteLine($"Generated : {outputSldXml} ({infoSld.Length} bytes)");
Console.WriteLine($"BOM bytes : {string.Join(" ", Array.ConvertAll(bom, b => b.ToString("X2")))}");
