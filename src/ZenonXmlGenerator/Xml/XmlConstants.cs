namespace ZenonXmlGenerator.Xml;

/// <summary>
/// zenon Engineering Studio 15 XML 출력에 사용되는 상수 정의.
/// Ground Truth 규칙을 단일 파일에서 관리한다.
/// </summary>
public static class XmlConstants
{
    // ─── Subject 루트 속성 ───────────────────────────────────────────────
    /// <summary>Subject/@ShortName</summary>
    public const string SubjectShortName = "zenOn(R) exported project";

    /// <summary>Subject/@MainVersion — zenon 15</summary>
    public const int MainVersion = 15000;

    // ─── Apartment 컨테이너 속성 ─────────────────────────────────────────
    /// <summary>Apartment/@ShortName</summary>
    public const string ApartmentShortName = "zenOn(R) pictures list";

    /// <summary>Apartment/@Version</summary>
    public const string ApartmentVersion = "15000";

    // ─── Picture 컨테이너 기본값 ─────────────────────────────────────────
    /// <summary>Picture 기본 타입 값 (Standard 화면 = 0, Ground Truth)</summary>
    public const string PictureType = "0";

    /// <summary>Picture 기본 배경색 (zenon 시스템 색상 DWORD)</summary>
    public const string PictureBackgroundColor = "80000037";

    /// <summary>Picture 기본 템플릿(프레임) 이름</summary>
    public const string PictureDefaultTemplate = "MAIN";

    /// <summary>Picture SizeFromTemplate 기본값</summary>
    public const string PictureSizeFromTemplate = "TRUE";

    // ─── NODE 속성 값 ────────────────────────────────────────────────────
    /// <summary>Elements_n/@NODE — 모든 화면 요소에 공통 적용</summary>
    public const string NodeEmbeddedObject = "zenOn(R) embedded object";

    // ─── 요소 TYPE 코드 ──────────────────────────────────────────────────
    /// <summary>선(Line)         TYPE="101"</summary>
    public const string TypeLine      = "101";

    /// <summary>사각형(Rectangle) TYPE="102"</summary>
    public const string TypeRectangle = "102";

    /// <summary>정적 텍스트       TYPE="107"</summary>
    public const string TypeText      = "107";

    /// <summary>심볼(Combined)    TYPE="16"</summary>
    public const string TypeSymbol    = "16";

    // ─── States ValueMask 상수 ───────────────────────────────────────────
    /// <summary>wildcard(default) 상태의 ValueMask = 0</summary>
    public const long ValueMaskWildcard = 0L;

    /// <summary>exact-match 상태의 ValueMask = 0xFFFFFFFF</summary>
    public const long ValueMaskExact = 4294967295L;

    // ─── ALC (Automatic Line Coloring) 상수 ──────────────────────────────
    /// <summary>차단기 ALCType = 2</summary>
    public const string ALCTypeCircuitBreaker = "2";

    /// <summary>단로기 ALCType = 7</summary>
    public const string ALCTypeDisconnector = "7";

    /// <summary>변압기 ALCType = 4</summary>
    public const string ALCTypeTransformer = "4";

    // ─── 기기 심볼 표준 크기 (Symbol Size Standardization) ───────────────
    /// <summary>차단기(CircuitBreaker) 기본 크기: 32×32 px</summary>
    public const int SymbolSizeCB = 32;

    /// <summary>단로기(Disconnector) 기본 크기: 24×24 px</summary>
    public const int SymbolSizeDS = 24;

    /// <summary>변압기(Transformer) 기본 크기: 60×60 px</summary>
    public const int SymbolSizeTR = 60;

    /// <summary>기타 심볼(CT, PT, ES 등) 기본 크기: 28×28 px</summary>
    public const int SymbolSizeDefault = 28;

    // ─── 시각적 계층화 선 굵기 (Visual Hierarchy Line Widths) ─────────────
    /// <summary>주 모선(Busbar) 기본 선 굵기 = 12</summary>
    public const int LineWidthBusbar = 12;

    /// <summary>주요 분기/극 라인(Feeder/Pole) 기본 선 굵기 = 6</summary>
    public const int LineWidthFeeder = 6;

    /// <summary>일반 연결선 기본 선 굵기 = 3</summary>
    public const int LineWidthDefault = 3;

    // ─── 텍스트 레이블 자동 오프셋 ────────────────────────────────────────
    /// <summary>TagLabel 텍스트를 심볼 중심 Y에서 위로 띄우는 픽셀 오프셋 = 30</summary>
    public const int TagLabelYOffset = 30;

    /// <summary>TagLabel 텍스트 기본 폰트 크기 = 9</summary>
    public const int TagLabelFontSize = 9;

    /// <summary>TagLabel 텍스트 기본 색상 (짙은 회색)</summary>
    public const string TagLabelColor = "#444444";
}
