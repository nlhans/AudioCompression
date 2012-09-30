using System.IO;
using System.Text;

namespace AudioCompression
{
    public static partial class Extensions
    {
        public static string ReadString(this BinaryReader reader, int length)
        {
            byte[] bf = reader.ReadBytes(length);
            return ASCIIEncoding.ASCII.GetString(bf);
        }
        public static string PeekString(this BinaryReader reader, int length)
        {
            long seek = reader.BaseStream.Position;
            byte[] bf = reader.ReadBytes(length);
                reader.BaseStream.Seek(seek, SeekOrigin.Begin);
            return ASCIIEncoding.ASCII.GetString(bf);
        }

        public static void WriteChars(this BinaryWriter writer, string str)
        {
            byte[] bf = ASCIIEncoding.ASCII.GetBytes(str);
            writer.Write(bf);

        }
    }
}