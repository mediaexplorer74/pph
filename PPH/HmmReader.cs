using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PPH
{
    public class HmmHeader
    {
        public ushort Version { get; set; }
        public uint SaveTime { get; set; }
        public uint RandomSeed { get; set; }
        public byte MapSizeCode { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FileVersion { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public uint CurrentDay { get; set; }
        public ushort Width { get; set; }
        public ushort Height { get; set; }
    }

    public static class HmmReader
    {
        static string ReadWString(DataReader reader)
        {
            // Strings are serialized as: uint32 length, followed by length UTF-16LE code units
            reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16LE;
            uint len = reader.ReadUInt32();
            if (len == 0) return string.Empty;
            return reader.ReadString(len);
        }

        public static async Task<HmmHeader> ReadHeaderAsync(string relativePath = "Data/xl.hmm")
        {
            var uri = new Uri("ms-appx:///" + relativePath.Replace("\\", "/"));
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            using (IRandomAccessStream stream = await file.OpenReadAsync())
            {
                var reader = new DataReader(stream);
                // Load entire file to simplify sequential reads
                await reader.LoadAsync((uint)stream.Size);

                var hdr = new HmmHeader();

                // Basic header
                hdr.Version = reader.ReadUInt16();
                hdr.SaveTime = reader.ReadUInt32();
                hdr.RandomSeed = reader.ReadUInt32();
                hdr.MapSizeCode = reader.ReadByte();

                // Strings
                hdr.Name = ReadWString(reader);
                hdr.Description = ReadWString(reader);
                hdr.FileVersion = ReadWString(reader);
                hdr.Author = ReadWString(reader);

                // Misc fields we skip but read to advance
                hdr.CurrentDay = reader.ReadUInt32(); // curDay
                ushort gameMode = reader.ReadUInt16();
                sbyte difLvl = unchecked((sbyte)reader.ReadByte());

                // Players block (skip)
                ushort pCount = reader.ReadUInt16();
                for (int i = 0; i < pCount; i++)
                {
                    // sint8 playerId + 3x uint8
                    reader.ReadByte(); // playerId
                    reader.ReadByte(); // nation
                    reader.ReadByte(); // playerTypeMask
                    reader.ReadByte(); // playerType
                    // iMineralSet: 7 x sint32
                    for (int m = 0; m < 7; m++) reader.ReadInt32();
                    // curHeroId, curCastleIdx
                    reader.ReadUInt16();
                    reader.ReadUInt16();
                    // keys
                    reader.ReadByte();
                }

                // Current player id (skip value)
                reader.ReadUInt16();

                // Map metrics
                hdr.Width = reader.ReadUInt16();
                hdr.Height = reader.ReadUInt16();

                return hdr;
            }
        }

        public static async Task<string> ReadHeaderSummaryAsync(string relativePath = "Data/xl.hmm")
        {
            try
            {
                var hdr = await ReadHeaderAsync(relativePath);
                var sb = new StringBuilder();
                sb.AppendLine("HMM header");
                sb.AppendLine($"Version: {hdr.Version}");
                sb.AppendLine($"SaveTime: {hdr.SaveTime}");
                sb.AppendLine($"RandomSeed: {hdr.RandomSeed}");
                sb.AppendLine($"MapSize(code): {hdr.MapSizeCode}");
                sb.AppendLine($"Name: {hdr.Name}");
                sb.AppendLine($"Description: {hdr.Description}");
                sb.AppendLine($"FileVersion: {hdr.FileVersion}");
                sb.AppendLine($"Author: {hdr.Author}");
                sb.AppendLine($"CurrentDay: {hdr.CurrentDay}");
                sb.AppendLine($"Width: {hdr.Width}, Height: {hdr.Height}");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return "HMM read error: " + ex.Message;
            }
        }
    }
}
