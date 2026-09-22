using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 특정 TopologyElement 타입을 XmlWriter에 GrafEle_n 형태로 출력하는 전략 인터페이스.
/// </summary>
public interface IElementWriter
{
    /// <summary>
    /// <paramref name="element"/>를 GrafEle_<paramref name="index"/> 요소로 출력한다.
    /// </summary>
    void Write(XmlWriter writer, TopologyElement element, int index);
}
