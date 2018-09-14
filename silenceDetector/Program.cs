using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace silenceDetector
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            double startFile = 0;
            double duration = 0;
            double newDuration = 0;

            string inputFile = "testL.mp4";

            string fileName = "trimmed.mp4";

            Process ffmpeg = new Process();

            ffmpeg.StartInfo.FileName = @"c:\tools\ffmpegnew.exe";
            ffmpeg.StartInfo.Arguments = @"-i c:\temp\testL.mp4 -af silencedetect=noise=-20dB:d=0.5 -f null -";
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.RedirectStandardError = true;
            ffmpeg.Start();
            string output2 = ffmpeg.StandardError.ReadToEnd();
            ffmpeg.WaitForExit();

            

            Regex regx = new Regex("(silence_(start|end): )([-0-9.:]+).*");
            Regex regNum = new Regex("([-0-9.]+)");
            MatchCollection matches = regx.Matches(output2);

            foreach (Match match in matches)
                Console.WriteLine(match.Value);

            Console.ReadKey();

            // silence in the beginning and the end

            if (matches.Count > 2)
            {

                if (matches[1].Value.StartsWith("silence_end"))
                {
                    startFile = Convert.ToDouble(regNum.Match(matches[1].Value).Value) - 0.3;
                    Console.WriteLine("startFile:" + startFile);

                    if (startFile < 0.3)
                        startFile = 0;
                }
                else
                    startFile = 0;

                int endMatchIdx = 0;

                if (matches[matches.Count - 1].Value.StartsWith("silence_start"))
                    endMatchIdx = matches.Count - 1;
                else
                    endMatchIdx = matches.Count - 2;

                double endSilenceBegin = Convert.ToDouble(regNum.Match(matches[endMatchIdx].Value).Value);
                Console.WriteLine("silence end begin:" + endSilenceBegin.ToString());


                duration = endSilenceBegin - startFile + 0.5 ;
                Console.WriteLine("duration:" + duration);

                ffmpeg.StartInfo.Arguments = @"-i c:\temp\" + inputFile + " -ss " + startFile.ToString() + " -c:v libx264 -crf 0 -preset ultrafast -c:a copy -y -t " + duration.ToString() + " c:\\temp\\" + fileName;
                ffmpeg.StartInfo.RedirectStandardError = false;
                ffmpeg.Start();
                ffmpeg.WaitForExit();

                newDuration = duration;
            }

            if (matches.Count > 1)
            {
                startFile = Convert.ToDouble(regNum.Match(matches[1].Value).Value) - 0.3;
                Console.WriteLine("startFile:" + startFile);

                ffmpeg.StartInfo.Arguments = @"-i c:\temp\" + inputFile + "  -ss " + startFile.ToString() + " -c:v libx264 -crf 0 -preset ultrafast -c:a copy -y c:\\temp\\" + fileName;
                ffmpeg.StartInfo.RedirectStandardError = false;
                ffmpeg.Start();
                ffmpeg.WaitForExit();

            }

            
            */

            string outputSample = @"
                silence_start: 10.005CHFJSAD
                silence_end: 10.8XCVCVKSKS
            ";

            //List<TimeInterval> silenceTimes = Helpers.ExtractSilenceTimeIntervals(outputSample);
            TimeInterval timeIntervalToKeep = Helpers.ObtainClipToKeep(outputSample);
            Console.ReadKey();

        }
    }
}
