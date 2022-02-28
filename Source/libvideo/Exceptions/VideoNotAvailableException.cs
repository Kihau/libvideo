using System;

namespace VideoLibrary.Exceptions;

public class VideoNotAvailableException : Exception
{
    public VideoNotAvailableException()
    { }

    public VideoNotAvailableException(string message)
        : base(message)
    { }
}