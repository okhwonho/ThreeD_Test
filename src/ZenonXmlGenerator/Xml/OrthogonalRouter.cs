using System;
using System.Collections.Generic;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml;

/// <summary>
/// 사선(대각선) LineElement를 2개의 직교선(수평+수직)으로 분할하는 라우터.
///
/// 규칙:
///   - 모든 선은 X축 또는 Y축에 평행해야 한다.
///   - 대각선(IsDiagonal=true)이 감지되면 T-Junction(중간 꺾임점)을 계산하여
///     수평선 1개 + 수직선 1개로 분할한다.
///   - 꺾임점은 기본적으로 (x2, y1) — 먼저 수평 이동 후 수직 이동 순서.
///
/// 예시:
///   원본: (100,200) → (400,500)  [대각선]
///   분할: (100,200) → (400,200)  [수평]
///        (400,200) → (400,500)  [수직]
/// </summary>
public static class OrthogonalRouter
{
    /// <summary>
    /// 토폴로지 요소 목록에서 대각선 LineElement를 직교 2-세그먼트로 분할하여
    /// 새로운 요소 목록을 반환한다.
    /// 비(非) LineElement 요소는 그대로 통과(pass-through)한다.
    /// </summary>
    public static List<TopologyElement> Route(IEnumerable<TopologyElement> elements)
    {
        var result = new List<TopologyElement>();

        foreach (var element in elements)
        {
            if (element is LineElement line && line.IsDiagonal)
            {
                // 꺾임점: (x2, y1) — 수평 우선 라우팅
                int jx = line.X2;
                int jy = line.Y1;

                // 세그먼트 1: 수평선 (y1 → y1, x1 → x2)
                var seg1 = new LineElement
                {
                    Id          = line.Id + "_H",
                    X1          = line.X1,
                    Y1          = line.Y1,
                    X2          = jx,
                    Y2          = jy,
                    Color       = line.Color,
                    LineWidth   = line.LineWidth,
                    DeviceType  = line.DeviceType,
                    ALCUseColor = line.ALCUseColor,
                    TagLabel    = line.TagLabel,
                };

                // 세그먼트 2: 수직선 (x2, y1 → y2)
                var seg2 = new LineElement
                {
                    Id          = line.Id + "_V",
                    X1          = jx,
                    Y1          = jy,
                    X2          = line.X2,
                    Y2          = line.Y2,
                    Color       = line.Color,
                    LineWidth   = line.LineWidth,
                    DeviceType  = line.DeviceType,
                    ALCUseColor = line.ALCUseColor,
                };

                result.Add(seg1);
                result.Add(seg2);
            }
            else
            {
                result.Add(element);
            }
        }

        return result;
    }
}
