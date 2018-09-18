using FlixpressFFMPEG.SilenceDetector;
using System;

namespace silenceDetector
{
    class Program
    {
        static void Main(string[] args)
        {
            string ffmpegExecutablePath = @"c:\tools\ffmpegnew.exe";
            string inputFilename = @"c:\temp\testL.mp4";
            string outputFilename = @"c:\temp\trimmed.mp4";
            double fullDurationOfClip = 24.78;
            double startTimeThreshhold = 0.5;
            double endTimeThreshhold = 0.5;

            SilenceDetectorExecutor.Execute(ffmpegExecutablePath, inputFilename, outputFilename, fullDurationOfClip, startTimeThreshhold, endTimeThreshhold);

            Console.ReadKey();

        }
    }
}
