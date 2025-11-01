using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PPH
{
    public class HmmHeader
    {
        public uint Version { get; set; }
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
        public bool IsEditorMap { get; set; }
        public uint RawFourCC { get; set; }
        public string ParseStatus { get; set; } = string.Empty;
    }

    public static class HmmReader
    {
        const uint GMAP_FILE_HDR_KEY = ((uint)'G') | ((uint)'M' << 8) | ((uint)'A' << 16) | ((uint)'P' << 24);
        const uint EMAP_FILE_HDR_KEY = 0x76235278; // editor map

        static int BitCount(uint mask)
        {
            int c = 0;
            while (mask != 0)
            {
                mask &= (mask - 1);
                c++;
            }
            return c;
        }

        static string ReadWString(DataReader reader)
        {
            // Формат строк в .phm/.hmm: uint32 длина (в кодовых единицах iCharT),
            // затем len UTF-16LE кодовых единиц без отдельного нулевого терминатора.
            // Читаем вручную как байты, декодируем Unicode (UTF-16LE) и отрезаем завершающий нуль, если он присутствует.
            uint len = reader.ReadUInt32();
            if (len == 0) return string.Empty;

            // Каждая кодовая единица UTF-16LE = 2 байта
            uint bytesToRead = checked(len * 2);
            // Защитимся от некорректного значения len относительно оставшегося буфера
            if (reader.UnconsumedBufferLength < bytesToRead)
            {
                throw new Exception($"Недостаточно данных для чтения строки: требуется {bytesToRead} байт, доступно {reader.UnconsumedBufferLength} байт.");
            }

            byte[] data = new byte[bytesToRead];
            reader.ReadBytes(data);
            string s = Encoding.Unicode.GetString(data);
            return s.TrimEnd('\0');
        }

        public static async Task<HmmHeader> ReadHeaderAsync(string relativePath = "Data/xl.hmm")
        {
            // Пытаемся открыть из LocalFolder, иначе читаем из пакета
            StorageFile file = await RuntimeAssets.TryGetLocalFileAsync(relativePath);
            if (file == null)
            {
                var uri = new Uri("ms-appx:///" + relativePath.Replace("\\", "/"));
                file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            }
            using (IRandomAccessStream stream = await file.OpenReadAsync())
            {
                var reader = new DataReader(stream);
                // Load entire file to simplify sequential reads
                await reader.LoadAsync((uint)stream.Size);

                var hdr = new HmmHeader();

                // Первые 4 байта: либо 'GMAP', либо EMAP_FILE_HDR_KEY
                uint fourcc = reader.ReadUInt32();

                // Проверка сигнатуры fourcc
                if (fourcc == EMAP_FILE_HDR_KEY)
                {
                    // Editor map (.hmm из редактора)
                    hdr.IsEditorMap = true;
                    hdr.RawFourCC = fourcc;
                    hdr.ParseStatus = "EMAP";
                    hdr.Version = reader.ReadUInt32(); // EMAP_FILE_VERSION (0x19)
                    hdr.MapSizeCode = reader.ReadByte();

                    // По умолчанию английский язык для старых версий
                    uint lngMask = 1u << 0; // GLNG_ENGLISH

                    if (hdr.Version > 0x18)
                    {
                        lngMask = reader.ReadUInt32();
                        uint cnt = reader.ReadUInt32();
                        for (uint i = 0; i < cnt; i++)
                        {
                            string key = ReadWString(reader);
                            short tet = reader.ReadInt16();
                            int langs = BitCount(lngMask);
                            string firstText = string.Empty;
                            for (int l = 0; l < langs; l++)
                            {
                                string txt = ReadWString(reader);
                                if (l == 0) firstText = txt; // текст для языка с минимальным индексом
                            }

                            if (string.Equals(key, "Map name", StringComparison.Ordinal))
                            {
                                hdr.Name = firstText;
                            }
                            else if (string.Equals(key, "Map Description", StringComparison.Ordinal))
                            {
                                hdr.Description = firstText;
                            }
                        }
                    }
                    else
                    {
                        // Старые версии: имя и описание записаны напрямую
                        hdr.Name = ReadWString(reader);
                        hdr.Description = ReadWString(reader);
                    }

                    if (hdr.Version >= 0x15)
                    {
                        hdr.FileVersion = ReadWString(reader); // map_version
                        hdr.Author = ReadWString(reader);       // map_author
                    }

                    // В EMAP ширина/высота задаются кодом размера карты (MAP_SIZ_SIZE)
                    // 0->32, 1->64, 2->128, 3->256
                    int size;
                    switch (hdr.MapSizeCode)
                    {
                        case 0: size = 32; break;
                        case 1: size = 64; break;
                        case 2: size = 128; break;
                        case 3: size = 256; break;
                        default: size = 0; break;
                    }
                    hdr.Width = (ushort)size;
                    hdr.Height = (ushort)size;

                    // Остальной бинарный контент (время событий, игроки, герои, дамп карты) пропускаем в заголовке
                    return hdr;
                }
                else if (fourcc == GMAP_FILE_HDR_KEY)
                {
                    // Игровой снимок (GMAP): прежняя логика
                    hdr.IsEditorMap = false;
                    hdr.RawFourCC = fourcc;
                    hdr.ParseStatus = "GMAP";
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
                else
                {
                    // Unknown header FourCC: fall back without throwing
                    hdr.IsEditorMap = false;
                    hdr.RawFourCC = fourcc;
                    hdr.ParseStatus = "Unknown";
                    hdr.Version = 0;
                    hdr.SaveTime = 0;
                    hdr.RandomSeed = 0;
                    hdr.MapSizeCode = 0;
                    hdr.Name = string.Empty;
                    hdr.Description = string.Empty;
                    hdr.FileVersion = string.Empty;
                    hdr.Author = string.Empty;
                    hdr.CurrentDay = 0;
                    hdr.Width = 0;
                    hdr.Height = 0;
                    return hdr;
                }
            }
        }

        public static async Task<string> ReadHeaderSummaryAsync(string relativePath = "Data/xl.hmm")
        {
            try
            {
                var hdr = await ReadHeaderAsync(relativePath);
                var sb = new StringBuilder();
                sb.AppendLine("HMM header");
                sb.AppendLine($"FourCC: 0x{hdr.RawFourCC:X8}");
                var format = hdr.ParseStatus == "Unknown" ? "Unknown" : (hdr.IsEditorMap ? "EMAP" : "GMAP");
                sb.AppendLine($"Format: {format}");
                sb.AppendLine($"ParseStatus: {hdr.ParseStatus}");
                sb.AppendLine($"Version: {hdr.Version}");
                sb.AppendLine($"SaveTime: {hdr.SaveTime}");
                sb.AppendLine($"RandomSeed: {hdr.RandomSeed}");
                sb.AppendLine($"MapSize(code): {hdr.MapSizeCode}");
                sb.AppendLine($"Name: {hdr.Name}");
                sb.AppendLine($"Description: {hdr.Description}");
                sb.AppendLine($"FileVersion: {hdr.FileVersion}");
                sb.AppendLine($"Author: {hdr.Author}");
                sb.AppendLine($"CurrentDay: {hdr.CurrentDay}");
                if (hdr.Width == 0 || hdr.Height == 0)
                {
                    sb.AppendLine("Map size: Unknown");
                }
                else
                {
                    sb.AppendLine($"Width: {hdr.Width}, Height: {hdr.Height}");
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return "HMM read error: " + ex.Message;
            }
        }
    }
}
