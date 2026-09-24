using System;
using System.Collections.Generic;

namespace GodotSyxPort.Data;

/// <summary>Parser for the source game's relaxed data syntax: bare keys and
/// values, trailing commas, root objects without braces and ** line comments.</summary>
public sealed class SyxDataParser
{
    private readonly string _source;
    private int _position;

    private SyxDataParser(string source) => _source = source;

    public static SyxDataNode Parse(string source) => new SyxDataParser(source).ParseObject(false);

    private SyxDataNode ParseObject(bool braced)
    {
        // JsonValueJson uses KeyMap (a case-sensitive HashMap) and rejects duplicate keys.
        var fields = new Dictionary<string, SyxDataNode>(StringComparer.Ordinal);
        if (braced) Expect('{');
        while (true)
        {
            SkipIgnored();
            if (End || braced && Peek() == '}')
            {
                if (braced && !End) _position++;
                break;
            }
            var key = ReadToken();
            SkipIgnored();
            Expect(':');
            var value = ParseValue();
            if (!fields.TryAdd(key, value)) throw Error($"Duplicate entry: {key}");
            SkipIgnored();
            // Some source text files close a one-line quoted value twice:
            //     DESC: "text"
            //     ",
            // The original loader accepts these relaxed localization files.  Treat
            // the isolated second quote as a redundant terminator instead of trying
            // to parse it as the next field name.
            if (!End && Peek() == '"' &&
                _position + 1 < _source.Length && _source[_position + 1] == ',')
            {
                _position++;
            }
            if (!End && Peek() == ',') _position++;
        }
        return new SyxDataNode { Fields = fields };
    }

    private SyxDataNode ParseArray()
    {
        Expect('[');
        var items = new List<SyxDataNode>();
        while (true)
        {
            SkipIgnored();
            if (End) throw Error("Unterminated array");
            if (Peek() == ']')
            {
                _position++;
                break;
            }
            items.Add(ParseArrayValue());
            SkipIgnored();
            if (!End && Peek() == ',') _position++;
        }
        return new SyxDataNode { Items = items };
    }

    private SyxDataNode ParseArrayValue()
    {
        SkipIgnored();
        if (Peek() is '{' or '[') return ParseValue();
        var token = ReadToken();
        SkipIgnored();
        if (!End && Peek() == ':')
        {
            _position++;
            return new SyxDataNode
            {
                Fields = new Dictionary<string, SyxDataNode>(StringComparer.Ordinal)
                {
                    [token] = ParseValue()
                }
            };
        }
        return new SyxDataNode { Scalar = token };
    }

    private SyxDataNode ParseValue()
    {
        SkipIgnored();
        if (End) throw Error("Expected value");
        return Peek() switch
        {
            '{' => ParseObject(true),
            '[' => ParseArray(),
            _ => new SyxDataNode { Scalar = ReadToken() }
        };
    }

    private string ReadToken()
    {
        SkipIgnored();
        if (End) throw Error("Expected token");
        if (Peek() == '"')
        {
            _position++;
            var result = new System.Text.StringBuilder();
            while (!End)
            {
                var c = _source[_position++];
                if (c == '"') return result.ToString();
                if (c == '\\' && !End)
                {
                    var escaped = _source[_position++];
                    result.Append(escaped switch
                    {
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        _ => escaped
                    });
                }
                else result.Append(c);
            }
            throw Error("Unterminated string");
        }

        var start = _position;
        while (!End)
        {
            var c = Peek();
            if (char.IsWhiteSpace(c) || c is ':' or ',' or '{' or '}' or '[' or ']') break;
            _position++;
        }
        if (start == _position) throw Error($"Unexpected character '{Peek()}'");
        return _source[start.._position];
    }

    private void SkipIgnored()
    {
        while (!End)
        {
            if (char.IsWhiteSpace(Peek()))
            {
                _position++;
                continue;
            }
            if (Peek() == '*' && _position + 1 < _source.Length && _source[_position + 1] == '*')
            {
                _position += 2;
                while (!End && Peek() != '\n') _position++;
                continue;
            }
            break;
        }
    }

    private void Expect(char expected)
    {
        SkipIgnored();
        if (End || Peek() != expected) throw Error($"Expected '{expected}'");
        _position++;
    }

    private char Peek() => _source[_position];
    private bool End => _position >= _source.Length;
    private FormatException Error(string message) =>
        new($"{message} at character {_position}");
}
