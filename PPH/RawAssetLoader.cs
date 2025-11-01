using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace PPH
{
    // Утилита для прямой загрузки PNG/PCM из папки Data без XNB
    public static class RawAssetLoader
    {
        public static async Task<Texture2D> LoadTextureAsync(GraphicsDevice device, string relativePath)
        {
            var uri = new Uri("ms-appx:///" + Normalize(relativePath));
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            using (IRandomAccessStream ras = await file.OpenReadAsync())
            using (var stream = ras.AsStreamForRead())
            {
                return Texture2D.FromStream(device, stream);
            }
        }

        // Загрузка 8-битного моно PCM и оборачивание в WAV для SoundEffect
        public static async Task<SoundEffect> LoadPcm8Async(string relativePath, int sampleRate = 22050)
        {
            var uri = new Uri("ms-appx:///" + Normalize(relativePath));
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            using (IRandomAccessStream ras = await file.OpenReadAsync())
            {
                var reader = new DataReader(ras);
                await reader.LoadAsync((uint)ras.Size);
                byte[] pcm = new byte[ras.Size];
                reader.ReadBytes(pcm);

                var wavStream = BuildWavFromPcm8Mono(pcm, sampleRate);
                wavStream.Position = 0;
                return SoundEffect.FromStream(wavStream);
            }
        }

        private static MemoryStream BuildWavFromPcm8Mono(byte[] pcm8, int sampleRate)
        {
            int bitsPerSample = 8;
            short channels = 1;
            short audioFormat = 1; // PCM
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            short blockAlign = (short)(channels * bitsPerSample / 8);
            int dataSize = pcm8.Length;
            int fmtChunkSize = 16;
            int riffChunkSize = 4 + (8 + fmtChunkSize) + (8 + dataSize);

            var ms = new MemoryStream(44 + dataSize);
            using (var bw = new BinaryWriter(ms, System.Text.Encoding.ASCII, leaveOpen: true))
            {
                // RIFF header
                bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                bw.Write(riffChunkSize);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

                // fmt chunk
                bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                bw.Write(fmtChunkSize);
                bw.Write((short)audioFormat);
                bw.Write(channels);
                bw.Write(sampleRate);
                bw.Write(byteRate);
                bw.Write(blockAlign);
                bw.Write((short)bitsPerSample);

                // data chunk
                bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                bw.Write(dataSize);
                bw.Write(pcm8);
            }
            return ms;
        }

        private static string Normalize(string p) => p.Replace("\\", "/");
    }
}
