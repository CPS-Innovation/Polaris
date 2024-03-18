using System;
using System.IO;
using System.Threading.Tasks;

namespace Common.Streaming;

public static class StreamExtensions
{
  public static async Task<Stream> EnsureSeekableAsync(this Task<Stream> task)
  {
    if (task == null)
    {
      throw new ArgumentNullException(nameof(task));
    }
    var stream = await task;

    return await stream.EnsureSeekableAsync();
  }

  public static async Task<Stream> EnsureSeekableAsync(this Stream stream)
  {
    if (stream == null)
    {
      throw new ArgumentNullException(nameof(stream));
    }

    if (stream.CanSeek)
    {
      return stream;
    }

    var seekableStream = new MemoryStream();
    await stream.CopyToAsync(seekableStream);
    await stream.DisposeAsync();

    seekableStream.Position = 0;
    return seekableStream;
  }
  public static async Task<byte[]> EnsureSeekableAndConvertToByteArrayAsync(this Task<Stream> task)
  {
    if (task == null)
    {
      throw new ArgumentNullException(nameof(task));
    }
    var stream = await task;

    return await stream.EnsureSeekableAndConvertToByteArrayAsync();
  }

  public static async Task<byte[]> EnsureSeekableAndConvertToByteArrayAsync(this Stream stream)
  {
    if (stream == null)
    {
      throw new ArgumentNullException(nameof(stream));
    }

    if (!stream.CanSeek)
    {
      var seekableStream = new MemoryStream();
      await stream.CopyToAsync(seekableStream);
      await stream.DisposeAsync();

      seekableStream.Position = 0;
      stream = seekableStream;
    }

    using var memoryStream = new MemoryStream();
    await stream.CopyToAsync(memoryStream);
    return memoryStream.ToArray();
  }
}