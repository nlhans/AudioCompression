using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace AudioCompression
{
    public struct Constants
    {
        public const int FFT_SIZE = 1024;
        public const float AUDIO_SIZE = 65536.0f/2.0f;
    }
    public struct Spectrum
    {
        private Complex[] d;
        public Complex[] ExportArray()
        {
            return ExportList().ToArray();
        }

        public List<Complex> ExportList()
        {
            List<Complex> Data = new List<Complex>();
            Data.AddRange(d);
            Data[0] = new Complex(0, 0);
            return Data;
        }
        public Spectrum(Complex[] data)
        {
            d = data;
        }

    }

    public class AudioData
    {
        private List<Complex> ComplexSamples = new List<Complex>();

        public void Add(List<int> Samples)
        {
            try
            {
                for (int s = 0; s < Samples.Count; s++)
                    ComplexSamples.Add(new Complex(Convert.ToSingle(Samples[s]/2)/Constants.AUDIO_SIZE, 0.0f));
            }
            catch (Exception)
            {
            }
        }

        public void Add(List<Complex> Samples)
        {
            try
            {
                ComplexSamples.AddRange(Samples);
            }catch(Exception)
            {
            }
        }

        public void Add(Complex[] Samples)
        {
            Add(Samples.OfType<Complex>().ToList());
        }
        public void Add(List<int> Samples, int start, int size)
        {
            for (int s = start; s < start + size; s++)
            {
                if (Samples.Count < start + size)
                    ComplexSamples.Add(new Complex(0.0f, 0.0f));
                else
                    ComplexSamples.Add(new Complex(Convert.ToSingle(Samples[s]) / Constants.AUDIO_SIZE, 0.0f));
            }
        }

        public List<Complex> Grab(int start, int size)
        {
            List<Complex> samples = new List<Complex>();
            for (int s = start; s < start+size; s++)
            {
                if(ComplexSamples.Count < start+size)
                    samples.Add(new Complex(0.0f, 0.0f));
                else
                    samples.Add(ComplexSamples[s]);
            }
            return samples;
        }

        public List<int> ExportInt()
        {
            List<int> Samples = new List<int>();

            for(int s = 0; s < ComplexSamples.Count; s++)
            {
                // Skip the phase:
                Samples.Add((int)(Constants.AUDIO_SIZE * ComplexSamples[s].Real));

            }
            return Samples;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            WaveReader read = new WaveReader("sample.wav");
            Console.Write("Reading samples...");
            read.ReadSamples();
            Console.WriteLine("OK");
            read.Close();
            Console.Write("Processing...");
            List<AudioData> Channels_In = new List<AudioData>();
            List<AudioData> Channels_Out = new List<AudioData>();

            List<List<Spectrum>> Spectrums = new List<List<Spectrum>>();
            for (int channel = 0; channel < read.Channels; channel++)
            {
                Channels_In.Add(new AudioData());
                Channels_Out.Add(new AudioData());
                Channels_In[channel].Add(read.Samples[channel]);
                Spectrums.Add(new List<Spectrum>());
            }

            int sections =Convert.ToInt32(Math.Ceiling((decimal)(read.SamplesCount/Constants.FFT_SIZE)));

            for (int section = 0; section < sections; section++)
            {
                int start = section*Constants.FFT_SIZE;
                for (int channel = 0; channel < Channels_In.Count; channel++)
                {
                    Complex[] data = Channels_In[channel].Grab(start, Constants.FFT_SIZE).ToArray();
                    Transform.FourierForward(data);
                    Spectrums[channel].Add(new Spectrum(data));
                }

            }

            for (int spectrum = 0; spectrum < Spectrums[0].Count; spectrum++)
            {
                for(int channel =0 ; channel < Channels_Out.Count; channel++)
                {
                    Complex[] data = Spectrums[channel][spectrum].ExportArray();
                    Transform.FourierInverse(data);
                    Channels_Out[channel].Add(data);
                }
            }

            // EXPORT
            List<List<int>> Samples = new List<List<int>>();

            for(int channel = 0; channel < Channels_Out.Count; channel++)
            {
                Samples.Add(Channels_Out[channel].ExportInt());
            }
            Console.WriteLine("OK");






            Console.WriteLine("Waiting for file access..");
            while(true)
            {
                try
                {
                    BinaryWriter w= new BinaryWriter(new FileStream("sample2wr.wav", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));
                    w.Close();
                    break;
                }
                catch(Exception)
                {
                }
                System.Threading.Thread.Sleep(100);

            }
            Console.WriteLine("OK");
                
            WaveWriter write = new WaveWriter("sample2wr.wav", 2, 16, 44100);

            Console.Write("Writing samples...");
            write.WriteSamples(Samples);
            Console.WriteLine("OK");

            write.Close();


            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}