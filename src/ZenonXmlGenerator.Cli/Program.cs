using System;
using System.IO;
using System.Text.Json;
using ZenonXmlGenerator;

namespace ZenonXmlGenerator.Cli;

public static class Program
{
    public const int ExitSuccess = 0;
    public const int ExitInvalidArgs = 1;
    public const int ExitFileNotFound = 2;
    public const int ExitParseError = 3;
    public const int ExitGenerationError = 4;

    public static int Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("-h") || args.Contains("--help"))
        {
            PrintUsage();
            return args.Length == 0 ? ExitInvalidArgs : ExitSuccess;
        }

        string? inputPath = null;
        string? outputPath = null;
        string? screenName = null;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg.Equals("--input", StringComparison.OrdinalIgnoreCase) || arg.Equals("-i", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length) inputPath = args[++i];
            }
            else if (arg.Equals("--output", StringComparison.OrdinalIgnoreCase) || arg.Equals("-o", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length) outputPath = args[++i];
            }
            else if (arg.Equals("--screen-name", StringComparison.OrdinalIgnoreCase) || arg.Equals("-s", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length) screenName = args[++i];
            }
            else
            {
                Console.Error.WriteLine($"[Error] 알 수 없거나 잘못된 명령줄 옵션입니다: '{arg}'");
                PrintUsage();
                return ExitInvalidArgs;
            }
        }

        if (string.IsNullOrWhiteSpace(inputPath))
        {
            Console.Error.WriteLine("[Error] 필수 인자 '--input <path>'가 지정되지 않았습니다.");
            return ExitInvalidArgs;
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            Console.Error.WriteLine("[Error] 필수 인자 '--output <path>'가 지정되지 않았습니다.");
            return ExitInvalidArgs;
        }

        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"[Error] 입력 파일이 존재하지 않습니다: '{inputPath}'");
            return ExitFileNotFound;
        }

        string jsonContent;
        try
        {
            jsonContent = File.ReadAllText(inputPath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Error] 입력 파일 읽기 실패: {ex.Message}");
            return ExitFileNotFound;
        }

        var generator = new ZenonXmlGenerator();
        try
        {
            if (!string.IsNullOrWhiteSpace(screenName))
            {
                var doc = JsonSerializer.Deserialize<Models.TopologyDocument>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                });

                if (doc == null)
                {
                    Console.Error.WriteLine("[Error] Topology JSON 역직렬화 결과가 null입니다.");
                    return ExitParseError;
                }

                doc.ScreenName = screenName;
                var bytes = generator.GenerateFromDocument(doc);
                var dir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllBytes(outputPath, bytes);
            }
            else
            {
                var dir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                generator.GenerateToFile(jsonContent, outputPath);
            }
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"[Error] JSON 구문 분석 실패: {ex.Message}");
            return ExitParseError;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Error] zenon XML 생성 실패: {ex.Message}");
            return ExitGenerationError;
        }

        var fi = new FileInfo(outputPath);
        Console.WriteLine($"[Success] zenon Screen XML 변환 완료!");
        Console.WriteLine($" - 입력: {Path.GetFullPath(inputPath)}");
        Console.WriteLine($" - 출력: {Path.GetFullPath(outputPath)} ({fi.Length} bytes, UTF-16 LE BOM)");
        if (!string.IsNullOrWhiteSpace(screenName))
        {
            Console.WriteLine($" - ScreenName 재정의: {screenName}");
        }

        return ExitSuccess;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("==================================================================");
        Console.WriteLine(" zenon-gen : Zenon Engineering Studio 15 Screen XML Generator CLI");
        Console.WriteLine("==================================================================");
        Console.WriteLine("사용법:");
        Console.WriteLine("  zenon-gen --input <path_to_topology.json> --output <path_to_screen.xml> [--screen-name <name>]");
        Console.WriteLine();
        Console.WriteLine("옵션:");
        Console.WriteLine("  -i, --input <path>         입력 Topology JSON 파일 경로 (필수)");
        Console.WriteLine("  -o, --output <path>        출력 zenon Screen XML 파일 경로 (필수)");
        Console.WriteLine("  -s, --screen-name <name>   화면명(ScreenName/Title) 재정의 (선택)");
        Console.WriteLine("  -h, --help                 도움말 표시");
        Console.WriteLine();
        Console.WriteLine("반환 코드(Exit Codes):");
        Console.WriteLine($"  {ExitSuccess} : 정상 완료 (Success)");
        Console.WriteLine($"  {ExitInvalidArgs} : 잘못된 명령줄 인자 (Invalid Arguments)");
        Console.WriteLine($"  {ExitFileNotFound} : 입력 파일 미존재 (File Not Found)");
        Console.WriteLine($"  {ExitParseError} : JSON 파싱 실패 (JSON Parse Error)");
        Console.WriteLine($"  {ExitGenerationError} : XML 생성 오류 (Generation Error)");
    }
}
