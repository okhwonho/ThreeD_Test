# Topology JSON Schema Specification for AI / Vision Model Integration

> **문서 버전:** 1.0.0  
> **최종 수정일:** 2026-09-22  
> **대상:** 도면(Single Line Diagram / CAD / Scan Image) 인식 AI & Vision 파이프라인 엔지니어

---

## 1. 개요 (Overview)

본 문서는 전력 단선도(SLD) 이미지 또는 CAD 도면을 AI/비전 인식 모듈이 분석한 후, `zenon 15 Engineering Studio` 화면 XML 변환기(`ZenonXmlGenerator` / `zenon-gen` CLI)로 전달할 **표준 Topology JSON 인터페이스 규격**을 정의합니다.

```
┌─────────────────────────┐
│  SLD Image / CAD Drawing │
└────────────┬────────────┘
             │ (AI / Computer Vision Recognition)
             ▼
┌─────────────────────────┐
│  Topology JSON (v1.0)   │ ◀── 본 명세서 대상
└────────────┬────────────┘
             │ (zenon-gen CLI)
             ▼
┌─────────────────────────┐
│ zenon Screen XML (v15)  │ (UTF-16 LE BOM, Standard Screen Type=2)
└─────────────────────────┘
```

---

## 2. 최상위 구조 (Document Root)

JSON의 루트는 단일 Screen 정의 객체입니다.

| 필드명 | 타입 | 필수 | 기본값 | 설명 |
|---|---|---|---|---|
| `screenName` | string | 필수 | `"Screen1"` | zenon 화면 식별명 (Picture/@ShortName 및 Title) |
| `template` | string | 선택 | `"Standard"` | zenon 템플릿(프레임) 이름 |
| `width` | integer | 필수 | `1920` | 캔버스 기준 너비 (픽셀 / 좌표 단위) |
| `height` | integer | 필수 | `1080` | 캔버스 기준 높이 (픽셀 / 좌표 단위) |
| `elements` | array | 필수 | `[]` | 화면에 배치될 기기, 선, 텍스트 요소 목록 |

### JSON 예시:
```json
{
  "screenName": "Substation_154kV_SLD",
  "template": "Standard",
  "width": 1920,
  "height": 1080,
  "elements": [ ... ]
}
```

---

## 3. 요소 다형성 (Element Polymorphism)

모든 요소는 공통 필드를 가지며, `"type"` 필드 값에 따라 4가지 구체 타입으로 분기됩니다:
- `"line"` : 선 / 모선 (Busbar, Feeder branch)
- `"symbol"` : 전력 기기 심볼 (차단기, 단로기, 변압기 등)
- `"rectangle"` : 구역 베이(Bay) 프레임, 기기 박스
- `"text"` : 모선명, 기기 레이블, 전압 등 텍스트

### 3.1. 공통 필드 (Base Properties)

| 필드명 | 타입 | 필수 | 기본값 | 설명 |
|---|---|---|---|---|
| `type` | string | 필수 | - | 요소 구분자 (`"line"`, `"symbol"`, `"rectangle"`, `"text"`) |
| `id` | string | 필수 | `""` | 도면 내 객체 고유 식별자 (예: `"CB101"`, `"BUS_1"`) |
| `rotation` | number | 선택 | `0.0` | 회전 각도 (도 단위: `0`, `90`, `180`, `270`) |
| `tagLabel` | string | 선택 | `null` | 도면 상의 텍스트 태그 매핑 라벨 (예: `"52-1A"`) |

---

## 4. 기기 심볼 정의 (`type: "symbol"`)

차단기, 단로기, 변압기 등 도면에서 검출된 전력 기기 심볼입니다.

### 4.1. 필드 명세

| 필드명 | 타입 | 필수 | 설명 |
|---|---|---|---|
| `deviceType` | string | 권장 | 기기 분류 enum (`"CircuitBreaker"`, `"Disconnector"`, `"Transformer"`, `"CurrentTransformer"`, `"PotentialTransformer"`, `"EarthSwitch"`) |
| `librarySymbolName` | string | 필수 | zenon Symbol Library 심볼 이름 (예: `"CB_Open"`, `"DS_Open"`, `"TR_2W"`) |
| `variableName` | string | 필수 | zenon 프로젝트 변수 바인딩 경로 (예: `"SS1.BAY1.CB11.Status"`) |
| `x` | integer | 조건부 | 심볼 좌상단 X 좌표 |
| `y` | integer | 조건부 | 심볼 좌상단 Y 좌표 |
| `centerX` | integer | 선택 | 비전 검출 중심점 X 좌표 (`x` 미제공 시 `centerX - width/2`로 자동 계산) |
| `centerY` | integer | 선택 | 비전 검출 중심점 Y 좌표 (`y` 미제공 시 `centerY - height/2`로 자동 계산) |
| `width` | integer | 필수 | 심볼 너비 (기본 40~60px 권장) |
| `height` | integer | 필수 | 심볼 높이 (기본 40~60px 권장) |
| `alcType` | string | 선택 | ALC 기기 타입 (`"2"`: 차단기, `"7"`: 단로기, `"4"`: 변압기). 미지정 시 `deviceType`에서 자동 매핑. |
| `states` | array | 선택 | 상태별 심볼/동작 매핑. 생략 시 기본 3-state (Wildcard/OFF/ON) 자동 생성. |

### 4.2. `DeviceType` Enum 매핑표

| `DeviceType` | 설명 | 기본 `alcType` | 권장 심볼 크기 (W×H) |
|---|---|---|---|
| `CircuitBreaker` | 차단기 (CB) | `"2"` | 40 × 40 |
| `Disconnector` | 단로기 (DS) | `"7"` | 40 × 40 |
| `Transformer` | 주변압기 (TR) | `"4"` | 60 × 60 |
| `CurrentTransformer` | 변류기 (CT) | - | 40 × 40 |
| `PotentialTransformer` | 계기용변압기 (PT) | - | 40 × 40 |
| `EarthSwitch` | 접지개폐기 (ES) | - | 40 × 40 |

### 4.3. 상태 매핑 구조 (`states`)

```json
{
  "value": 0,            // 상태 조건 값 (정수)
  "valueMask": 4294967295,// 비트마스크 (0=와일드카드, 4294967295=정확한 매칭)
  "symbolName": "CB_Open" // 해당 상태에서 렌더링할 심볼명
}
```

---

## 5. 연결선 및 모선 정의 (`type: "line"`)

도면 내 모선(Busbar), 인입/인출선, 기기 간 결선 라인입니다.

### 5.1. 필드 명세

| 필드명 | 타입 | 필수 | 기본값 | 설명 |
|---|---|---|---|---|
| `x1` | integer | 필수 | - | 시작점 X 좌표 |
| `y1` | integer | 필수 | - | 시작점 Y 좌표 |
| `x2` | integer | 필수 | - | 끝점 X 좌표 |
| `y2` | integer | 필수 | - | 끝점 Y 좌표 |
| `color` | string | 선택 | `"#000000"` | 색상 HEX 코드 (`"#RRGGBB"`) |
| `lineWidth` | integer | 선택 | `1` | 선 두께 (모선의 경우 `5`, 일반 결선 `2`) |
| `deviceType` | string | 선택 | `null` | 모선일 경우 `"Busbar"` 지정 |
| `alcUseColor` | string | 선택 | `null` | 모선 동적 배선 착색 적용 여부 (`"TRUE"` / `"FALSE"`) |
| `connectedBus` | string | 선택 | `null` | 연결된 상위 모선 ID (예: `"BUS_1"`) |

---

## 6. 사각형 및 텍스트 정의

### 6.1. 사각형 (`type: "rectangle"`)
- 베이(Bay) 영역 구분 박스, 주석 박스
- 속성: `x`, `y`, `width`, `height`, `fillColor` (`#RRGGBB`), `borderColor` (`#RRGGBB`)

### 6.2. 텍스트 (`type: "text"`)
- 모선 명칭, 전압 등급(154kV, 22.9kV), 기기 레이블
- 속성: `x`, `y`, `text`, `fontSize` (기본 12), `color` (`#RRGGBB`), `width` (선택), `height` (선택)

---

## 7. 변수 바인딩 네이밍 룰 템플릿 (Variable Naming Template)

비전 모듈에서 자동 바인딩을 생성할 때 아래 계층 명명 규칙 템플릿을 준수합니다:

$$\text{VariableName} = \langle\text{Substation}\rangle.\langle\text{Bay}\rangle.\langle\text{DeviceTag}\rangle.\langle\text{Attribute}\rangle$$

| 컴포넌트 | 예시 | 설명 |
|---|---|---|
| `Substation` | `SS1`, `Substation154` | 변전소/플랜트 식별자 |
| `Bay` | `BAY1`, `TR1_BAY`, `LINE1_BAY` | 베이/모듈 식별자 |
| `DeviceTag` | `CB11`, `DS12`, `TR1`, `CT11` | 기기 태그/식별자 |
| `Attribute` | `Status`, `CurrentVal`, `VoltageVal` | 측정값/상태값 필드 |

### 실제 예시:
- `SS1.BAY1.CB11.Status` (차단기 개폐 상태)
- `SS1.BAY1.DS11.Status` (단로기 개폐 상태)
- `SS1.BAY2.CT21.CurrentVal` (변류기 전류 측정값)
- `SS1.BAY2.PT21.VoltageVal` (변압기 전압 측정값)

---

## 8. 완성형 예시 (Complete Sample)

```json
{
  "screenName": "Substation_154kV_SLD",
  "template": "Standard",
  "width": 1920,
  "height": 1080,
  "elements": [
    {
      "type": "text",
      "id": "TXT_BUS1",
      "x": 60,
      "y": 120,
      "text": "154kV BUS #1",
      "fontSize": 14,
      "color": "#000000"
    },
    {
      "type": "line",
      "id": "BUS_1",
      "deviceType": "Busbar",
      "alcUseColor": "TRUE",
      "x1": 60,
      "y1": 150,
      "x2": 1800,
      "y2": 150,
      "color": "#FF0000",
      "lineWidth": 5
    },
    {
      "type": "symbol",
      "id": "CB11",
      "deviceType": "CircuitBreaker",
      "alcType": "2",
      "centerX": 360,
      "centerY": 460,
      "width": 40,
      "height": 40,
      "librarySymbolName": "CB_Open",
      "variableName": "SS1.BAY1.CB11.Status",
      "states": [
        { "value": 0, "valueMask": 0, "symbolName": "CB_Open" },
        { "value": 0, "valueMask": 4294967295, "symbolName": "CB_Open" },
        { "value": 1, "valueMask": 4294967295, "symbolName": "CB_Closed" }
      ]
    }
  ]
}
```
