using System.Text.Json.Serialization;

namespace ZenonXmlGenerator.Models;

/// <summary>
/// 전력 단선도(SLD) 기기 유형.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DeviceType
{
    /// <summary>모선 (Busbar: Line TYPE="101", LineWidth="5", ALCUseColor="TRUE")</summary>
    Busbar,

    /// <summary>차단기 (Circuit Breaker: Combined element TYPE="16", ALCType="2")</summary>
    CircuitBreaker,

    /// <summary>단로기 (Disconnector: Combined element TYPE="16", ALCType="7")</summary>
    Disconnector,

    /// <summary>주변압기 (Transformer: Combined element TYPE="16", ALCType="4")</summary>
    Transformer,

    /// <summary>변류기 (Current Transformer: Combined element TYPE="16")</summary>
    CurrentTransformer,

    /// <summary>계기용변압기 (Potential Transformer: Combined element TYPE="16")</summary>
    PotentialTransformer,

    /// <summary>접지개폐기 (Earth Switch: Combined element TYPE="16")</summary>
    EarthSwitch,
}
