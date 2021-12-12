using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MathCore.NET.HTTP
{
    public abstract class Message : IEnumerable<(string Header, string Value)>
    {
        //private byte[] _Content;

        protected readonly List<(string Header, string Value)> _Headers = new();

        public string UserAgent => GetHeader("User-Agent");

        public string Cookie => GetHeader();
        public string Version { get; set; }

        public IEnumerable<string> this[string HeaderName] => GetHeaders(HeaderName);

        public virtual void Load(Stream DataStream) => Load(new StreamReader(DataStream ?? throw new ArgumentNullException(nameof(DataStream))));

        public virtual async Task LoadAsync(Stream DataStream, CancellationToken Cancel = default) => await LoadAsync(new StreamReader(DataStream ?? throw new ArgumentNullException(nameof(DataStream))), Cancel).ConfigureAwait(false);

        public virtual void Load(StreamReader Reader)
        {
            if (Reader is null) throw new ArgumentNullException(nameof(Reader));
        }

        public virtual Task LoadAsync(StreamReader Reader, CancellationToken Cancel = default) => 
            Reader is null ? throw new ArgumentNullException(nameof(Reader)) : Task.CompletedTask;

        public string GetHeader([CallerMemberName] string HeaderName = null)
        {
            foreach (var (key, value) in _Headers)
                if (key.Equals(HeaderName, StringComparison.OrdinalIgnoreCase))
                    return value;
            return null;
        }

        public IEnumerable<string> GetHeaders([CallerMemberName] string HeaderName = null)
        {
            foreach (var (key, value) in _Headers)
                if (key.Equals(HeaderName, StringComparison.OrdinalIgnoreCase))
                    yield return value;
        }

        protected void LoadHeaders(StreamReader Reader)
        {
            _Headers.Clear();
            string line;
            while (!Reader.EndOfStream && !string.IsNullOrWhiteSpace(line = Reader.ReadLine()))
                _Headers.Add(Parse(line));
            _Headers.TrimExcess();
        }

        protected async Task LoadHeadersAsync(StreamReader Reader, CancellationToken Cancel = default)
        {
            Cancel.ThrowIfCancellationRequested();
            _Headers.Clear();
            string line;
            while (!Reader.EndOfStream && !string.IsNullOrWhiteSpace(line = await Reader.ReadLineAsync().WithCancellation(Cancel).ConfigureAwait(false)))
            {
                Cancel.ThrowIfCancellationRequested();
                _Headers.Add(Parse(line));
            }
            _Headers.TrimExcess();
        }

        protected static (string Header, string Value) Parse(string line)
        {
            var separator_index = line.IndexOf(':');
            if (separator_index <= 0) return ("unknown", line);
            var header = line.Substring(0, separator_index);
            var value = line.Substring(separator_index + 2);
            return (header, value);
        }

        protected void LoadContent(Stream DataStream)
        {
            //_Content = null;
            if (DataStream.Position == DataStream.Length) return;
            var content = new byte[DataStream.Length - DataStream.Position];
            DataStream.Read(content, 0, content.Length);
        }

        protected async Task LoadContentAsync(Stream DataStream, CancellationToken Cancel = default)
        {
            //_Content = null;
            if (DataStream.Position == DataStream.Length) return;
            var content = new byte[DataStream.Length - DataStream.Position];
            await DataStream.ReadAsync(content, 0, content.Length, Cancel);
        }

        public IEnumerator<(string Header, string Value)> GetEnumerator() => _Headers.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_Headers).GetEnumerator();
    }
}
