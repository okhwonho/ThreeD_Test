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
            [typeof(SymbolElement)]    = new SymbolWriter(),
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
        w.WriteElementString("Width",            doc.Width.ToString());
        w.WriteElementString("Height",           doc.Height.ToString());
        w.WriteElementString("BackgroundColor",  XmlConstants.PictureBackgroundColor);

        // ─── 파이프라인: OrthogonalRouter → tagLabel 자동 주입 ────────────
        var routed   = OrthogonalRouter.Route(doc.Elements);
        var elements = InjectTagLabels(routed);

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
    /// TextElement 위치: 심볼 중심 X, 심볼 EffectiveY - XmlConstants.TagLabelYOffset
    /// </summary>
    private static List<TopologyElement> InjectTagLabels(List<TopologyElement> elements)
    {
        var result = new List<TopologyElement>(elements.Count * 2);

        foreach (var element in elements)
        {
            if (element is SymbolElement sym && !string.IsNullOrWhiteSpace(sym.TagLabel))
            {
                // 심볼 중심 X 계산
                int centerX = sym.CenterX ?? (sym.EffectiveX + sym.EffectiveWidth / 2);
                int labelY  = sym.EffectiveY - XmlConstants.TagLabelYOffset;
                int labelX  = centerX - (int)(XmlConstants.TagLabelFontSize * (sym.TagLabel!.Length + 1) * 0.3);

                var labelElem = new TextElement
                {
                    Id       = sym.Id + "_TAG",
                    X        = labelX,
                    Y        = labelY,
                    Text     = sym.TagLabel,
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
