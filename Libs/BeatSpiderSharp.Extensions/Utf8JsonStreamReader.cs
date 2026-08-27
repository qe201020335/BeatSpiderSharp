using System.Buffers;
using System.IO.Pipelines;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace BeatSpiderSharp.Extensions;

/// <summary>
/// A forward-only, stateful JSON reader over a stream - the System.Text.Json counterpart to Newtonsoft's
/// <c>JsonTextReader</c>, which the BCL does not ship (dotnet/runtime#30328).
/// </summary>
/// <remarks>
/// <see cref="Utf8JsonReader"/> is a <c>ref struct</c>, so it cannot be held in a field or across an <c>await</c>
/// - nor across a <c>yield</c>, which is the same boundary to the compiler (<c>CS4007</c>), so a synchronous
/// version would not help. This keeps the buffered bytes and a <see cref="JsonReaderState"/> instead, and rebuilds
/// a short-lived reader per operation. A <see cref="PipeReader"/> owns the buffering; bytes stay UTF-8 throughout.
/// </remarks>
public sealed class Utf8JsonStreamReader : IAsyncDisposable
{
    /// <summary>Segment size. Large enough that values rarely straddle one; the 4 KiB default costs ~40%.</summary>
    private const int SegmentSize = 1024 * 1024;

    private readonly PipeReader _pipe;

    private ReadOnlySequence<byte> _buffer;
    private bool _reading;
    private bool _isFinalBlock;

    // The current token spans [_tokenStart, _tokenEnd) within _buffer. Both ends carry the reader state needed to
    // resume there, so the token can be re-read - to compare it, or to deserialize the value it opens.
    private long _tokenStart;
    private long _tokenEnd;
    private JsonReaderState _stateAtTokenStart;
    private JsonReaderState _stateAtTokenEnd;

    public Utf8JsonStreamReader(Stream stream, bool leaveOpen = true)
    {
        _pipe = PipeReader.Create(stream, new StreamPipeReaderOptions(
            bufferSize: SegmentSize, minimumReadSize: SegmentSize / 2, leaveOpen: leaveOpen));
    }

    /// <summary>The token the reader is currently positioned on.</summary>
    public JsonTokenType TokenType { get; private set; } = JsonTokenType.None;

    public async ValueTask DisposeAsync() => await _pipe.CompleteAsync();

    /// <summary>Advances to the next token. Returns <c>false</c> at the end of the stream.</summary>
    public async ValueTask<bool> ReadAsync(CancellationToken ct = default)
    {
        while (true)
        {
            var reader = new Utf8JsonReader(_buffer.Slice(_tokenEnd), _isFinalBlock, _stateAtTokenEnd);
            if (reader.Read())
            {
                _tokenStart = _tokenEnd;
                _stateAtTokenStart = _stateAtTokenEnd;
                _tokenEnd += reader.BytesConsumed;
                _stateAtTokenEnd = reader.CurrentState;
                TokenType = reader.TokenType;
                return true;
            }

            // Nothing was consumed, so the pipe can be released up to the failed read's start.
            if (!await RefillAsync(_tokenEnd, ct))
            {
                TokenType = JsonTokenType.None;
                return false;
            }
        }
    }

    /// <summary>Compares the current token's text to <paramref name="text"/> without allocating a string.</summary>
    public bool ValueTextEquals(string text)
    {
        var reader = new Utf8JsonReader(_buffer.Slice(_tokenStart), _isFinalBlock, _stateAtTokenStart);
        reader.Read();
        return reader.ValueTextEquals(text);
    }

    /// <summary>Skips the value the reader is positioned on, including everything nested inside it.</summary>
    public async ValueTask SkipAsync(CancellationToken ct = default)
    {
        while (true)
        {
            var reader = new Utf8JsonReader(_buffer.Slice(_tokenStart), _isFinalBlock, _stateAtTokenStart);
            reader.Read();
            if (reader.TrySkip())
            {
                _tokenEnd = _tokenStart + reader.BytesConsumed;
                _stateAtTokenEnd = reader.CurrentState;
                TokenType = reader.TokenType;
                return;
            }

            // The value runs past the buffered bytes; keep its first byte and pull more.
            if (!await RefillAsync(_tokenStart, ct))
            {
                throw new JsonException("Unexpected end of JSON while skipping a value");
            }
        }
    }

    /// <summary>Deserializes the current value, leaving the reader on that value's last token.</summary>
    public async ValueTask<T?> DeserializeAsync<T>(JsonTypeInfo<T> typeInfo, CancellationToken ct = default)
    {
        while (true)
        {
            var incomplete = false;
            T? value = default;
            long consumed = 0;
            JsonReaderState endState = default;
            JsonTokenType endToken = default;

            var reader = new Utf8JsonReader(_buffer.Slice(_tokenStart), _isFinalBlock, _stateAtTokenStart);
            try
            {
                // Step back onto the current token first. Handed a reader that has not read anything,
                // JsonSerializer treats the value as a whole document and rejects whatever follows it.
                reader.Read();
                value = JsonSerializer.Deserialize(ref reader, typeInfo);
                consumed = reader.BytesConsumed;
                endState = reader.CurrentState;
                endToken = reader.TokenType;
            }
            catch (JsonException) when (!_isFinalBlock)
            {
                // The value runs past the buffered bytes. Nothing has moved, so it can simply be retried.
                incomplete = true;
            }

            if (!incomplete)
            {
                _tokenEnd = _tokenStart + consumed;
                _stateAtTokenEnd = endState;
                TokenType = endToken;
                return value;
            }

            if (!await RefillAsync(_tokenStart, ct))
            {
                throw new JsonException("Unexpected end of JSON while reading a value");
            }
        }
    }

    /// <summary>
    /// Releases everything before <paramref name="keepFrom"/> and waits for more bytes.
    /// Returns <c>false</c> once the stream is exhausted.
    /// </summary>
    private async ValueTask<bool> RefillAsync(long keepFrom, CancellationToken ct)
    {
        if (_isFinalBlock) return false;

        if (_reading)
        {
            // Consumed marks what is finished with; examining the whole buffer tells the pipe the remainder is not
            // enough on its own, so the next read waits for more bytes rather than returning the same span.
            _pipe.AdvanceTo(_buffer.GetPosition(keepFrom), _buffer.End);
        }

        var result = await _pipe.ReadAsync(ct);
        _reading = true;
        _buffer = result.Buffer;
        _isFinalBlock = result.IsCompleted;

        // Offsets are relative to the buffer, which just lost everything before keepFrom.
        _tokenStart -= keepFrom;
        _tokenEnd -= keepFrom;
        return true;
    }
}
