using NiL.JS.BaseLibrary;
using NiL.JS.Core;
using NiL.JS.Extensions;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoLibrary.Extensions;

namespace VideoLibrary;

public partial class YouTubeVideo
{
	private async Task<string> NDescrambleAsync(string uri, HttpClient client)
	{
		var query = new Query(uri);

		if (!query.TryGetValue("n", out var signature))
			return uri;

		if (string.IsNullOrWhiteSpace(signature))
			throw new Exception("N Signature not found.");

		if (_jsPlayer == null)
		{
			_jsPlayer = await client
				.GetStringAsync(JsPlayerUrl)
				.ConfigureAwait(false);
		}

		query["n"] = DescrambleNSignature(_jsPlayer, signature);
		return query.ToString();
	}

	private string DescrambleNSignature(string js, string signature)
	{
		var descrambleFunction = GetDescrambleFunctionLines(js);
			
		if (string.IsNullOrWhiteSpace(descrambleFunction)) 
			return signature;
			
		var context = new Context();
		context.Eval("var " + descrambleFunction);
		return context
			.GetVariable(descrambleFunction.Substring(0,
				descrambleFunction.IndexOf("=", StringComparison.Ordinal))).As<Function>()
			.Call(new Arguments {signature}).Value.ToString();

	}

	private string GetDescrambleFunctionLines(string js)
	{
		var functionRegexMatchStart = Regex.Match(js, @"\w+=function\((\w)\){var\s+\w=\1.split\(\x22{2}\),\w=");
		var functionRegexMatchEnd = Regex.Match(js, @"\+a}return\s\w.join\(\x22{2}\)};");

		if (!functionRegexMatchStart.Success || !functionRegexMatchEnd.Success) 
			return null;
			
		var block = js.Substring(functionRegexMatchStart.Index,
			functionRegexMatchEnd.Index + functionRegexMatchEnd.Length - functionRegexMatchStart.Index);
			
		return block.Contains("enhanced_except") ? block : null;
	}
}