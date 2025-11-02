using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PPH
{
    // Простой ридер индекса паков: читает первые байты и валидирует магик
    public static class PackIndexReader
    {
        public sealed class MagicValidationResult
        {
            public byte[] MagicBytes { get; set; } = Array.Empty<byte>();
            public string MagicAscii { get; set; } = string.Empty;
            public bool IsValid { get; set; }
        }

        // Валидация магик-байт: читаем первые 4 байта файла
        public static async Task<MagicValidationResult> ValidateMagicAsync(StorageFile file, byte[] expectedMagic = null)
        {
            using (var stream = await file.OpenStreamForReadAsync())
            {
                var buf = new byte[4];
                int read = await stream.ReadAsync(buf, 0, buf.Length);
                var ascii = SafeAscii(buf, read);

                bool isNonZero = read == 4 && !(buf[0] == 0 && buf[1] == 0 && buf[2] == 0 && buf[3] == 0);
                bool matchesExpected = expectedMagic == null || (read == expectedMagic.Length && BytesEqual(buf, expectedMagic));

                return new MagicValidationResult
                {
                    MagicBytes = buf,
                    MagicAscii = ascii,
                    IsValid = isNonZero && matchesExpected
                };
            }
        }

        static bool BytesEqual(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
            return true;
        }

        static string SafeAscii(byte[] buf, int count)
        {
            try
            {
                return Encoding.ASCII.GetString(buf, 0, count);
            }
            catch
            {
                return BitConverter.ToString(buf, 0, count);
            }
        }
    }
}

