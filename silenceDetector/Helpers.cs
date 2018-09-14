using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace silenceDetector
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

        public static TimeInterval ObtainClipToKeep(string silenceQueryOutput, double fullDurationOfClip)
        {
            List<TimeInterval> allSilenceTimeIntervals = ExtractSilenceTimeIntervals(silenceQueryOutput);

            if (allSilenceTimeIntervals.Count == 0)
                return null;

            if (allSilenceTimeIntervals.Count == 1)
            {

                return new TimeInterval();
            }
            else 
            {
                /* If we have more than one silence intervals, we'll need to take the first interval's 
                 * 
                 */

                int indexOfLastInterval = allSilenceTimeIntervals.Count - 1;

                return new TimeInterval
                {
                    Start = allSilenceTimeIntervals[0].End.Value,
                    End = allSilenceTimeIntervals[indexOfLastInterval].Start
                };
            
            }
        }
    }
}
