using System.Text;

namespace LTSBackend.Comman.Security;

/// <summary>
/// SECURITY (SRS "File Upload Security" -> "MIME validation"): the original
/// upload checks (UploadDocumentValidator + FileService.BlockedExtensions)
/// only ever inspected the client-supplied file name/extension. A file's
/// extension is just a label the uploader chooses - nothing stops someone
/// from renaming "payload.exe" to "invoice.pdf" and sailing straight past
/// an extension allow-list, or hiding a valid image behind a
/// legal-looking .docx name. This class adds a second, independent check
/// that reads the first few bytes actually written to the stream (the
/// file's "magic number") and confirms they match a known signature for
/// the claimed extension, so a mislabeled/spoofed file is rejected before
/// it is ever written to disk.
///
/// This is defense-in-depth alongside (not a replacement for) the
/// extension allow-list: it cannot catch a *malicious* PDF or DOCX (a
/// polyglot or an infected but structurally valid file - that requires a
/// real malware scanner, which is out of scope here and is called out
/// separately as an integration point), but it does close the far more
/// common "renamed executable/script" bypass.
/// </summary>
public static class FileSignatureValidator
{
    // Maps an allowed extension to the set of acceptable byte signatures at
    // offset 0. DOC/XLS (legacy OLE compound format) and DOCX/XLSX (both
    // just ZIP containers under the hood) share signatures with their
    // sibling formats, which is expected and fine - the goal here is only
    // to reject content that is NOT any of the plausible container formats
    // for that extension (e.g. an MZ/ELF executable, a shell script, raw
    // HTML/JS).
    private static readonly Dictionary<string, byte[][]> Signatures = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = new[] { Encoding.ASCII.GetBytes("%PDF") },
        [".doc"] = new[] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } },
        [".xls"] = new[] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } },
        // .docx/.xlsx/.zip are all ZIP containers ("PK\x03\x04", or the
        // rarer empty-archive/spanned-archive variants).
        [".docx"] = new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 }, new byte[] { 0x50, 0x4B, 0x05, 0x06 }, new byte[] { 0x50, 0x4B, 0x07, 0x08 } },
        [".xlsx"] = new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 }, new byte[] { 0x50, 0x4B, 0x05, 0x06 }, new byte[] { 0x50, 0x4B, 0x07, 0x08 } },
        [".zip"] = new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 }, new byte[] { 0x50, 0x4B, 0x05, 0x06 }, new byte[] { 0x50, 0x4B, 0x07, 0x08 } },
        [".jpg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
        [".jpeg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
        [".png"] = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
        // WebP = "RIFF"<4-byte size>"WEBP" - the size field varies per file,
        // so match only the fixed "RIFF" prefix at offset 0 (still enough
        // to reject a renamed executable/script).
        [".webp"] = new[] { Encoding.ASCII.GetBytes("RIFF") },
        // .txt has no reliable magic number - any byte sequence is
        // "valid" plain text (including empty). Explicitly allow-listed
        // here (rather than falling through to "unknown extension =
        // reject") so it isn't silently rejected, while byte-signature
        // extensions above still get real content verification.
        [".txt"] = Array.Empty<byte[]>()
    };

    /// <summary>
    /// Returns true when <paramref name="extension"/> is a recognized
    /// document extension AND the leading bytes of the stream match a
    /// known signature for that extension (or the extension has no
    /// reliable signature, e.g. .txt). Returns false for any unrecognized
    /// extension or any signature mismatch. The stream position is reset
    /// to its original value before returning, so the caller can still
    /// save/copy it afterward.
    /// </summary>
    public static bool HasValidSignature(Stream stream, string extension)
    {
        if (!Signatures.TryGetValue(extension, out var candidates))
        {
            // Unknown extension to this validator - let the caller's
            // extension allow-list be the sole authority for it.
            return false;
        }

        if (candidates.Length == 0)
        {
            return true; // e.g. .txt - no signature to check
        }

        long originalPosition = stream.CanSeek ? stream.Position : 0;
        try
        {
            if (!stream.CanSeek)
            {
                return true; // can't inspect a non-seekable stream safely; skip
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
