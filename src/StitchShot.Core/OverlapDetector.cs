using System.Runtime.InteropServices;
using SkiaSharp;

namespace StitchShot.Core;

/// <summary>
/// Finds how many rows at the top of the new frame repeat the bottom of the previous one,
/// which is what Page Down leaves behind in browsers and document viewers.
/// </summary>
public static class OverlapDetector
{
    /// <param name="nextOffset">Rows at the top of <paramref name="next"/> already discarded (sticky-header crop).</param>
    /// <param name="minRows">Smaller matches are treated as coincidence.</param>
    /// <returns>Overlapping row count, 0 when none.</returns>
    public static int Find(SKBitmap prev, SKBitmap next, int nextOffset = 0, int minRows = 8)
    {
        if (prev.Width != next.Width) return 0;
        var a = RowHashes(prev);
        var b = RowHashes(next);
        int max = Math.Min(prev.Height, next.Height - nextOffset);
        for (int k = max; k >= minRows; k--)
        {
            int start = prev.Height - k;
            bool match = true;
            for (int i = 0; i < k && match; i++) match = a[start + i] == b[nextOffset + i];
            if (match && !IsFlat(a, start, k)) return k;
        }
        return 0;
    }

    // A block of identical rows (blank space) matches anywhere, so it proves nothing.
    static bool IsFlat(ulong[] hashes, int start, int count)
    {
        for (int i = 1; i < count; i++) if (hashes[start + i] != hashes[start]) return false;
        return true;
    }

    static ulong[] RowHashes(SKBitmap bitmap)
    {
        var bytes = bitmap.GetPixelSpan();
        var hashes = new ulong[bitmap.Height];
        int rowBytes = bitmap.RowBytes, width = bitmap.Width * bitmap.BytesPerPixel;
        for (int y = 0; y < bitmap.Height; y++)
        {
            ulong h = 14695981039346656037; // FNV-1a over the row's pixel bytes
            foreach (var v in MemoryMarshal.Cast<byte, uint>(bytes.Slice(y * rowBytes, width)))
                h = (h ^ v) * 1099511628211;
            hashes[y] = h;
        }
        return hashes;
    }
}
