using System.Text;

namespace LTSBackend.Comman.Security;





















public static class FileSignatureValidator
{







    private static readonly Dictionary<string, byte[][]> Signatures = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = new[] { Encoding.ASCII.GetBytes("%PDF") },
        [".doc"] = new[] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } },
        [".xls"] = new[] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } },


        [".docx"] = new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 }, new byte[] { 0x50, 0x4B, 0x05, 0x06 }, new byte[] { 0x50, 0x4B, 0x07, 0x08 } },
        [".xlsx"] = new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 }, new byte[] { 0x50, 0x4B, 0x05, 0x06 }, new byte[] { 0x50, 0x4B, 0x07, 0x08 } },
        [".zip"] = new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 }, new byte[] { 0x50, 0x4B, 0x05, 0x06 }, new byte[] { 0x50, 0x4B, 0x07, 0x08 } },
        [".jpg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
        [".jpeg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
        [".png"] = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },



        [".webp"] = new[] { Encoding.ASCII.GetBytes("RIFF") },





        [".txt"] = Array.Empty<byte[]>()
    };










    public static bool HasValidSignature(Stream stream, string extension)
    {
        if (!Signatures.TryGetValue(extension, out var candidates))
        {


            return false;
        }

        if (candidates.Length == 0)
        {
            return true;
        }

        long originalPosition = stream.CanSeek ? stream.Position : 0;
        try
        {
            if (!stream.CanSeek)
            {
                return true;
            }

            stream.Position = 0;
            int maxLen = candidates.Max(c => c.Length);
            var header = new byte[maxLen];
            int read = stream.Read(header, 0, maxLen);

            foreach (var candidate in candidates)
            {
                if (read >= candidate.Length && header.Take(candidate.Length).SequenceEqual(candidate))
                {
                    return true;
                }
            }

            return false;
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }
    }
}
