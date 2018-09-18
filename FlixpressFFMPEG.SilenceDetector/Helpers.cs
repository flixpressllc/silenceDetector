using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FlixpressFFMPEG.SilenceDetector
{
    public static class Helpers
    {
        public static List<TimeInterval> ExtractSilenceTimeIntervals(string silenceQueryOutput)
        {
            List<TimeInterval> silenceTimeIntervals = new List<TimeInterval>();

            Regex regx = new Regex("(silence_(start|end): )([-0-9.:]+)*");
            Regex regNum = new Regex("([-0-9.]+)");
            MatchCollection matches = regx.Matches(silenceQueryOutput);

            foreach (Match match in matches)
                Console.WriteLine(match.Value);

            for(int i = 0; i < matches.Count; i += 2)
            {
                Match startMatch = matches[i];
                Match endMatch = (i + 1 < matches.Count) ? matches[i + 1] : null;

                /* Convert.ToDouble(regNum.Match(matches[1].Value).Value)
                 */
                TimeInterval timeInterval = new TimeInterval(
                    start: Convert.ToDouble(regNum.Match(startMatch.Value).Value),
                    end: (endMatch != null) ? Convert.ToDouble(regNum.Match(endMatch.Value).Value) : default(double?));

                silenceTimeIntervals.Add(timeInterval);
            }

            return silenceTimeIntervals;
        }

        private static double DetermineEndTimeOfClipToKeep(TimeInterval lastSilenceTimeInterval, double fullDurationOfClip, double endTimeThreshhold)
        {
            double endThreshholdMark = fullDurationOfClip - endTimeThreshhold;

            if (lastSilenceTimeInterval.Start < endThreshholdMark)
            {
                if (!lastSilenceTimeInterval.End.HasValue)
                    return lastSilenceTimeInterval.Start;

                if (lastSilenceTimeInterval.End.Value > endThreshholdMark)
                    return fullDurationOfClip;
                else
                    return lastSilenceTimeInterval.Start;
            }

            return fullDurationOfClip;
        }

        private static double DetermineStartTimeOfClipToKeep(TimeInterval firstSilenceTimeInterval, double startTimeThreshhold)
        {
            if (firstSilenceTimeInterval.Start > startTimeThreshhold)
                return 0;
            
            if (firstSilenceTimeInterval.End.HasValue && firstSilenceTimeInterval.End.Value > startTimeThreshhold)
                return firstSilenceTimeInterval.End.Value;

            return 0;
        }
        
        private static TimeInterval DetermineTimeIntervalToKeepIfOnlyOneSilenceInverval(TimeInterval loneSilenceTimeInterval, double fullDurationOfClip, double startTimeThreshhold,
            double endTimeThreshhold)
        {
            double endThreshholdMark = fullDurationOfClip - endTimeThreshhold;

            if (loneSilenceTimeInterval.Start > startTimeThreshhold)
            {
                if (!loneSilenceTimeInterval.End.HasValue)
                    return new TimeInterval()
                    {
                        Start = 0,
                        End = loneSilenceTimeInterval.Start
                    };
                else if (loneSilenceTimeInterval.End.Value < endThreshholdMark)
                    return new TimeInterval()
                    {
                        Start = 0,
                        End = fullDurationOfClip
                    };
            }                

            return new TimeInterval
            {
                Start = (loneSilenceTimeInterval.Start < startTimeThreshhold) ? 0 : loneSilenceTimeInterval.Start,
                End = DetermineEndTimeOfClipToKeep(loneSilenceTimeInterval, fullDurationOfClip, endTimeThreshhold)
            };
        }

        private static void RemoveLastFewShortTimeIntervals(List<TimeInterval> allSilenceTimeIntervals, double cutoffPoint)
        {
            List<TimeInterval> timeIntervalsToDelete = allSilenceTimeIntervals.Where(ti => ti.Start > cutoffPoint).ToList();

            foreach(TimeInterval timeIntervalToDelete in timeIntervalsToDelete)
            {
                allSilenceTimeIntervals.Remove(timeIntervalToDelete);
            }
        }

        /* If this function returns null, it means that we should just copy the clip.
         */
        public static TimeInterval ObtainClipToKeep(string silenceQueryOutput, double fullDurationOfClip, double startTimeThreshhold = 0.5, double endTimeThreshhold = 0.5)
        {
            /* startTimeThrehhold means that if the first silence interval ends before the threshhold, the start of the desired clip will be  0.
             * endTimeThreshhold means that if the last silence interval starts after the threshhold, the end of the desired clip will be fullDurationOfClip.
             */
            List<TimeInterval> allSilenceTimeIntervals = ExtractSilenceTimeIntervals(silenceQueryOutput);

            RemoveLastFewShortTimeIntervals(allSilenceTimeIntervals, fullDurationOfClip - 2); // Remove silence intervals that start during the last 2 seconds of the clip.

            if (allSilenceTimeIntervals.Count == 0)
                return null;

            TimeInterval timeIntervalToClip = new TimeInterval();

            if (allSilenceTimeIntervals.Count == 1)
            {
                timeIntervalToClip = DetermineTimeIntervalToKeepIfOnlyOneSilenceInverval(allSilenceTimeIntervals[0], fullDurationOfClip, startTimeThreshhold, endTimeThreshhold);
            }
            else 
            {
                /* If we have more than one silence intervals, we'll need to take the first interval's 
                 * end value and the last silent time interval's start.
                 */

                /* We will need to chop off the last n time intervals whose start time is beyond three seconds to the end.
                 */

                int indexOfLastInterval = allSilenceTimeIntervals.Count - 1;

                timeIntervalToClip = new TimeInterval
                {
                    Start = DetermineStartTimeOfClipToKeep(allSilenceTimeIntervals[0], startTimeThreshhold),
                    End = DetermineEndTimeOfClipToKeep(allSilenceTimeIntervals[indexOfLastInterval], fullDurationOfClip, endTimeThreshhold)
                };       
            }

            if (timeIntervalToClip.Start == 0 && timeIntervalToClip.End == fullDurationOfClip)
                return null;
            
            return timeIntervalToClip;
        }
    }
}
