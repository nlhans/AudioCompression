using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioCompression
{
    public struct Constants
    {
        public const int FFT_SIZE = 1024;
    }
    public struct Spectrum
    {
        public float[] Amplitude;
        public float[] Phase; // not in use.

        public Spectrum(int a)
        {
            Amplitude = new float[Constants.FFT_SIZE];
            Phase = new float[Constants.FFT_SIZE];
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            WaveReader read = new WaveReader("sample2.wav");
            WaveWriter write = new WaveWriter("sample2wr.wav", 2, 16, 44100);

            Console.Write("Reading samples...");
            read.ReadSamples();
            Console.WriteLine("OK");
            Console.Write("Writing samples...");
            for(int s = 0; s < read.SamplesCount; s++)
            {
                read.Samples[0][s] /= 10;
                read.Samples[1][s] /= 10;
            }
            write.WriteSamples(read.Samples);
            Console.WriteLine("OK");

            read.Close();
            write.Close();
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}