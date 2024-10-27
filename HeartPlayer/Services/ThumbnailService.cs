using System;
using System.Collections.Concurrent;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using HeartPlayer.Models;
using LibVLCSharp.Shared;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace HeartPlayer.Services;

public sealed class ThumbnailService
{
    private const uint Width = 320;
    private const uint Height = 240;

    /// <summary>
    /// RGBA is used, so 4 byte per pixel, or 32 bits.
    /// </summary>
    private const uint BytePerPixel = 4;

    /// <summary>
    /// the number of bytes per "line"
    /// For performance reasons inside the core of VLC, it must be aligned to multiples of 32.
    /// </summary>
    private static readonly uint Pitch;

    /// <summary>
    /// The number of lines in the buffer.
    /// For performance reasons inside the core of VLC, it must be aligned to multiples of 32.
    /// </summary>
    private static readonly uint Lines;

    static ThumbnailService()
    {
        Pitch = Align(Width * BytePerPixel);
        Lines = Align(Height);

        uint Align(uint size)
        {
            if (size % 32 == 0)
            {
                return size;
            }

            return ((size / 32) + 1) * 32;// Align on the next multiple of 32
        }
    }

    public ThumbnailService()
    {
        // Load native libvlc library
        Core.Initialize();
    }

    private readonly ConcurrentDictionary<string, ImageSource> _thumbnailCache = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private static MemoryMappedFile CurrentMappedFile;
    private static MemoryMappedViewAccessor CurrentMappedViewAccessor;
    private static readonly ConcurrentQueue<(MemoryMappedFile file, MemoryMappedViewAccessor accessor)> FilesToProcess = new ConcurrentQueue<(MemoryMappedFile file, MemoryMappedViewAccessor accessor)>();

    public async Task<ImageSource> GetThumbnailAsync(VideoFile video)
    {
        if (_thumbnailCache.TryGetValue(video.Path, out var cachedThumbnail))
        {
            return cachedThumbnail;
        }

        await _semaphore.WaitAsync();
        try
        {
            if (_thumbnailCache.TryGetValue(video.Path, out cachedThumbnail))
            {
                return cachedThumbnail;
            }

            var thumbnail = await GenerateThumbnailAsync(video);
            _thumbnailCache[video.Path] = thumbnail;
            return thumbnail;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<ImageSource> GenerateThumbnailAsync(VideoFile video)
    {
        try
        {
            var outputPath = Path.Combine(FileSystem.CacheDirectory, $"{Path.GetFileNameWithoutExtension(video.Name)}_thumb.jpg");

            if (File.Exists(outputPath))
            {
                Console.WriteLine($"Thumbnail already exists: {outputPath}");
                return ImageSource.FromFile(outputPath);
            }

            Console.WriteLine($"Generating thumbnail for: {video.Name}");

            using (var libvlc = new LibVLC())
            using (var mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libvlc))
            {
                // Create new media
                using var media = new Media(libvlc, video.Path, FromType.FromPath);

                media.AddOption(":no-audio");

                // Set the size and format of the video here.
                mediaPlayer.SetVideoFormat("RV32", Width, Height, Pitch);
                mediaPlayer.SetVideoCallbacks(Lock, null, Display);

                // Start recording
                mediaPlayer.Play(media);

                await Task.Delay(2000); // Wait for 1 second to ensure the video has started

                if (FilesToProcess.TryDequeue(out var file))
                {
                    using (var image = new Image<SixLabors.ImageSharp.PixelFormats.Bgra32>(
                                          (int)(Pitch / BytePerPixel),
                                          (int)Lines))
                    using (var sourceStream = file.file.CreateViewStream())
                    {
                        var mg = image.GetPixelMemoryGroup();
                        for (int i = 0; i < mg.Count; i++)
                        {
                            sourceStream.Read(MemoryMarshal.AsBytes(mg[i].Span));
                        }

                        using (var outputFile = File.Open(outputPath, FileMode.Create))
                        {
                            image.Mutate(ctx => ctx.Crop((int)Width, (int)Height));
                            image.SaveAsJpeg(outputFile);
                        }
                    }
                    file.accessor.Dispose();
                    file.file.Dispose();
                }

                mediaPlayer.Stop();

                ImageSource thumbnail;

                if (File.Exists(outputPath))
                {
                    Console.WriteLine($"Thumbnail generated: {outputPath}");
                    thumbnail = ImageSource.FromFile(outputPath);
                }
                else
                {
                    Console.WriteLine($"Thumbnail file not found: {outputPath}");
                    thumbnail = await GetDefaultThumbnailAsync();
                }

                _thumbnailCache[video.Path] = thumbnail;
                return thumbnail;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error generating thumbnail for {video.Name}");
            return await GetDefaultThumbnailAsync();
        }
    }

    private static IntPtr Lock(IntPtr opaque, IntPtr planes)
    {
        CurrentMappedFile = MemoryMappedFile.CreateNew(null, Pitch * Lines);
        CurrentMappedViewAccessor = CurrentMappedFile.CreateViewAccessor();
        Marshal.WriteIntPtr(planes, CurrentMappedViewAccessor.SafeMemoryMappedViewHandle.DangerousGetHandle());
        return IntPtr.Zero;
    }

    private static void Display(IntPtr opaque, IntPtr picture)
    {
        if (FilesToProcess.Count > 1)
        {
            FilesToProcess.TryDequeue(out var file);
        }

        FilesToProcess.Enqueue((CurrentMappedFile, CurrentMappedViewAccessor));
        CurrentMappedFile = null;
        CurrentMappedViewAccessor = null;
    }

    private Task<ImageSource> GetDefaultThumbnailAsync()
    {
        return Task.FromResult(ImageSource.FromFile("dotnet_bot.png"));
    }

    public async Task<ImageSource> GetFolderThumbnailAsync(Folder folder)
    {
        try
        {
            var outputPath = Path.Combine(FileSystem.CacheDirectory, $"{folder.Name}_thumb.jpg");

            if (File.Exists(outputPath))
            {
                return ImageSource.FromFile(outputPath);
            }

            var videoFiles = folder.Videos.Take(4).ToList();
            var thumbnails = new List<Image<SixLabors.ImageSharp.PixelFormats.Rgba32>>();

            foreach (var video in videoFiles)
            {
                var thumbnail = await GetThumbnailAsync(video);
                if (thumbnail is FileImageSource fileImageSource)
                {
                    var image = Image.Load<SixLabors.ImageSharp.PixelFormats.Rgba32>(fileImageSource.File);
                    thumbnails.Add(image);
                }
            }

            while (thumbnails.Count < 4)
            {
                thumbnails.Add(new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>((int)Width, (int)Height, SixLabors.ImageSharp.Color.Black));
            }

            using (var outputImage = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>((int)Width * 2, (int)Height * 2))
            {
                outputImage.Mutate(ctx =>
                {
                    ctx.DrawImage(thumbnails[0], new SixLabors.ImageSharp.Point(0, 0), 1f);
                    ctx.DrawImage(thumbnails[1], new SixLabors.ImageSharp.Point((int)Width, 0), 1f);
                    ctx.DrawImage(thumbnails[2], new SixLabors.ImageSharp.Point(0, (int)Height), 1f);
                    ctx.DrawImage(thumbnails[3], new SixLabors.ImageSharp.Point((int)Width, (int)Height), 1f);
                });

                using (var outputFile = File.Open(outputPath, FileMode.Create))
                {
                    outputImage.SaveAsJpeg(outputFile);
                }
            }

            // Dispose of the thumbnail images after we're done using them
            foreach (var thumbnail in thumbnails)
            {
                thumbnail.Dispose();
            }

            return ImageSource.FromFile(outputPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error generating thumbnail for {folder.Name}.");
            return await GetDefaultThumbnailAsync();
        }
    }
}
