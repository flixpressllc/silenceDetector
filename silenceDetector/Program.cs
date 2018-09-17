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
            string inputFilename = "testL.mp4";
            string outputFilename = "trimmed.mp4";

            Process ffmpeg = new Process();

            ffmpeg.StartInfo.FileName = @"c:\tools\ffmpegnew.exe";
            ffmpeg.StartInfo.Arguments = @"-i c:\temp\testL.mp4 -af silencedetect=noise=-20dB:d=0.5 -f null -";
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.RedirectStandardError = true;
            ffmpeg.Start();
            string output = ffmpeg.StandardError.ReadToEnd();

            TimeInterval timeIntervalToKeep = Helpers.ObtainClipToKeep(output, 11.5);

            ffmpeg.WaitForExit();

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

            Console.ReadKey();

        }
    }
}
