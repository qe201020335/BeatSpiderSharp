namespace BeatSpiderSharp.Tests.TestHelpers;

/// <summary>
/// Returns at most <paramref name="chunkSize"/> bytes per read, so a JSON value's bytes arrive split at every
/// possible offset. Reading a stream a byte at a time is what exercises the reader's resume paths.
/// </summary>
public sealed class DripStream(byte[] data, int chunkSize) : Stream
{
    private int _pos;

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => data.Length;

    public override long Position
    {
        get => _pos;
        set => throw new NotSupportedException();
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count)
    {
        var n = Math.Min(Math.Min(chunkSize, count), data.Length - _pos);
        Array.Copy(data, _pos, buffer, offset, n);
        _pos += n;
        return n;
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var n = Math.Min(Math.Min(chunkSize, buffer.Length), data.Length - _pos);
        data.AsSpan(_pos, n).CopyTo(buffer.Span);
        _pos += n;
        return ValueTask.FromResult(n);
    }
}
