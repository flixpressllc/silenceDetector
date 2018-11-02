namespace FlixpressFFMPEG.SilenceDetector
{
    public class TimeInterval
    {
        public double Start { get; set; }
        public double? End { get; set; }

        public TimeInterval()
        {
        }

        public TimeInterval(double start, double? end)
        {
            Start = start;
            End = end;
        }

        public double? CalculateDuration()
        {
            if (End.HasValue)
                return End - Start;

            return null;
        }
    }
}
