using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>
/// Builds one flat JSON object without a JSON library. Keys keep insertion order and
/// numbers use invariant culture, so the output is identical on German devices.
/// </summary>
public sealed class JsonFields
{
    private readonly StringBuilder _builder = new StringBuilder("{");
    private bool _hasFields;

    public JsonFields Add(string key, string value)
    {
        return AddRaw(key, value == null ? "null" : "\"" + Escape(value) + "\"");
    }

    public JsonFields Add(string key, double? value)
    {
        // NaN/Infinity are not valid JSON; one such token would break the whole JSONL file for R.
        bool finite = value.HasValue && !double.IsNaN(value.Value) && !double.IsInfinity(value.Value);
        return AddRaw(key, finite ? value.Value.ToString("R", CultureInfo.InvariantCulture) : "null");
    }

    public JsonFields Add(string key, int? value)
    {
        return AddRaw(key, value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : "null");
    }

    public JsonFields Add(string key, bool? value)
    {
        return AddRaw(key, value.HasValue ? (value.Value ? "true" : "false") : "null");
    }

    /// <summary>Adds already-valid JSON (object, array, number) without escaping.</summary>
    public JsonFields AddRaw(string key, string json)
    {
        if (_hasFields)
        {
            _builder.Append(',');
        }

        _hasFields = true;
        _builder.Append('"').Append(Escape(key)).Append("\":").Append(json ?? "null");
        return this;
    }

    public JsonFields AddStringList(string key, IReadOnlyList<string> values)
    {
        if (values == null)
        {
            return AddRaw(key, "null");
        }

        var list = new StringBuilder("[");
        for (int i = 0; i < values.Count; i++)
        {
            if (i > 0)
            {
                list.Append(',');
            }

            list.Append(values[i] == null ? "null" : "\"" + Escape(values[i]) + "\"");
        }

        return AddRaw(key, list.Append(']').ToString());
    }

    public string ToJson()
    {
        return _builder.ToString() + "}";
    }

    /// <summary>Escapes quotes, backslash and control characters; non-ASCII stays as UTF-8.</summary>
    public static string Escape(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(s.Length + 8);
        foreach (char c in s)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 0x20)
                    {
                        sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        sb.Append(c);
                    }

                    break;
            }
        }

        return sb.ToString();
    }
}
