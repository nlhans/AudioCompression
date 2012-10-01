using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AudioCompression
{
    public class WaveReader
    {
        private BinaryReader reader;

        private WavRiff Riff;
        private WavFmt Fmt;
        private WavData Data;
        private bool fact = false;

        public List<List<int>> Samples = new List<List<int>>();

        public WaveReader(string file)
        {
            Riff = new WavRiff();
            Fmt = new WavFmt();
            Data = new WavData();

            reader = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read));

            ReadHeader();
        }

        public uint SamplesCount
        {
            get { return (uint)this.Samples[0].Count; }
        }

        public void ReadHeader()
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin); // set to start
            bool datafound = false;
            while (datafound == false)
            {
                if (reader.BaseStream.Position > 256) // header of 256 bytes? c'mon
                    return;

                if (reader.PeekString(4).ToLower() == "riff")
                {
                    reader.ReadString(4);//RIFF
                    Riff.FileLength = 8 + reader.ReadUInt32();
                    Riff.RiffType = reader.ReadString(4); // WAVE
                }
                else if (reader.PeekString(4).ToLower() == "fmt ")
                {
                    reader.ReadString(4);//fmt_

                    Fmt.ChunkSize = reader.ReadUInt32();
                    Fmt.FormatTag = reader.ReadUInt16();
                    Fmt.Channels = reader.ReadUInt16();
                    Fmt.SamplesPerSec = reader.ReadUInt32();
                    Fmt.AvgBytesPerSec = reader.ReadUInt32();
                    Fmt.BlockAlign = reader.ReadUInt16();
                    Fmt.BitsPerSample = reader.ReadUInt16();

                }
                else if (reader.PeekString(4).ToLower() == "data")
                {
                    reader.ReadString(4);//DATA
                    Data.ChunkSize = reader.ReadUInt32();
                    Data.FilePosition = reader.BaseStream.Position;
                    if (fact)
                        Data.NumSamples = 0; // Missed it.. TODO: Implement fact reader
                    else
                        Data.NumSamples = Data.ChunkSize/ ((uint)(Fmt.Channels * Fmt.BitsPerSample/ 8));

                    datafound = true;
                }
                else
                {
                    reader.ReadByte(); // set 1 foward
                }
            }

        }

        public void ReadSamples()
        {
            ReadHeader(); // read header for good measure.

            List<List<int>> Samples_t = new List<List<int>>();
            for (int d = 0, sample = 0; sample < Data.NumSamples; sample++)
            {
                Samples_t.Add(new List<int>());
                for (int channel = 0; channel < Fmt.Channels; channel++)
                {
                    switch (Fmt.BitsPerSample)
                    {
                        case 8:
                            d = reader.ReadByte();
                            break;
                        case 16:
                            d = reader.ReadInt16();
                            break;
                        case 32:
                            d = reader.ReadInt32();
                            break;
                    }

                    Samples_t[sample].Add(d);

                }
            }

            // Shuffle lists around.
            for (int c = 0; c < Fmt.Channels; c++)
            {
                Samples.Add(new List<int>());
                for(int s = 0; s < Data.NumSamples; s++)
                    Samples[c].Add(Samples_t[s][c]);
            }


        }

        public void Close()
        {
            reader.Close();
        }
    }
}