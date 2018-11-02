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
        
        public static List<TimeInterval> ExtractAudibleTimeIntervals(List<TimeInterval> allSilenceTimeIntervals, double minimumSilenceIntervalToCut = 1.0, 
            double audibleClipSilencePadding = 0.2, bool includeSilenceIntervals = false)
        {
            /* Here's where we'll do the math. We'll test this one extensively before involving FFMPEG.
             */
            List<TimeInterval> longEnoughTimeIntervals = new List<TimeInterval>();

            allSilenceTimeIntervals.ForEach(timeInterval =>
            {
                int indexOfTimeInterval = allSilenceTimeIntervals.IndexOf(timeInterval);

                if (indexOfTimeInterval == 0 || indexOfTimeInterval == allSilenceTimeIntervals.Count - 1)
                    longEnoughTimeIntervals.Add(timeInterval);
                else
                {
                    double? duration = timeInterval.CalculateDuration();

                    if (duration.HasValue && duration >= minimumSilenceIntervalToCut)
                        longEnoughTimeIntervals.Add(timeInterval);
                }
            });

            List<TimeInterval> audibleTimeIntervals = new List<TimeInterval>();

            /* At this point, all silence time intervals >= 1 second AND the first and last time intervals will be added!
             * So now, it's time to construct
             */
            for (int i = 0; i < longEnoughTimeIntervals.Count - 1; i++)
            {
                // Can't just say longEnoughTimeIntervals[i+1]
                TimeInterval timeInterval = longEnoughTimeIntervals[i];
                TimeInterval nextTimeInterval = longEnoughTimeIntervals[i + 1];

                TimeInterval audibleTimeInterval = new TimeInterval(
                    start: Math.Max(0, timeInterval.End.Value - audibleClipSilencePadding),
                    end: nextTimeInterval.Start + audibleClipSilencePadding);

                audibleTimeIntervals.Add(audibleTimeInterval);
            }

            if (!includeSilenceIntervals)
                return audibleTimeIntervals;

            List<TimeInterval> includesSilenceIntervals = new List<TimeInterval>();

            for(int j = 0; j < audibleTimeIntervals.Count - 1; j++)
            {
                includesSilenceIntervals.Add(audibleTimeIntervals[j]);
                TimeInterval nextAudibleTimeInterval = audibleTimeIntervals[j + 1];

                TimeInterval silenceInterval = new TimeInterval(
                    start: audibleTimeIntervals[j].End.Value,
                    end: nextAudibleTimeInterval.Start
                    );

                includesSilenceIntervals.Add(silenceInterval);
            }

            includesSilenceIntervals.Add(audibleTimeIntervals.Last());

            return includesSilenceIntervals;
        }

        public static List<TimeInterval> ExtractAudibleTimeIntervals(string silenceQueryOutput, double minimumSilenceIntervalToCut = 1.0, double audibleClipSilencePadding = 0.2,
            bool includeSilenceIntervals = false)
        {
            List<TimeInterval> allSilenceTimeIntervals = ExtractSilenceTimeIntervals(silenceQueryOutput);

            return ExtractAudibleTimeIntervals(allSilenceTimeIntervals, minimumSilenceIntervalToCut, audibleClipSilencePadding, includeSilenceIntervals);
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
                return firstSilenceTimeInterval.End.Value - 0.3;

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

        /* If this function returns null, it means that we should just copy the clip.
         */
        public static TimeInterval ObtainClipToKeep(string silenceQueryOutput, double fullDurationOfClip, double startTimeThreshhold = 0.5, double endTimeThreshhold = 0.5)
        {
            /* startTimeThrehhold means that if the first silence interval ends before the threshhold, the start of the desired clip will be  0.
             * endTimeThreshhold means that if the last silence interval starts after the threshhold, the end of the desired clip will be fullDurationOfClip.
             */
            List<TimeInterval> allSilenceTimeIntervals = ExtractSilenceTimeIntervals(silenceQueryOutput);

            if (allSilenceTimeIntervals.Count == 0)
                return null;

            TimeInterval timeIntervalToClip = new TimeInterval();

            TimeInterval lastSilenceTimeInterval = allSilenceTimeIntervals[allSilenceTimeIntervals.Count - 1];

            if (lastSilenceTimeInterval.End.HasValue && ((fullDurationOfClip - 2.0) > lastSilenceTimeInterval.End.Value) && (lastSilenceTimeInterval.End.Value - lastSilenceTimeInterval.Start) < 2)
                lastSilenceTimeInterval.End = fullDurationOfClip;
                

            if (allSilenceTimeIntervals.Count == 1)
            {
                timeIntervalToClip = DetermineTimeIntervalToKeepIfOnlyOneSilenceInverval(allSilenceTimeIntervals[0], fullDurationOfClip, startTimeThreshhold, endTimeThreshhold);
            }
            else 
            {              
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
