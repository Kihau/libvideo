using System;

namespace VideoLibrary.Exceptions;

public class AudioExtractionException : Exception
{
    public AudioExtractionException(string message)
        : base(message)
    { }
}