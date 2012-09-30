using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioCompression
{
    public struct WavFmt
    {
        public uint ChunkSize;          // Length of header
        public ushort FormatTag;  	    // 1 if uncompressed
        public ushort Channels;         // Number of channels: 1-5
        public uint SamplesPerSec;      // In Hz
        public uint AvgBytesPerSec;     // For estimating RAM allocation
        public ushort BlockAlign;       // Sample frame size in bytes
        public ushort BitsPerSample;      // Bits per sample
    }
    public struct WavRiff
    {
        public uint FileLength;		    // In bytes, measured from offset 8
        public string RiffType;         // WAVE, usually
    }

    public struct WavData
    {
        public uint ChunkSize;
        public long FilePosition;
        public uint NumSamples;
    }
}
