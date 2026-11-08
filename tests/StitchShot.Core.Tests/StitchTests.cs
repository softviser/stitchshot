using SkiaSharp;
using StitchShot.Core;

namespace StitchShot.Core.Tests;

public class StitchTests
{
    /// <summary>Each row gets a distinct color derived from a seed so rows can be told apart.</summary>
    static SKBitmap Rows(int width, int height, int firstRowId)
    {
        var b = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque);
        for (int y = 0; y < height; y++)
        {
            var c = new SKColor((byte)(firstRowId + y), (byte)((firstRowId + y) * 7), (byte)((firstRowId + y) * 13));
            for (int x = 0; x < width; x++) b.SetPixel(x, y, c);
        }
        return b;
    }

    [Fact]
    public void Finds_page_down_overlap()
    {
        using var prev = Rows(40, 50, 0);     // rows 0..49
        using var next = Rows(40, 50, 30);    // rows 30..79, so 20 rows repeat
        Assert.Equal(20, OverlapDetector.Find(prev, next));
    }

    [Fact]
    public void No_overlap_and_flat_areas_return_zero()
    {
        using var prev = Rows(40, 50, 0);
        using var next = Rows(40, 50, 100);
        Assert.Equal(0, OverlapDetector.Find(prev, next));

        using var blank1 = Rows(40, 50, 0); using var blank2 = Rows(40, 50, 0);
        using var c1 = new SKCanvas(blank1); c1.Clear(SKColors.White);
        using var c2 = new SKCanvas(blank2); c2.Clear(SKColors.White);
        Assert.Equal(0, OverlapDetector.Find(blank1, blank2));
    }

    [Fact]
    public void Overlap_after_header_crop_is_found()
    {
        using var prev = Rows(40, 50, 0);
        using var next = Rows(40, 50, 30);
        using var c = new SKCanvas(next); c.DrawRect(0, 0, 40, 5, new SKPaint { Color = SKColors.Red }); // sticky header
        Assert.Equal(15, OverlapDetector.Find(prev, next, nextOffset: 5)); // rows 35..49 of prev
    }

    [Fact]
    public void Composer_stacks_trims_and_undoes()
    {
        using var composer = new StitchComposer();
        composer.Add(Rows(40, 50, 0));
        composer.Add(Rows(40, 50, 30));
        Assert.Equal(80, composer.Height);
        composer.Add(Rows(20, 10, 200));          // narrower frame: no trim, widens nothing
        Assert.Equal(90, composer.Height);
        Assert.Equal(40, composer.Width);

        using (var image = composer.Compose())
        {
            Assert.Equal(40, image.Width);
            Assert.Equal(90, image.Height);
            Assert.Equal(Rows(1, 1, 79).GetPixel(0, 0), image.GetPixel(0, 79)); // last row of page 2 is row id 79
            Assert.Equal(SKColors.White, image.GetPixel(30, 85));               // gap beside the narrow frame
        }

        Assert.True(composer.Undo());
        Assert.Equal(2, composer.Count);
        Assert.Equal(80, composer.Height);
    }
}
