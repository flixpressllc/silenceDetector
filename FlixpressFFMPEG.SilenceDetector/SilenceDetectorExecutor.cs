using System.Diagnostics;

namespace FlixpressFFMPEG.SilenceDetector
{
    public static class SilenceDetectorExecutor
    {
        public static void Execute(string ffmpegExecutablePath, string inputFilename, string outputFilename,
            double fullDurationOfClip, double startTimeThreshhod = 0.5, double endTimeThreshhold = 0.5)
        {
            Process ffmpeg = new Process();

            ffmpeg.StartInfo.FileName =
            ffmpeg.StartInfo.Arguments = @"-i c:\temp\testL.mp4 -af silencedetect=noise=-20dB:d=0.5 -f null -";
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.RedirectStandardError = true;
            ffmpeg.Start();
            string output = ffmpeg.StandardError.ReadToEnd();
            ffmpeg.WaitForExit();

            TimeInterval timeIntervalToKeep = Helpers.ObtainClipToKeep(output, fullDurationOfClip, startTimeThreshhod, endTimeThreshhold);

            if (timeIntervalToKeep == null)
            {
                ffmpeg.StartInfo.Arguments = @"-i c:\temp\" + inputFilename + "  -ss 0.3 -c:v libx264 -crf 0 -preset ultrafast -c:a copy -y c:\\temp\\" + outputFilename;
                ffmpeg.StartInfo.RedirectStandardError = false;
                ffmpeg.Start();
                ffmpeg.WaitForExit();
            }
            else
            {
                ffmpeg.StartInfo.Arguments = @"-i c:\temp\" + inputFilename + " -ss " + timeIntervalToKeep.Start + " -c:v libx264 -crf 0 -preset ultrafast -c:a copy -y -to " + (timeIntervalToKeep.End + 0.5) + " c:\\temp\\" + outputFilename;
                ffmpeg.StartInfo.RedirectStandardError = false;
                ffmpeg.Start();
                ffmpeg.WaitForExit();
            }
        }
    }
}
