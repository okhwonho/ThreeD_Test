using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ZenonXmlGenerator.Models;
using ZenonXmlGenerator.Xml.ElementWriters;

namespace ZenonXmlGenerator.Xml;

/// <summary>
/// TopologyDocument → zenon Screen XML 변환 핵심 엔진.
///
/// 출력 계층 구조 (zenon 15 Ground Truth):
/// <code>
/// &lt;Subject ShortName="zenOn(R) exported project" MainVersion="15000"&gt;
///   &lt;Apartment ShortName="zenOn(R) pictures list" Version="15000"&gt;
///     &lt;Picture ShortName="{screenName}"&gt;
///       &lt;Title&gt;{screenName}&lt;/Title&gt;
///       &lt;Template&gt;{template}&lt;/Template&gt;
///       &lt;Type&gt;0&lt;/Type&gt;
///       &lt;SizeFromTemplate&gt;TRUE&lt;/SizeFromTemplate&gt;
///       &lt;Width&gt;…&lt;/Width&gt;
///       &lt;Height&gt;…&lt;/Height&gt;
///       &lt;BackgroundColor&gt;80000037&lt;/BackgroundColor&gt;
///       &lt;Elements_0 NODE="zenOn(R) embedded object" TYPE="101"&gt;…&lt;/Elements_0&gt;
///       …
///     &lt;/Picture&gt;
///   &lt;/Apartment&gt;
/// &lt;/Subject&gt;
/// </code>
/// 인코딩: UTF-16 LE BOM (0xFF 0xFE)
/// 요소 태그명: Elements_0, Elements_1, …
/// </summary>
public sealed class ZenonXmlBuilder
{
    private readonly Dictionary<Type, IElementWriter> _writers;

    /// <summary>기본 Writer 레지스트리로 빌더를 초기화한다.</summary>
    public ZenonXmlBuilder()
    {
        _writers = new Dictionary<Type, IElementWriter>
        {
            [typeof(LineElement)]      = new LineWriter(),
            [typeof(RectangleElement)] = new RectangleWriter(),
            [typeof(TextElement)]      = new ElementWriters.TextWriter(),
            [typeof(CircleElement)]    = new CircleWriter(),
            // SymbolElement 는 VectorSymbolRenderer에서 프리미티브로 분해 후 처리.
            // SymbolWriter(TYPE=16) 는 외부 라이브러리 의존이므로 레지스트리에서 제거.
        };
    }

    /// <summary>
    /// <paramref name="document"/>를 zenon XML로 변환하여 UTF-16 BOM 바이트 배열로 반환.
    /// </summary>
    public byte[] Build(TopologyDocument document)
    {
        using var memoryStream = new MemoryStream();
        // UTF-16 LE BOM: StreamWriter가 BOM을 먼저 씀
        using var streamWriter = new StreamWriter(
            memoryStream, new UnicodeEncoding(bigEndian: false, byteOrderMark: true));

        var settings = new XmlWriterSettings
        {
            Encoding           = new UnicodeEncoding(bigEndian: false, byteOrderMark: true),
            Indent             = true,
            IndentChars        = "  ",
            OmitXmlDeclaration = false,
            CloseOutput        = false,
        };

        using var xmlWriter = XmlWriter.Create(streamWriter, settings);
        WriteDocument(xmlWriter, document);
        xmlWriter.Flush();
        streamWriter.Flush();

        return memoryStream.ToArray();
    }

    /// <summary>
    /// <paramref name="document"/>를 zenon XML로 변환하여 <paramref name="filePath"/>에 저장.
    /// </summary>
    public void BuildToFile(TopologyDocument document, string filePath)
    {
        var bytes = Build(document);
        File.WriteAllBytes(filePath, bytes);
    }

    // ─── Private helpers ────────────────────────────────────────────────

    private void WriteDocument(XmlWriter w, TopologyDocument doc)
    {
        // <Subject ShortName="zenOn(R) exported project" MainVersion="15000">
        w.WriteStartElement("Subject");
        w.WriteAttributeString("ShortName",   XmlConstants.SubjectShortName);
        w.WriteAttributeString("MainVersion", XmlConstants.MainVersion.ToString());

        WriteApartment(w, doc);

        w.WriteEndElement(); // Subject
    }

    private void WriteApartment(XmlWriter w, TopologyDocument doc)
    {
        // <Apartment ShortName="zenOn(R) pictures list" Version="15000">
        w.WriteStartElement("Apartment");
        w.WriteAttributeString("ShortName", XmlConstants.ApartmentShortName);
        w.WriteAttributeString("Version",   XmlConstants.ApartmentVersion);

        WritePicture(w, doc);

        w.WriteEndElement(); // Apartment
    }

    private void WritePicture(XmlWriter w, TopologyDocument doc)
    {
        // <Picture ShortName="{screenName}">
        w.WriteStartElement("Picture");
        w.WriteAttributeString("ShortName", doc.ScreenName);

        // Picture 메타데이터
        w.WriteElementString("Title",            doc.ScreenName);
        w.WriteElementString("Template",         string.IsNullOrWhiteSpace(doc.Template) ? XmlConstants.PictureDefaultTemplate : doc.Template);
        w.WriteElementString("Type",             XmlConstants.PictureType);
        w.WriteElementString("SizeFromTemplate", string.IsNullOrWhiteSpace(doc.SizeFromTemplate) ? XmlConstants.PictureSizeFromTemplate : doc.SizeFromTemplate);
        w.WriteElementString("Width",            (doc.Width > 0 ? doc.Width : 3440).ToString());
        w.WriteElementString("Height",           (doc.Height > 0 ? doc.Height : 1440).ToString());
        w.WriteElementString("BackgroundColor",  XmlConstants.PictureBackgroundColor);

        // ─── 파이프라인: OrthogonalRouter → tagLabel 자동 주입 → 벡터 분해 ──
        var routed   = OrthogonalRouter.Route(doc.Elements);
        var labeled  = InjectTagLabels(routed);
        var elements = VectorSymbolRenderer.Expand(labeled);

        // <Elements_0 NODE="zenOn(R) embedded object" TYPE="…"> … </Elements_0>
        int index = 0;
        foreach (var element in elements)
        {
            if (_writers.TryGetValue(element.GetType(), out var writer))
            {
                writer.Write(w, element, index++);
            }
            else
            {
                throw new NotSupportedException(
                    $"요소 타입 '{element.GetType().Name}'에 대한 Writer가 등록되어 있지 않습니다.");
            }
        }

        w.WriteEndElement(); // Picture
    }

    /// <summary>
    /// SymbolElement 중 TagLabel이 설정된 항목의 바로 앞에 자동 TextElement를 삽입한다.
    /// 기본 위치: 심볼 중심 X, 심볼 상단 - XmlConstants.TagLabelYOffset.
    /// 단, 심볼 상단으로 수직 선로(Vertical Line)가 진입하는 경우에는 선로와 텍스트가 겹치지 않도록
    /// 우측(또는 하단)으로 자동 오프셋 배치한다.
    /// </summary>
    private static List<TopologyElement> InjectTagLabels(List<TopologyElement> elements)
    {
        var result = new List<TopologyElement>(elements.Count * 2);

        foreach (var element in elements)
        {
            if (element is SymbolElement sym && !string.IsNullOrWhiteSpace(sym.TagLabel))
            {
                int centerX = sym.CenterX ?? (sym.EffectiveX + sym.EffectiveWidth / 2);
                int centerY = sym.CenterY ?? (sym.EffectiveY + sym.EffectiveHeight / 2);
                int symTop = sym.DeviceType == DeviceType.Transformer
                    ? centerY - 44
                    : sym.EffectiveY;
                int symBottom = sym.DeviceType == DeviceType.Transformer
                    ? centerY + 44
                    : sym.EffectiveY + sym.EffectiveHeight;
                int symRight = sym.DeviceType == DeviceType.Transformer
                    ? centerX + 44
                    : sym.EffectiveX + sym.EffectiveWidth;

                int tagWidth = Math.Max(80, (sym.TagLabel?.Length ?? 1) * 12);
                int labelX = centerX - tagWidth / 2;
                int labelY = symTop - XmlConstants.TagLabelYOffset;

                // 심볼 상단에 수직 선로가 연결되어 있는지 검사
                bool hasVerticalWireAbove = false;
                bool hasHorizontalWireAtCenter = false;
                foreach (var other in elements)
                {
                    if (other is LineElement line)
                    {
                        if (line.X1 == line.X2 && Math.Abs(line.X1 - centerX) <= 4)
                        {
                            int minY = Math.Min(line.Y1, line.Y2);
                            int maxY = Math.Max(line.Y1, line.Y2);
                            if (minY < symTop && maxY >= symTop)
                            {
                                hasVerticalWireAbove = true;
                            }
                        }
                        else if (line.Y1 == line.Y2 && Math.Abs(line.Y1 - centerY) <= 4)
                        {
                            int minX = Math.Min(line.X1, line.X2);
                            int maxX = Math.Max(line.X1, line.X2);
                            if (minX <= symRight && maxX >= centerX)
                            {
                                hasHorizontalWireAtCenter = true;
                            }
                        }
                    }
                }

                if (hasVerticalWireAbove)
                {
                    if (hasHorizontalWireAtCenter)
                    {
                        // 상단 수직선 + 우측/중심 수평선이 모두 있는 코너 기기(예: MMC(-))는 하단 배치
                        labelX = centerX - tagWidth / 2;
                        labelY = symBottom + 8;
                    }
                    else
                    {
                        // 수직 분기선 위의 기기(DS1, 71CB, DS2, TR1-CB, TR-1 등)는 기기 우측 배치
                        int rightOffset = sym.DeviceType == DeviceType.Transformer ? 20 : 6;
                        labelX = symRight + rightOffset;
                        labelY = centerY - 14;
                    }
                }

                var labelElem = new TextElement
                {
                    Id       = sym.Id + "_TAG",
                    X        = labelX,
                    Y        = labelY,
                    Width    = tagWidth,
                    Text     = sym.TagLabel!,
                    FontSize = XmlConstants.TagLabelFontSize,
                    Color    = XmlConstants.TagLabelColor,
                };

                result.Add(labelElem); // 레이블을 심볼 앞에 삽입
            }

            result.Add(element);
        }

        return result;
    }
}
