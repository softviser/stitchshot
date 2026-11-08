using SkiaSharp;

namespace StitchShot.Core;

/// <summary>Collects frames and stacks them vertically. Owns the frames it is given.</summary>
public sealed class StitchComposer : IDisposable
{
    readonly List<SKBitmap> _frames = new();
    readonly List<int> _skipTop = new(); // rows dropped from the top of each frame: header crop + detected overlap

    public bool TrimOverlap { get; init; } = true;
    public int CropTop { get; init; }
    // shortcut: vertical, left-aligned, white gaps; add direction/alignment settings when someone needs them.
    public SKColor Background { get; init; } = SKColors.White;

    public int Count => _frames.Count;
    public int Width => _frames.Count == 0 ? 0 : _frames.Max(f => f.Width);
    public int Height => _frames.Select((f, i) => f.Height - _skipTop[i]).Sum();

    public void Add(SKBitmap frame)
    {
        int skip = 0;
        if (_frames.Count > 0)
        {
            skip = Math.Min(CropTop, frame.Height);
            if (TrimOverlap) skip += OverlapDetector.Find(_frames[^1], frame, skip);
        }
        _frames.Add(frame);
        _skipTop.Add(skip);
    }

    public bool Undo()
    {
        if (_frames.Count == 0) return false;
        _frames[^1].Dispose();
        _frames.RemoveAt(_frames.Count - 1);
        _skipTop.RemoveAt(_skipTop.Count - 1);
        return true;
    }

    public SKBitmap Compose()
    {
        var result = new SKBitmap(Math.Max(1, Width), Math.Max(1, Height), SKColorType.Bgra8888, SKAlphaType.Opaque);
        using var canvas = new SKCanvas(result);
        canvas.Clear(Background);
        int y = 0;
        for (int i = 0; i < _frames.Count; i++)
        {
            var f = _frames[i];
            int h = f.Height - _skipTop[i];
            if (h <= 0) continue;
            canvas.DrawBitmap(f, new SKRect(0, _skipTop[i], f.Width, f.Height), new SKRect(0, y, f.Width, y + h),
                new SKSamplingOptions(SKFilterMode.Nearest));
            y += h;
        }
        return result;
    }

    public void Dispose()
    {
        foreach (var f in _frames) f.Dispose();
        _frames.Clear();
        _skipTop.Clear();
    }
}
