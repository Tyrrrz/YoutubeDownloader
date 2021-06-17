using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using System.Drawing;
using libZPlay;

namespace YoutubeDownloader.Services
{
    public class BPMDetector
    {
        private string filename;
        private double BPM;
        private double sampleRate = 44100;
        private double trackLength = 0;

        public double getBPM()
        {
            return BPM;
        }

        public BPMDetector(string filename, int _samplerate = 44100)
        {
            this.filename = filename;
            this.sampleRate = _samplerate;
            Detect();
        }

        private void Detect()
        {
            try
            {
                ZPlay player = new ZPlay();
                if (player.OpenFile(filename, TStreamFormat.sfAutodetect) == false)
                {
                    // error
                }
                BPM = player.DetectBPM(TBPMDetectionMethod.dmAutoCorrelation);
                player.Close();
                player = null;
            }
            catch (Exception ex)
            {

            }

        }

        private static double rangeQuadSum(short[] samples, int start, int stop)
        {
            double tmp = 0;
            for (int i = start; i <= stop; i++)
            {
                tmp += Math.Pow(samples[i], 2);
            }

            return tmp;
        }

        private static double rangeSum(double[] data, int start, int stop)
        {
            double tmp = 0;
            for (int i = start; i <= stop; i++)
            {
                tmp += data[i];
            }

            return tmp;
        }
    }
}
