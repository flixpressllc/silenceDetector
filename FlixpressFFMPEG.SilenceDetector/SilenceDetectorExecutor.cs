﻿using System.Diagnostics;

namespace FlixpressFFMPEG.SilenceDetector
{
    public static class SilenceDetectorExecutor
    {
        public static string ExecuteForSilenceIntervalsOutput(string ffmpegExecutablePath, string inputFilename)
        {
            Process ffmpeg = new Process();
            ffmpeg.StartInfo.FileName = ffmpegExecutablePath;
            ffmpeg.StartInfo.Arguments = @"-i " + inputFilename + " -af silencedetect=noise=-15dB:d=0.5 -f null -";
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.RedirectStandardError = true;
            ffmpeg.Start();
            string output = ffmpeg.StandardError.ReadToEnd();
            ffmpeg.WaitForExit();

            return output;
        }

        public static double Execute(string ffmpegExecutablePath, string inputFilename, string outputFilename,
            double fullDurationOfClip, double startTimeThreshhod = 0.5, double endTimeThreshhold = 0.5)
        {
            string output = ExecuteForSilenceIntervalsOutput(ffmpegExecutablePath, inputFilename);

            TimeInterval timeIntervalToKeep = Helpers.ObtainClipToKeep(output, fullDurationOfClip, startTimeThreshhod, endTimeThreshhold);

            Process ffmpeg = new Process();
            ffmpeg.StartInfo.FileName = ffmpegExecutablePath;

            if (timeIntervalToKeep == null)
            {
                ffmpeg.StartInfo.Arguments = @"-i " + inputFilename + "  -ss 0.3 -c:v libx264 -crf 0 -preset ultrafast -c:a copy -y " + outputFilename;
                ffmpeg.StartInfo.RedirectStandardError = false;
                ffmpeg.Start();
                ffmpeg.WaitForExit();

                return fullDurationOfClip;
            }
            else
            {
                ffmpeg.StartInfo.Arguments = @"-i " + inputFilename + " -ss " + timeIntervalToKeep.Start  + " -c:v libx264 -crf 0 -preset ultrafast -c:a copy -y -to " + (timeIntervalToKeep.End + 0.5) + " " + outputFilename;
                ffmpeg.StartInfo.RedirectStandardError = false;
                ffmpeg.Start();
                ffmpeg.WaitForExit();

                return (timeIntervalToKeep.End.Value + 0.5 - timeIntervalToKeep.Start);
            }
        }
    }
}
