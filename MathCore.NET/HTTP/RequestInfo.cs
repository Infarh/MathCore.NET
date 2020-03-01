using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using MathCore.NET.HTTP.Html;
// ReSharper disable UnusedMember.Global
// ReSharper disable ConvertToAutoPropertyWhenPossible

namespace MathCore.NET.HTTP
{
    public class RequestInfo : EventArgs, IDisposable
    {
        private readonly WebServer _Server;
        public HttpListenerContext Context { get; }
        public Match RouteRegexMatch { get; }

        public WebServer Server => _Server;
        public BinaryReader BinaryReader => new BinaryReader(Context.Request.InputStream);
        public StreamReader Reader => new StreamReader(Context.Request.InputStream);
        public BinaryWriter BinaryWriter => new BinaryWriter(Context.Response.OutputStream);
        public StreamWriter Writer => new StreamWriter(Context.Response.OutputStream) { AutoFlush = true };
        public Uri URI => Context.Request.Url;

        public string ContentType { get => Context.Response.ContentType; set => Context.Response.ContentType = value; }

        public bool LeaveOpen { get; set; }

        public RequestInfo(HttpListenerContext context, Match match, WebServer Server)
        {
            _Server = Server;
            Context = context;
            RouteRegexMatch = match;
        }

        public RequestInfo SetContentType(string type)
        {
            ContentType = type;
            return this;
        }

        public RequestInfo SetStatusCode(int StatusCode)
        {
            Context.Response.StatusCode = StatusCode;
            return this;
        }

        public RequestInfo SetStatusCode(HttpStatusCode StatusCode)
        {
            Context.Response.StatusCode = (int)StatusCode;
            return this;
        }

        public RequestInfo SendFile(string FileName, int buffer_length = 1048, IProgress<double> progress = null)
        {
            var response = Context.Response;
            var file = new FileInfo(FileName);

            if(!file.Exists)
                file = new FileInfo(Path.Combine(_Server.HomeDirectoryPath, FileName));

            if (!file.Exists)
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return this;
            }

            try
            {
                var response_stream = response.OutputStream;
                using (var file_stream = file.OpenRead())
                {
                    response.ContentLength64 = file_stream.Length;
                    var buffer = new byte[buffer_length];
                    int readed;
                    do
                    {
                        readed = file_stream.Read(buffer, 0, buffer_length);
                        response_stream.Write(buffer, 0, readed);
                        progress?.Report((double)file_stream.Position / file_stream.Length);
                    } while (readed == buffer_length);
                }
            }
            catch (Exception e)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Trace.TraceError(e.ToString());
                return this;
            }
            return this;
        }

        public RequestInfo SendFile(int buffer_length = 1048, IProgress<double> progress = null)
        {
            var file = new FileInfo(Path.Combine(_Server.HomeDirectoryPath, URI.LocalPath.TrimStart('/')));
            var response = Context.Response;
            if (!file.Exists)
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                return this;
            }

            try
            {
                var response_stream = response.OutputStream;
                using (var file_stream = file.OpenRead())
                {
                    response.ContentLength64 = file_stream.Length;
                    var buffer = new byte[buffer_length];
                    int readed;
                    do
                    {
                        readed = file_stream.Read(buffer, 0, buffer_length);
                        response_stream.Write(buffer, 0, readed);
                        progress?.Report((double)file_stream.Position / file_stream.Length);
                    } while (readed == buffer_length);
                }
            }
            catch (Exception e)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                Trace.TraceError(e.ToString());
                return this;
            }

            return this;
        }

        public RequestInfo Send(string text)
        {
            Writer.Write(text);
            return this;
        }

        public RequestInfo Send(Page page) => Send(page.ToString());

        /// <inheritdoc />
        public void Dispose()
        {
            if (!LeaveOpen) Context.Response.OutputStream.Dispose();
        }
    }
}