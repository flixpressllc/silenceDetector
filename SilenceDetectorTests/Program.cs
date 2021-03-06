﻿using FlixpressFFMPEG.SilenceDetector;
using System;
using System.Collections.Generic;
using System.IO;

namespace SilenceDetectorTests
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            List<TimeInterval> silenceIntervals = new List<TimeInterval>
            {
                new TimeInterval(0, 0.1),
                new TimeInterval(10, 13),
                new TimeInterval(22, 22.5),
                new TimeInterval(37, 40.5),
                new TimeInterval(57, null)
            };

            List<TimeInterval> audibleTimeIntervals = Helpers.ExtractAudibleTimeIntervals(silenceIntervals);
            
            var dummy = 15;
            */
            string ffmpegExecutablePath = @"c:\tools\ffmpegnew.exe";
            string inputFilename = @"c:\temp\audiblesplit\SoundTest.mp4";
            double minimalSilenceIntervalToCut = 1;
            double audibleClipSilencePadding = 0.2;

            AudibleDetectorExecutor.Execute(ffmpegExecutablePath, inputFilename, minimalSilenceIntervalToCut, audibleClipSilencePadding);

            Console.ReadLine();
        }
    }
}
