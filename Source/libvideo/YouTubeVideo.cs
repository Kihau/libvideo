using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VideoLibrary.Extensions;

namespace VideoLibrary;

public partial class YouTubeVideo : Video
{
    public string JsPlayerUrl { get; }
    private string _jsPlayer;
    private string _uri;
    private readonly Query _uriQuery;
    private bool _encrypted;
    private bool _needNDescramble;
        
    internal YouTubeVideo(VideoInfo info, UnscrambledQuery query, string jsPlayerUrl)
    {
        this.Info = info;
        this.Title = info?.Title;
        this._uri = query.Uri;
        this._uriQuery = new Query(_uri);
        this.JsPlayerUrl = jsPlayerUrl;
        this._encrypted = query.IsEncrypted;
        this._needNDescramble = _uriQuery.ContainsKey("n");
        this.FormatCode = int.Parse(_uriQuery["itag"]);
    }

    public override string Title { get; }

    public override VideoInfo Info { get; }

    public override WebSites WebSite => WebSites.YouTube;

    public override string Uri => GetUriAsync().GetAwaiter().GetResult();
    
    public async Task<string> GetUriAsync()
    {
        var handler = new HttpClientHandler();
        if (handler.SupportsAutomaticDecompression)
        {
            handler.AutomaticDecompression =
                DecompressionMethods.GZip |
                DecompressionMethods.Deflate;
        }

        var client = new HttpClient(handler);
        
        if (_encrypted)
        {
            _uri = await DecryptAsync(_uri, client).ConfigureAwait(false);
            _encrypted = false;
        }

        if (!_needNDescramble) 
            return _uri;
            
        _uri = await NDescrambleAsync(_uri, client).ConfigureAwait(false);
        _needNDescramble = false;

        return _uri;
    }

    public int FormatCode { get; }
    public bool IsEncrypted => _encrypted;

    private long? ContentLength => this.GetContentLength(_uriQuery).Result;
    private async Task<long?> GetContentLength(Query query)
    {
        if (query.TryGetValue("clen", out string clen))
            return long.Parse(clen);
            
        using (var client = new VideoClient())
            return await client.GetContentLengthAsync(_uri);
    }

}