using System;
using System.Collections.Generic;
using System.IO;

namespace AudioCompression
{
    public class WaveWriter
    {
        private BinaryWriter writer;

        private WavRiff Riff;
        private WavFmt Fmt;
        private WavData Data;

        private bool HeaderFlushed = false;
        private int SamplesFlushed = 0;

        public WaveWriter(string file, ushort channels, ushort sample_bits, uint sample_rate)
        {
            writer = new BinaryWriter(new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));

            Riff = new WavRiff();
            Riff.RiffType = "WAVE";

            Fmt = new WavFmt();
            Fmt.AvgBytesPerSec = 176400;
            Fmt.BlockAlign =(ushort) (channels * 2);
            Fmt.FormatTag = 1;
            Fmt.ChunkSize = 16;

            // User configuration:
            Fmt.Channels = channels;
            Fmt.BitsPerSample = sample_bits;
            Fmt.SamplesPerSec = sample_rate;

            Data = new WavData();
            Data.NumSamples = 0;
        }

        public WaveWriter(string file)
            : this(file, 2, 16, 44100)
        {

        }

        public void SetChannels(uint channels)
        {
            if (!HeaderFlushed)
            {
                Fmt.Channels = (ushort)channels;
                Fmt.BlockAlign = (ushort)(channels * 2);
                Fmt.AvgBytesPerSec = Fmt.SamplesPerSec * Fmt.BitsPerSample / 8 * channels;
            }
        }

        public void SetSampleRate(uint rate)
        {
            if (!HeaderFlushed)
            {
                Fmt.SamplesPerSec = rate;
                Fmt.AvgBytesPerSec = Fmt.SamplesPerSec * Fmt.BitsPerSample / 8 * Fmt.Channels;
            }
        }

        public void SetSampleSize(ushort bits)
        {
            if (!HeaderFlushed)
            {
                Fmt.BitsPerSample = bits;
                Fmt.AvgBytesPerSec = Fmt.SamplesPerSec * Fmt.BitsPerSample / 8 * Fmt.Channels;
            }
        }

        public void WriteHeader()
        {
            if (!HeaderFlushed)
            {
                HeaderFlushed = true;

                writer.WriteChars("RIFF");
                writer.Write((UInt32)0);
                writer.WriteChars("WAVE");

                writer.WriteChars("fmt ");
                writer.Write(Fmt.ChunkSize);
                writer.Write(Fmt.FormatTag); // PCM(=1)
                writer.Write(Fmt.Channels);
                writer.Write(Fmt.SamplesPerSec);
                writer.Write(Fmt.AvgBytesPerSec);
                writer.Write(Fmt.BlockAlign);
                writer.Write(Fmt.BitsPerSample);

                writer.WriteChars("data");
                writer.Write((UInt32)0);

            }
        }

        public void WriteSamples(List<List<int>> samples)
        {
            WriteHeader();

            for (int sample = 0; sample < samples.Count; sample++)
            {
                for (int channel = 0; channel < Fmt.Channels; channel++)
                {
                    switch (Fmt.BitsPerSample)
                    {
                        case 8:
                            writer.Write((byte)samples[sample][channel]);
                            break;
                        case 16:
                            writer.Write((ushort)samples[sample][channel]);
                            break;
                        case 32:
                            writer.Write((uint)samples[sample][channel]);
                            break;
                    }

                    SamplesFlushed++;
                }
            }
        }
        public void WriteSamples(List<List<float>> samples)
        {
            // TODO: Test this function.
            WriteHeader();

            for (int sample = 0; sample < Data.NumSamples; sample++)
            {
                for (int channel = 0; channel < Fmt.Channels; channel++)
                {
                    writer.Write(samples[sample][channel]);
                    SamplesFlushed++;
                }
            }
        }

        public void Close()
        {
            // Finalize file size.
            writer.Seek(4, SeekOrigin.Begin);
            writer.Write((UInt32)(36 + SamplesFlushed * Fmt.BitsPerSample / 8));
            writer.Seek(40, SeekOrigin.Begin);
            writer.Write((UInt32)(SamplesFlushed * Fmt.BitsPerSample / 8));

            writer.Close();
        }
    }
}