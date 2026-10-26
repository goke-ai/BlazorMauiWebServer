namespace GokeSharedCore.MapApp
{

    public class DirectionEngine
    {

        public static DirectionRouteMarker BuildRouteMarker(DirectionRouteSeed seed, DateTime timestamp)
        {
            var elapsedSeconds = Math.Max(0, (timestamp - seed.StartedAtUtc).TotalSeconds) + seed.StartOffsetSeconds;
            var phase = elapsedSeconds % (seed.SecondsPerLeg * 2d);
            var forward = phase <= seed.SecondsPerLeg;
            var legProgress = forward
                ? phase / seed.SecondsPerLeg
                : 1d - ((phase - seed.SecondsPerLeg) / seed.SecondsPerLeg);

            var currentLatitude = Lerp(seed.StartLatitude, seed.EndLatitude, legProgress);
            var currentLongitude = Lerp(seed.StartLongitude, seed.EndLongitude, legProgress);
            var percent = (int)Math.Round(legProgress * 100d);
            var directionText = forward ? "toward destination" : "returning";

            return new DirectionRouteMarker
            {
                Id = seed.Id,
                Name = seed.Name,
                TransportMode = seed.TransportMode,
                StartLatitude = seed.StartLatitude,
                StartLongitude = seed.StartLongitude,
                EndLatitude = seed.EndLatitude,
                EndLongitude = seed.EndLongitude,
                CurrentLatitude = currentLatitude,
                CurrentLongitude = currentLongitude,
                Progress = legProgress,
                Description = $"{seed.TransportMode} {directionText} ({percent}%)",
                Timestamp = timestamp.ToLocalTime().ToString("HH:mm:ss"),
                MarkerColor = seed.MarkerColor,
                MarkerShape = seed.MarkerShape,
                MarkerInitials = seed.MarkerInitials
            };
        }



        private static double Lerp(double start, double end, double progress)
        {
            return start + ((end - start) * progress);
        }

    }

    public sealed record DirectionRouteSeed(
               string Id,
               string Name,
               string TransportMode,
               double StartLatitude,
               double StartLongitude,
               double EndLatitude,
               double EndLongitude,
               double SecondsPerLeg,
               double StartOffsetSeconds,
               string MarkerColor,
               string MarkerShape,
               string MarkerInitials,
               DateTime StartedAtUtc);
}
