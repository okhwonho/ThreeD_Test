using System;
using System.IO;

var jsonPath   = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..",
                              "tests", "ZenonXmlGenerator.Tests", "Samples", "sample_topology.json");
var outputPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..",
                              "output_test_screen.xml");

jsonPath   = Path.GetFullPath(jsonPath);
outputPath = Path.GetFullPath(outputPath);

var json = File.ReadAllText(jsonPath);
var gen  = new ZenonXmlGenerator.ZenonXmlGenerator();
gen.GenerateToFile(json, outputPath);

var info  = new FileInfo(outputPath);
var bom   = File.ReadAllBytes(outputPath)[..4];
Console.WriteLine($"Generated : {outputPath}");
Console.WriteLine($"File size : {info.Length} bytes");
Console.WriteLine($"BOM bytes : {string.Join(" ", Array.ConvertAll(bom, b => b.ToString("X2")))}");
