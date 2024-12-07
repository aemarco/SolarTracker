// ReSharper disable NotAccessedPositionalProperty.Global

namespace SolarTracker.Models.Api;

public record StateInfo(
    StateProvider State,
    DateTime Timestamp);