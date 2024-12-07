// ReSharper disable NotAccessedPositionalProperty.Global

namespace SolarTracker.Models;

public record Orientation(
    float Azimuth,
    float Altitude,
    DateTime ValidUntil,
    DateTime Timestamp);