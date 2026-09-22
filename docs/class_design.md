# ZenonXmlGenerator — Class Design

## 객체 모델 개요

```
ZenonXmlGenerator (Facade)
│
├── [JSON] System.Text.Json.JsonSerializer
│       └─▶ TopologyDocument
│               ├─ ScreenName / Width / Height
│               └─ List<TopologyElement>  [polymorphic: type discriminator]
│                       ├─ LineElement      (type="line")
│                       ├─ RectangleElement (type="rectangle")
│                       ├─ TextElement      (type="text")
│                       └─ SymbolElement    (type="symbol")
│                               └─ List<SymbolState>  (nullable → auto 3-state)
│
└── [XML] ZenonXmlBuilder
        ├─ XmlWriterSettings { Encoding=Unicode, Indent=true }
        ├─ Dictionary<Type, IElementWriter>
        │       ├─ LineWriter      → TYPE="101"
        │       ├─ RectangleWriter → TYPE="102"
        │       ├─ TextWriter      → TYPE="107"
        │       └─ SymbolWriter    → TYPE="16"
        └─ ColorConverter  (#RRGGBB → COLORREF DWORD)
```

---

## 핵심 클래스 역할

| 클래스 | 역할 |
|--------|------|
| `ZenonXmlGenerator` | 공개 Facade. JSON 파싱 + XML 빌드 조합. |
| `ZenonXmlBuilder` | UTF-16 BOM XmlWriter 생성, Screen 루트 구조 출력. |
| `XmlConstants` | zenon 15 TYPE 코드, NODE 문자열, ValueMask 상수의 단일 출처. |
| `ColorConverter` | HTML #RRGGBB → Windows COLORREF (BGR DWORD) 변환. |
| `IElementWriter` | 요소별 XML 출력 전략 인터페이스. |
| `LineWriter` | StartX/Y, Width=dx, Height=dy (음수 허용). |
| `SymbolWriter` | DynEleVar_0 바인딩 + States_n (자동/커스텀). |
| `TopologyElement` | `[JsonPolymorphic]` 기반 다형성 역직렬화 기반 클래스. |
| `SymbolElement` | `States` null/empty 시 기본 3-state 자동 생성 (`EffectiveStates`). |

---

## zenon 15 States 규칙 (Ground Truth A1)

```xml
<!-- States_0: wildcard default (ValueMask=0) -->
<States_0 NODE="zenOn(R) embedded object">
  <Value>0</Value>
  <ValueMask>0</ValueMask>
  <SymbolName>{LibrarySymbolName}</SymbolName>
</States_0>

<!-- States_1: OFF exact (ValueMask=0xFFFFFFFF) -->
<States_1 NODE="zenOn(R) embedded object">
  <Value>0</Value>
  <ValueMask>4294967295</ValueMask>
  <SymbolName>{LibrarySymbolName}</SymbolName>
</States_1>

<!-- States_2: ON exact -->
<States_2 NODE="zenOn(R) embedded object">
  <Value>1</Value>
  <ValueMask>4294967295</ValueMask>
  <SymbolName>{LibrarySymbolName}</SymbolName>
</States_2>
```

커스텀 상태가 JSON `states` 배열로 제공되면 자동 생성을 완전히 대체한다.

---

## 확장 포인트

- **새 요소 타입 추가**: `TopologyElement` 파생 클래스 + `[JsonDerivedType]` 등록 + `IElementWriter` 구현 + `ZenonXmlBuilder._writers`에 등록.
- **Screen 다중 지원**: `TopologyDocument`에 `List<TopologyScreen>` 추가 후 `ZenonXmlBuilder.WriteScreen` 루프화.
- **다국어 폰트**: `TextWriter`에 `FontName` 필드 추가.
