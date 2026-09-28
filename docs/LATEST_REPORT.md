# Latest Execution Report

> **작업 일시:** 2026-09-28 13:57 KST  
> **마일스톤:** zenon 15 렌더링 버그 3종 근본 수정 — 화면 잘림, 텍스트 회색 박스, 모선 가로 누움

---

## 1. 개요 (Summary)

### 1.1 화면 잘림 해결: SizeFromTemplate = FALSE
- **원인:** `SizeFromTemplate = TRUE`는 zenon이 MAIN 프레임(1920px) 크기로 화면을 강제 축소 → 3840 와이드 도면이 절반 잘림.
- **수정:**
  - `XmlConstants.PictureSizeFromTemplate = "FALSE"` (기존 `"TRUE"`)
  - `TopologyDocument.SizeFromTemplate` C# 기본값 `"FALSE"` (기존 `"TRUE"`)
  - `hvdc_full_system_topology.json` 헤더: `"sizeFromTemplate": "FALSE"` 명시
- **결과:** 3840×1080 와이드 화면이 Width/Height 기준으로 온전히 렌더링됨.

### 1.2 텍스트 불투명 회색 박스 해결: TextWriter Ground Truth 구조 완전 일치화
- **원인:** 이전 구조(`FillStyle/Transparent`)가 zenon 실제 TYPE="107" 표준 노드 구조와 불일치 → 회색 불투명 배경 렌더링.
- **수정** (`TextWriter.cs`): zenon 15 standard.XML Ground Truth 구조로 전면 교체:
  ```xml
  <BackColor>0</BackColor>
  <LineColor>FFFFFF</LineColor>
  <AlphaForeColor>0</AlphaForeColor>
  <AlphaLineColor>FFFFFF</AlphaLineColor>
  <AlphaBackColor>0</AlphaBackColor>
  <Transparent>TRUE</Transparent>
  ```
- **결과:** 텍스트 박스 배경 완전 투명, 텍스트만 렌더링.

### 1.3 모선 가로 누움 해결: 수직 모선 좌표 전면 교정
- **원인:** 이전 JSON에서 모선이 수평선(`y1=y2, x1≠x2`)으로 정의되어 도면에서 가로로 표시됨.
- **수정** (`hvdc_full_system_topology.json` 전면 재작성):
  - **ST1 수직 모선:**
    - `BUS_345KV_1`: x1=150, y1=200, x2=150, y2=880, lineWidth=12 ✅
    - `BUS_345KV_2`: x1=270, y1=200, x2=270, y2=880, lineWidth=12 ✅
  - **ST2 수직 모선:**
    - `BUS_345KV_2`: x1=3570, y1=200, x2=3570, y2=880, lineWidth=12 ✅
    - `BUS_345KV_1`: x1=3690, y1=200, x2=3690, y2=880, lineWidth=12 ✅
  - **DC 라인 (수평 직교):**
    - Pos Pole (+525kV): y=200 수평, lineWidth=6
    - DMR Neutral: y=540 수평, lineWidth=3
    - Neg Pole (-525kV): y=880 수평, lineWidth=6
  - **중앙 계측 블록:** x=1800, y=470, 240×140
  - **ST1 00CB 타이 차단기:** centerX=210, centerY=350 (수평 연결 150→270)
  - **ST2 00CB 타이 차단기:** centerX=3630, centerY=350 (수평 연결 3570→3690)
  - **전체 배선 직교성 보장:** 모든 라인 x1==x2(수직) 또는 y1==y2(수평) — OrthogonalRouter 불필요

---

## 2. 세부 변경 파일 목록
| 파일 | 변경 유형 | 주요 내용 |
|------|-----------|-----------|
| `src/.../Xml/XmlConstants.cs` | 수정 | `PictureSizeFromTemplate = "FALSE"` |
| `src/.../Models/TopologyDocument.cs` | 수정 | `SizeFromTemplate` C# 기본값 `"FALSE"` |
| `src/.../ElementWriters/TextWriter.cs` | 수정 | zenon standard.XML Ground Truth 구조 완전 일치화 (BackColor/LineColor/AlphaXxx/Transparent) |
| `tests/.../Samples/hvdc_full_system_topology.json` | 전면 재작성 | 수직 모선, 3840 캔버스, 직교 배선, SizeFromTemplate=FALSE |
| `tests/.../RootNodeTests.cs` | 수정 | `SizeFromTemplate` 어설션 `"FALSE"` |
| `tests/.../CliE2ETests.cs` | 수정 | HVDC Full Bipole 테스트 `SizeFromTemplate "FALSE"` |
| `output_hvdc_full_system.xml` | 재생성 | 156,240 bytes, UTF-16 LE BOM |

---

## 3. 검증 결과 (Validation Results)
- **빌드 (`dotnet build`):** 성공 (경고 0, 오류 0)
- **단위 및 E2E 테스트 (`dotnet test`):**
  - 총 테스트 수: **67개**
  - 통과: **67개**
  - 실패: **0개**
  - 실행 시간: **63 ms**

---

## 4. Git 배포 정보 (Git Information)
- **Branch:** `main`
- **Remote:** `origin/main`
