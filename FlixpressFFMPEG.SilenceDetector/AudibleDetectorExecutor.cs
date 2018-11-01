using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FlixpressFFMPEG.SilenceDetector
{
    public static class AudibleDetectorExecutor
    {
        private static void ExtractAudible(string ffmpegExecutablePath, TimeInterval timeIntervalToKeep, string inputFilename, string outputFilename)
        {
            Process ffmpeg = new Process();
            ffmpeg.StartInfo.FileName = ffmpegExecutablePath;
            ffmpeg.StartInfo.Arguments = @"-i " + inputFilename + " -ss " + timeIntervalToKeep.Start + " -c:v libx264 -crf 0 -preset ultrafast -c:a copy -y -to " + (timeIntervalToKeep.End + 0.5) + " " + outputFilename;
            ffmpeg.StartInfo.RedirectStandardError = false;
            ffmpeg.Start();
            ffmpeg.WaitForExit();

        }

        public static void Execute(string ffmpegExecutablePath, string inputFilename, double minimumSilenceIntervalToCut = 1, double audibleClipSilencePadding = 0.2)
        {   
            string output = SilenceDetectorExecutor.ExecuteForSilenceIntervalsOutput(ffmpegExecutablePath, inputFilename);

            List<TimeInterval> audibleTimeIntervals = Helpers.ExtractAudibleTimeIntervals(output, minimumSilenceIntervalToCut, audibleClipSilencePadding);

            int idx = 1;
            audibleTimeIntervals.ForEach(timeInterval =>
            {
                // Formulate the output Filename.
                string outputFilename = $"{Path.GetDirectoryName(inputFilename)}\\{Path.GetFileNameWithoutExtension(inputFilename)}_{idx}{Path.GetExtension(inputFilename)}";

                ExtractAudible(ffmpegExecutablePath, timeInterval, inputFilename, outputFilename);

                idx++;
            });
        }
    }
}
