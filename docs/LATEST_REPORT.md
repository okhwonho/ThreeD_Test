# Engineering Audit & Final Defect Resolution Report

> **작업 일시:** 2026-10-21 KST  
> **마일스톤:** 텍스트 불투명 블랙 박스 근본 해결(`AlphaBackColor=100` & `BackColor=07101C`), 텍스트 간 겹침 0건 달성, 수직 분기선 기기 라벨 우측/하단 오프셋 배치, CT/PT 중앙 정렬, 변압기 권선 관통 선로 자동 클리핑 완료  

---

## 1. 해결된 5대 핵심 결함 내역

### 1.1 [Critical] `TYPE="107"`(정적 텍스트) 검은색 불투명 배경 박스 근본 해결 (`TextWriter.cs`)
- **원인:** zenon 15 `TYPE="107"` 스키마(`Golden.XML` 기준)에서 `AlphaBackColor`는 불투명도가 아니라 **투명도(Transparency %, `0` = 불투명 100%, `100` = 완전 투명)** 이며, 이전 `<BackColor>0</BackColor>` + `<AlphaBackColor>0</AlphaBackColor>` 설정은 검은색(`#000000`) 불투명 박스를 렌더링함.
- **조치:**
  - `<AlphaBackColor>100</AlphaBackColor>` (투명도 100%)
  - `<BackColor>07101C</BackColor>` (다크 네이비 캔버스 배경색 매칭으로 GDI 폴백 시에도 이질감 완전 제거)
  - `<Transparent>TRUE</Transparent>`, `<FillPattern>0</FillPattern>`, `<LineWidth>0</LineWidth>` 동시 출력

### 1.2 [High] 텍스트 간 바운딩 박스 충돌(Text-to-Text Overlap) 전수 해소 (`hvdc_full_system_topology.json`)
- **DMR 라벨 분리:** `TXT_DMR_LABEL`(`'DMR  Neutral / Metallic Return'`)을 `X=1540, Y=785`로 배치하여 `DMR_GND_TAG`, `DMR_SW2_TAG` 및 접지 사다리(`Y2=774`)와 11px 이상 완전 이격.
- **좌·우측 345kV 모선 상단 헤더 분리:**
  - ST1: `TXT_ST1_BUS1`(`X=70, Y=160`), `TXT_ST1_BUS2`(`X=220, Y=160`) → 18px 수평 여백 확보
  - ST2: `TXT_ST2_BUS2`(`X=3090, Y=160`), `TXT_ST2_BUS1`(`X=3240, Y=160`) → 18px 수평 여백 확보
- **DC Yard 인접 스위치 간격 확대:** `P1 ES` / `P2 ES` / `PLD DS+` (및 음극/ST2 대칭 기기) 중심 간격을 `80px` → `90px`로 조정하여 인접 태그 박스 간 최소 8px 이상 수평 간격 확보, 극성 라벨(`TXT_DC_POS_LABEL`, `TXT_DC_NEG_LABEL`)을 `X=1576` 정중앙으로 정렬.
- **`DC_SPEC_BOX` 내부 텍스트 줄간격 확대:** `DC_SPEC_BOX` 높이를 `170px`(`Y=480..650`)로 확장하고 내부 4개 텍스트 행 간격을 `25px` → `30px`(`Y=492, 522, 552, 582`)로 넓혀 행간 겹침 완전 제거.

### 1.3 [High] 수직(Vertical) 선로 위 기기 라벨 자동 우측/하단 오프셋 (`ZenonXmlBuilder.cs`)
- `InjectTagLabels()`에서 기기 상단으로 수직 선로가 진입하는 기기(`DS1`, `71CB`, `DS2`, `72DS`, `72CB`, `TR1-CB`, `TR-1`, `MMC(-)`)를 자동 감지:
  - 수직 분기 기기: 기기 우측(`symRight + 6`, 변압기는 `symRight + 20`, `Y = centerY - 14`)에 배치하여 수직 선로 가림 현상 제거.
  - 상단 수직선과 수평선이 교차하는 코너 기기(`MMC(-)`): 기기 하단(`symBottom + 8`)에 배치하여 양쪽 선로와의 충돌을 모두 회피.

### 1.4 [Medium] `CT` / `PT` 심볼 내부 텍스트 중앙 정렬 및 백색 교정 (`VectorSymbolRenderer.cs`)
- `RenderCT()`, `RenderPT()`의 라벨 좌표를 `X = cx - 40, Y = cy - 14` 및 `Color = #FFFFFF`로 교정하여 `80×28` 텍스트 박스 중심이 기기 중심 `(cx, cy)`와 정확히 일치하도록 수정.

### 1.5 [Medium] 변압기(TR) 투명 권선 원환 내부 관통 선로 자동 클리핑 (`VectorSymbolRenderer.cs`)
- `VectorSymbolRenderer.Expand()`에서 변압기(`DeviceType.Transformer`)의 중심 `(cx, cy)`에 연결된 수직 선로 끝점을 변압기 외곽 코일 경계(`cy - 44` 및 `cy + 44`)로 자동 트리밍하여, 내부가 투명한 3권선 원환(`Y`, `Y`, `Δ`) 내부를 수직선이 관통하지 않도록 개선.

---

## 2. 독립 검증 및 단위 테스트 결과

- **생성 파일:** `output_hvdc_full_system.xml` (226,084 bytes, UTF-16 LE BOM, `3440 × 1440`)
- **텍스트 투명도 스키마 준수율:** 74 / 74개 (`100%`, `AlphaBackColor=100`, `BackColor=07101C`, `Transparent=TRUE`)
- **전체 텍스트 바운딩 박스 충돌 검사:** **0건** (기존 7건 → 0건)
- **화면 하단 클리어런스:** 최대 `Y2 = 1380px` (모니터 하단 1440px 대비 **60px 여백**)
- **단위 테스트:** `dotnet test` **87개 전체 통과 (실패 0, 건너뜀 0)**
