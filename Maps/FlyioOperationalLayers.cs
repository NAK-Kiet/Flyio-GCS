using System;
using System.Collections.Generic;
using GMap.NET;

namespace MissionPlanner.Maps
{
    // Data-only contracts for future authorized providers. They perform no I/O and issue no vehicle commands.
    public sealed class FlyioAircraftSnapshot
    {
        public string AircraftId { get; set; }
        public string Callsign { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double GroundSpeed { get; set; }
        public double Heading { get; set; }
        public double VerticalRate { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum FlyioOperationalZoneType
    {
        NoFly,
        Restricted,
        Warning,
        Authorized
    }

    public sealed class FlyioOperationalZone
    {
        public string ZoneId { get; set; }
        public string Name { get; set; }
        public FlyioOperationalZoneType Type { get; set; }
        public IList<PointLatLng> Polygon { get; set; } = new List<PointLatLng>();
        public double? FloorAltitude { get; set; }
        public double? CeilingAltitude { get; set; }
        public bool Active { get; set; }
        public string Description { get; set; }
    }

    public static class FlyioOperationalLayerNames
    {
        public const string BaseMap = "BaseMapLayer";
        public const string Mission = "MissionLayer";
        public const string Vehicle = "VehicleLayer";
        public const string Aircraft = "AircraftLayer";
        public const string AircraftTrail = "AircraftTrailLayer";
        public const string Airspace = "AirspaceLayer";
        public const string RestrictedZone = "RestrictedZoneLayer";
        public const string WarningZone = "WarningZoneLayer";
        public const string DrdcStation = "DRDCStationLayer";
        public const string Weather = "WeatherLayer";
        public const string CustomOverlay = "CustomOverlayLayer";
    }
}
