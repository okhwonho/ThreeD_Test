using System.Collections.Generic;
using System.IO;
using System.Xml;
using ZenonXmlGenerator;
using ZenonXmlGenerator.Models;

namespace ZenonXmlGenerator.Tests;

/// <summary>
/// Symbol 요소의 변수 바인딩 및 States_n 구조 검증.
/// Ground Truth:
///   DynEleVar_0/ProjectVar = VariableName
///   DynEleVar_0/SymVarName = VariableName
///   States_0: Value=0, ValueMask=0,          SymbolName (wildcard/default)
///   States_1: Value=0, ValueMask=4294967295,  SymbolName (OFF exact)
///   States_2: Value=1, ValueMask=4294967295,  SymbolName (ON exact)
/// </summary>
public sealed class SymbolBindingTests
{
    private static XmlDocument BuildWithSymbol(SymbolElement sym)
    {
        var topDoc = new TopologyDocument
        {
            ScreenName = "TestScreen",
            Elements   = new List<TopologyElement> { sym },
        };
        var gen   = new ZenonXmlGenerator();
        var bytes = gen.GenerateFromDocument(topDoc);
        var doc   = new XmlDocument();
        using var ms = new MemoryStream(bytes);
        doc.Load(ms);
        return doc;
    }

    private static XmlElement GetSymbolEle(XmlDocument doc) =>
        (XmlElement)doc.DocumentElement!
            .SelectSingleNode("Apartment/Picture/Elements_0")!;

    // ─── TYPE 코드 ────────────────────────────────────────────────────

    [Fact]
    public void Symbol_HasTypeCode16()
    {
        var sym = new SymbolElement
        {
            Id = "S1", VariableName = "Var1", LibrarySymbolName = "CB_Open",
            X = 0, Y = 0, Width = 40, Height = 40,
        };
        var xml = BuildWithSymbol(sym);
        Assert.Equal("16", GetSymbolEle(xml).GetAttribute("TYPE"));
    }

    // ─── DynEleVar_0 바인딩 ───────────────────────────────────────────

    [Fact]
    public void Symbol_DynEleVar0_HasProjectVar()
    {
        const string varName = "Substation1.CB1.Status";
        var sym = new SymbolElement
        {
            Id = "S1", VariableName = varName, LibrarySymbolName = "CB_Open",
            X = 0, Y = 0, Width = 40, Height = 40,
        };
        var xml = BuildWithSymbol(sym);
        var ele = GetSymbolEle(xml);

        var projectVar = ele.SelectSingleNode("DynEleVar_0/ProjectVar")!.InnerText;
        Assert.Equal(varName, projectVar);
    }

    [Fact]
    public void Symbol_DynEleVar0_HasSymVarName()
    {
        const string varName = "Substation1.CB1.Status";
        var sym = new SymbolElement
        {
            Id = "S1", VariableName = varName, LibrarySymbolName = "CB_Open",
            X = 0, Y = 0, Width = 40, Height = 40,
        };
        var xml = BuildWithSymbol(sym);
        var ele = GetSymbolEle(xml);

        var symVarName = ele.SelectSingleNode("DynEleVar_0/SymVarName")!.InnerText;
        Assert.Equal(varName, symVarName);
    }

    // ─── 자동 3-state 생성 ────────────────────────────────────────────

    [Fact]
    public void Symbol_AutoStates_GeneratesThreeStates()
    {
        var sym = new SymbolElement
        {
            Id = "S1", VariableName = "Var1", LibrarySymbolName = "CB_Open",
            X = 0, Y = 0, Width = 40, Height = 40,
            // States 생략 → 자동 생성
        };
        var xml = BuildWithSymbol(sym);
        var ele = GetSymbolEle(xml);

        Assert.NotNull(ele.SelectSingleNode("States_0"));
        Assert.NotNull(ele.SelectSingleNode("States_1"));
        Assert.NotNull(ele.SelectSingleNode("States_2"));
        Assert.Null(ele.SelectSingleNode("States_3")); // 3개 초과 없음
    }

    [Fact]
    public void Symbol_AutoStates_States0_IsWildcard()
    {
        var sym = new SymbolElement
        {
            Id = "S1", VariableName = "Var1", LibrarySymbolName = "CB_Open",
            X = 0, Y = 0, Width = 40, Height = 40,
        };
        var xml = BuildWithSymbol(sym);
        var s0  = GetSymbolEle(xml).SelectSingleNode("States_0")!;

        Assert.Equal("0", s0.SelectSingleNode("Value")!.InnerText);
        Assert.Equal("0", s0.SelectSingleNode("ValueMask")!.InnerText);
        Assert.Equal("zenOn(R) embedded object",
            ((XmlElement)s0).GetAttribute("NODE"));
    }

    [Fact]
    public void Symbol_AutoStates_States1_IsOffExact()
    {
        var sym = new SymbolElement
        {
            Id = "S1", VariableName = "Var1", LibrarySymbolName = "CB_Open",
            X = 0, Y = 0, Width = 40, Height = 40,
        };
        var xml = BuildWithSymbol(sym);
        var s1  = GetSymbolEle(xml).SelectSingleNode("States_1")!;

        Assert.Equal("0",          s1.SelectSingleNode("Value")!.InnerText);
        Assert.Equal("4294967295", s1.SelectSingleNode("ValueMask")!.InnerText);
    }

    [Fact]
    public void Symbol_AutoStates_States2_IsOnExact()
    {
        var sym = new SymbolElement
        {
            Id = "S1", VariableName = "Var1", LibrarySymbolName = "CB_Open",
            X = 0, Y = 0, Width = 40, Height = 40,
        };
        var xml = BuildWithSymbol(sym);
        var s2  = GetSymbolEle(xml).SelectSingleNode("States_2")!;

        Assert.Equal("1",          s2.SelectSingleNode("Value")!.InnerText);
        Assert.Equal("4294967295", s2.SelectSingleNode("ValueMask")!.InnerText);
    }

    // ─── 커스텀 States 사용 ──────────────────────────────────────────

    [Fact]
    public void Symbol_CustomStates_AreUsed()
    {
        var sym = new SymbolElement
        {
            Id = "S2", VariableName = "Var2", LibrarySymbolName = "DS_Open",
            X = 0, Y = 0, Width = 40, Height = 40,
            States = new List<SymbolState>
            {
                new() { Value = 0, ValueMask = 0,          SymbolName = "DS_Open"   },
                new() { Value = 0, ValueMask = 4294967295, SymbolName = "DS_Open"   },
                new() { Value = 1, ValueMask = 4294967295, SymbolName = "DS_Closed" },
            },
        };
        var xml = BuildWithSymbol(sym);
        var ele = GetSymbolEle(xml);

        Assert.Equal("DS_Open",   ele.SelectSingleNode("States_0/SymbolName")!.InnerText);
        Assert.Equal("DS_Open",   ele.SelectSingleNode("States_1/SymbolName")!.InnerText);
        Assert.Equal("DS_Closed", ele.SelectSingleNode("States_2/SymbolName")!.InnerText);
    }
}
