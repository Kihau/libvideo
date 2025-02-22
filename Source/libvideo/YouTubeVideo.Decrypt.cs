using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoLibrary.Extensions;

namespace VideoLibrary;

public partial class YouTubeVideo
{
    private async Task<string> DecryptAsync(string uri, HttpClient client)
    {
        var query = new Query(uri);

        string signature;
        if (!query.TryGetValue(YouTube.GetSignatureKey(), out signature))
            return uri;

        if (string.IsNullOrWhiteSpace(signature))
            throw new Exception("Signature not found.");

        if (_jsPlayer == null)
        {
            _jsPlayer = await client
                .GetStringAsync(JsPlayerUrl)
                .ConfigureAwait(false);
        }

        query[YouTube.GetSignatureKey()] = DecryptSignature(_jsPlayer, signature);
        return query.ToString();
    }
        
    private string DecryptSignature(string js, string signature)
    {
        var functionNameRegex = new Regex(@"\w+(?:.|\[)(\""?\w+(?:\"")?)\]?\(");
        var functionLines = GetDecryptionFunctionLines(js);
        var decryptor = new Decryptor();
        var decipherDefinitionName = Regex.Match(string.Join(";", functionLines), "([\\$_\\w]+).\\w+\\(\\w+,\\d+\\);").Groups[1].Value;
        if (string.IsNullOrEmpty(decipherDefinitionName))
        {
            throw new Exception("Could not find signature decipher definition name. Please report this issue to us.");
        }

        var decipherDefinitionBody = Regex.Match(js, $@"var\s+{Regex.Escape(decipherDefinitionName)}=\{{(\w+:function\(\w+(,\w+)?\)\{{(.*?)\}}),?\}};", RegexOptions.Singleline).Groups[0].Value;
        if (string.IsNullOrEmpty(decipherDefinitionBody))
            throw new Exception("Could not find signature decipher definition body. Please report this issue to us.");
            
        foreach (var functionLine in functionLines)
        {
            if (decryptor.IsComplete)
                break;

            var match = functionNameRegex.Match(functionLine);
                
            if (match.Success)
                decryptor.AddFunction(decipherDefinitionBody, match.Groups[1].Value);
        }

        foreach (var functionLine in functionLines)
        {
            var match = functionNameRegex.Match(functionLine);
                
            if (match.Success)
                signature = decryptor.ExecuteFunction(signature, functionLine, match.Groups[1].Value);
        }

        return signature;
    }
        
    private static string[] GetDecryptionFunctionLines(string js)
    {
        var decipherFuncName = Regex.Match(js, @"(\w+)=function\(\w+\){(\w+)=\2\.split\(\x22{2}\);.*?return\s+\2\.join\(\x22{2}\)}");
        return decipherFuncName.Success ? decipherFuncName.Groups[0].Value.Split(';') : null;
    }
        
    private class Decryptor
    {
        private static readonly Regex ParametersRegex = new Regex(@"\(\w+,(\d+)\)");

        private readonly Dictionary<string, FunctionType> _functionTypes = new();
        private readonly StringBuilder _stringBuilder = new();

        public bool IsComplete =>
            _functionTypes.Count == Enum.GetValues(typeof(FunctionType)).Length;

        public void AddFunction(string js, string function)
        {
            var escapedFunction = Regex.Escape(function);
            FunctionType? type = null;
            /* Pass  "do":function(a){} or xa:function(a,b){} */
            if (Regex.IsMatch(js, $@"(\"")?{escapedFunction}(\"")?:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\."))
                type = FunctionType.Slice;
            else if (Regex.IsMatch(js, $@"(\"")?{escapedFunction}(\"")?:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b"))
                type = FunctionType.Swap;
                
            if (Regex.IsMatch(js, $@"(\"")?{escapedFunction}(\"")?:\bfunction\b\(\w+\){{\w+\.reverse"))
                type = FunctionType.Reverse;
                
            if (type.HasValue)
                _functionTypes[function] = type.Value;
        }

        public string ExecuteFunction(string signature, string line, string function)
        {
            if (!_functionTypes.TryGetValue(function, out var type))
                return signature;

            switch (type)
            {
                case FunctionType.Reverse:
                    return Reverse(signature);
                case FunctionType.Slice:
                case FunctionType.Swap:
                    var index =
                        int.Parse(
                            ParametersRegex.Match(line).Groups[1].Value,
                            NumberStyles.AllowThousands,
                            NumberFormatInfo.InvariantInfo);
                    return
                        type == FunctionType.Slice
                            ? Slice(signature, index)
                            : Swap(signature, index);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        private string Reverse(string signature)
        {
            _stringBuilder.Clear();
            for (var index = signature.Length - 1; index >= 0; index--)
                _stringBuilder.Append(signature[index]);

            return _stringBuilder.ToString();
        }

        private static string Slice(string signature, int index) =>
            signature[index..];

        private string Swap(string signature, int index)
        {
            _stringBuilder.Clear();
            _stringBuilder.Append(signature);
            _stringBuilder[0] = _stringBuilder[index % _stringBuilder.Length];
            _stringBuilder[index % _stringBuilder.Length] = signature[0];
            return _stringBuilder.ToString();
        }

        private enum FunctionType
        {
            Reverse,
            Slice,
            Swap
        }
    }
}