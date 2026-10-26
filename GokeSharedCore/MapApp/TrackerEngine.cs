using Goke.Core.Map;

namespace GokeSharedCore.MapApp
{
    public class  TrackerEngine
    {
        private const double LatitudeDegreesPerMeter = 1d / 111_320d;
        private readonly Dictionary<string, TrackerMotionState> _motionStates = new(StringComparer.Ordinal);

        public Location BuildSimulatedLocation(Location seedLocation, DateTime timestamp)
        {
            var motionState = GetOrCreateMotionState(seedLocation, timestamp);
            var elapsedSeconds = Math.Max(0, (timestamp - motionState.StartedAtUtc).TotalSeconds);
            var routePosition = elapsedSeconds / motionState.SecondsPerSegment;
            var currentSegment = ((int)Math.Floor(routePosition)) % motionState.Waypoints.Length;
            var nextSegment = (currentSegment + 1) % motionState.Waypoints.Length;
            var segmentProgress = routePosition - Math.Floor(routePosition);
            var currentWaypoint = motionState.Waypoints[currentSegment];
            var nextWaypoint = motionState.Waypoints[nextSegment];
            var latitudeOffsetMeters = Lerp(currentWaypoint.LatitudeOffsetMeters, nextWaypoint.LatitudeOffsetMeters, segmentProgress);
            var longitudeOffsetMeters = Lerp(currentWaypoint.LongitudeOffsetMeters, nextWaypoint.LongitudeOffsetMeters, segmentProgress);
            var latitudeDegrees = latitudeOffsetMeters * LatitudeDegreesPerMeter;
            var longitudeDegrees = longitudeOffsetMeters * GetLongitudeDegreesPerMeter(seedLocation.Latitude);

            return new Location
            {
                Name = seedLocation.Name,
                Latitude = seedLocation.Latitude + latitudeDegrees,
                Longitude = seedLocation.Longitude + longitudeDegrees
            };
        }

        private TrackerMotionState GetOrCreateMotionState(Location seedLocation, DateTime startedAtUtc)
        {
            if (_motionStates.TryGetValue(seedLocation.Name, out var existingState))
            {
                return existingState;
            }

            var hash = Math.Abs(seedLocation.Name.GetHashCode(StringComparison.Ordinal));
            var movementState = new TrackerMotionState(
                startedAtUtc,
                SecondsPerSegment: 4 + (hash % 3),
                Waypoints: CreateRouteWaypoints(hash));

            _motionStates[seedLocation.Name] = movementState;
            return movementState;
        }

        private static RouteWaypoint[] CreateRouteWaypoints(int hash)
        {
            var heading = DegreesToRadians(hash % 360);
            var latitudeOffsetMeters = 0d;
            var longitudeOffsetMeters = 0d;
            var waypoints = new List<RouteWaypoint>
            {
                new(0, 0)
            };

            for (var index = 0; index < 5; index += 1)
            {
                var turnDirection = index % 2 == 0 ? 1 : -1;
                var turnDegrees = 35 + ((hash / ((index + 1) * 11)) % 55);
                var segmentLengthMeters = 18 + ((hash / ((index + 1) * 5)) % 45);

                heading += DegreesToRadians(turnDegrees * turnDirection);
                latitudeOffsetMeters += Math.Sin(heading) * segmentLengthMeters;
                longitudeOffsetMeters += Math.Cos(heading) * segmentLengthMeters;
                waypoints.Add(new(latitudeOffsetMeters, longitudeOffsetMeters));
            }

            return [.. waypoints];
        }

        private static double Lerp(double start, double end, double progress)
        {
            return start + ((end - start) * progress);
        }

        private static double DegreesToRadians(int degrees) => degrees * (Math.PI / 180d);


        private static double GetLongitudeDegreesPerMeter(double latitude)
        {
            var latitudeRadians = latitude * (Math.PI / 180d);
            var cosine = Math.Cos(latitudeRadians);

            if (Math.Abs(cosine) < 0.01)
            {
                cosine = cosine < 0 ? -0.01 : 0.01;
            }

            return LatitudeDegreesPerMeter / cosine;
        }

        private sealed record TrackerMotionState(
            DateTime StartedAtUtc,
            double SecondsPerSegment,
            RouteWaypoint[] Waypoints);

        private sealed record RouteWaypoint(
            double LatitudeOffsetMeters,
            double LongitudeOffsetMeters);

    }
}
