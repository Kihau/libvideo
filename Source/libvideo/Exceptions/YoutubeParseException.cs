using System;

namespace VideoLibrary.Exceptions;

public class YoutubeParseException : Exception
{
    public YoutubeParseException(string message, Exception innerException)
        : base(message, innerException)
    { }
}