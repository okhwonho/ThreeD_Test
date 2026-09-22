using System.Xml;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Xml.ElementWriters;

/// <summary>
/// 심볼(Combined element) 요소를 zenon XML로 출력한다. TYPE="16"
///
/// Ground Truth 구조:
/// <code>
/// &lt;Elements_n TYPE="16" NODE="zenOn(R) embedded object"&gt;
///   &lt;StartX&gt;…&lt;/StartX&gt;
///   &lt;DynEleVar_0&gt;
///     &lt;ProjectVar&gt;{VariableName}&lt;/ProjectVar&gt;
///     &lt;SymVarName&gt;{VariableName}&lt;/SymVarName&gt;
///   &lt;/DynEleVar_0&gt;
///   &lt;States_0 NODE="zenOn(R) embedded object"&gt;
///     &lt;Value&gt;0&lt;/Value&gt;&lt;ValueMask&gt;0&lt;/ValueMask&gt;
///     &lt;SymbolName&gt;Symbol 6&lt;/SymbolName&gt;
///   &lt;/States_0&gt;
///   …
/// &lt;/Elements_n&gt;
/// </code>
/// </summary>
public sealed class SymbolWriter : IElementWriter
{
    public void Write(XmlWriter writer, TopologyElement element, int index)
    {
        var sym = (SymbolElement)element;

        writer.WriteStartElement($"Elements_{index}");
        writer.WriteAttributeString("NODE", XmlConstants.NodeEmbeddedObject);
        writer.WriteAttributeString("TYPE", XmlConstants.TypeSymbol);

        // 위치·크기 (EffectiveX/EffectiveY)
        writer.WriteElementString("StartX", sym.EffectiveX.ToString());
        writer.WriteElementString("StartY", sym.EffectiveY.ToString());
        writer.WriteElementString("Width",  sym.Width.ToString());
        writer.WriteElementString("Height", sym.Height.ToString());

        // ALCType (선택적)
        if (!string.IsNullOrWhiteSpace(sym.EffectiveALCType))
        {
            writer.WriteElementString("ALCType", sym.EffectiveALCType);
        }

        // 변수 바인딩 (GUID 없음 — 문자열 명칭만)
        if (!string.IsNullOrWhiteSpace(sym.VariableName))
        {
            writer.WriteStartElement("DynEleVar_0");
            writer.WriteElementString("ProjectVar", sym.VariableName);
            writer.WriteElementString("SymVarName", sym.VariableName);
            writer.WriteEndElement(); // DynEleVar_0
        }

        // States_n (자동 생성 또는 JSON 커스텀)
        var states = sym.EffectiveStates;
        for (int i = 0; i < states.Count; i++)
        {
            var state = states[i];
            writer.WriteStartElement($"States_{i}");
            writer.WriteAttributeString("NODE", XmlConstants.NodeEmbeddedObject);
            writer.WriteElementString("Value",      state.Value.ToString());
            writer.WriteElementString("ValueMask",  state.ValueMask.ToString());
            writer.WriteElementString("SymbolName", state.SymbolName);
            writer.WriteEndElement(); // States_n
        }

        writer.WriteEndElement(); // Elements_n
    }
}
