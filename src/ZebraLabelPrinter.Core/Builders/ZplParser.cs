using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ZebraLabelPrinter.Core.Models;

namespace ZebraLabelPrinter.Core.Builders
{
    // ZPL 문자열을 LabelTemplate으로 역파싱. 우리가 생성하는 형식 위주로 동작하며,
    // 외부 ZPL은 best-effort (인식 못하는 명령은 무시).
    public static class ZplParser
    {
        public static LabelTemplate Parse(string zpl)
        {
            var t = new LabelTemplate
            {
                TemplateCode = "IMPORTED",
                TemplateName = "ZPL 가져오기",
                SourceDpi = 203,
                WidthDots = 800,
                HeightDots = 400,
                Copies = 1,
                Fields = new List<LabelField>()
            };
            if (string.IsNullOrEmpty(zpl)) return t;

            var clean = zpl.Replace("\r", "").Replace("\n", "");
            var tokens = clean.Split('^');

            int curX = 0, curY = 0;
            char fontAlias = '0';
            char rotation = 'N';
            int fontH = 30, fontW = 30;
            int barcodeHeight = 80;
            string mode = null; // text/bc/bq/be
            int fieldCount = 0;

            foreach (var raw in tokens)
            {
                if (string.IsNullOrEmpty(raw)) continue;
                var tok = raw;

                // 무시할 명령들
                if (tok.StartsWith("XA") || tok.StartsWith("XZ") || tok.StartsWith("LH") ||
                    tok.StartsWith("CI") || tok.StartsWith("CW") || tok.StartsWith("SE") ||
                    tok.StartsWith("BY"))
                    continue;

                if (tok.StartsWith("PW"))
                {
                    if (int.TryParse(LeadingDigits(tok.Substring(2)), out var pw) && pw > 0) t.WidthDots = pw;
                    continue;
                }
                if (tok.StartsWith("LL"))
                {
                    if (int.TryParse(LeadingDigits(tok.Substring(2)), out var ll) && ll > 0) t.HeightDots = ll;
                    continue;
                }
                if (tok.StartsWith("PQ"))
                {
                    var p = tok.Substring(2).Split(',');
                    if (p.Length > 0 && int.TryParse(LeadingDigits(p[0]), out var pq)) t.Copies = Math.Max(1, pq);
                    continue;
                }
                if (tok.StartsWith("FO"))
                {
                    var p = tok.Substring(2).Split(',');
                    if (p.Length >= 2 &&
                        int.TryParse(LeadingDigits(p[0]), out var x) &&
                        int.TryParse(LeadingDigits(p[1]), out var y))
                    { curX = x; curY = y; }
                    continue;
                }
                // 폰트 ^A0 / ^A1 / ^A@ ...
                if (tok[0] == 'A' && tok.Length >= 2 && (char.IsLetterOrDigit(tok[1]) || tok[1] == '@'))
                {
                    fontAlias = tok[1];
                    var rest = tok.Substring(2);
                    if (rest.Length > 0 && "NRIB".IndexOf(rest[0]) >= 0) { rotation = rest[0]; rest = rest.Substring(1); }
                    rest = rest.TrimStart(',');
                    var p = rest.Split(',');
                    if (p.Length >= 1 && int.TryParse(LeadingDigits(p[0]), out var h) && h > 0) fontH = h;
                    if (p.Length >= 2 && int.TryParse(LeadingDigits(p[1]), out var w) && w > 0) fontW = w;
                    mode = "text";
                    continue;
                }
                if (tok.StartsWith("BC")) { mode = "bc"; rotation = FirstRot(tok.Substring(2), rotation); barcodeHeight = NthInt(tok.Substring(2), 1, 80); continue; }
                if (tok.StartsWith("BE")) { mode = "be"; rotation = FirstRot(tok.Substring(2), rotation); barcodeHeight = NthInt(tok.Substring(2), 1, 80); continue; }
                if (tok.StartsWith("BQ")) { mode = "bq"; continue; }
                if (tok.StartsWith("GB"))
                {
                    var p = tok.Substring(2).Split(',');
                    int w = 0, h = 0, th = 2;
                    if (p.Length >= 1) int.TryParse(LeadingDigits(p[0]), out w);
                    if (p.Length >= 2) int.TryParse(LeadingDigits(p[1]), out h);
                    if (p.Length >= 3) int.TryParse(LeadingDigits(p[2]), out th);
                    t.Fields.Add(new LabelField
                    {
                        Name = "Box" + (++fieldCount),
                        FieldType = LabelFieldType.Box,
                        X = curX, Y = curY,
                        Width = w, Height = h, Thickness = Math.Max(1, th)
                    });
                    mode = null;
                    continue;
                }
                if (tok.StartsWith("FH")) continue; // hex indicator — 다음 FD에서 처리
                if (tok.StartsWith("FD"))
                {
                    var data = DecodeFieldData(tok.Substring(2));
                    var f = new LabelField { X = curX, Y = curY, Rotation = MapRot(rotation) };
                    switch (mode)
                    {
                        case "bc":
                            f.FieldType = LabelFieldType.Barcode128;
                            f.Height = barcodeHeight;
                            f.Name = "Barcode" + (++fieldCount);
                            f.Value = data;
                            break;
                        case "be":
                            f.FieldType = LabelFieldType.BarcodeEan13;
                            f.Height = barcodeHeight;
                            f.Name = "Ean13_" + (++fieldCount);
                            f.Value = data;
                            break;
                        case "bq":
                            f.FieldType = LabelFieldType.QrCode;
                            f.FontWidth = 5;
                            f.Name = "QR" + (++fieldCount);
                            f.Value = StripQrPrefix(data);
                            break;
                        default:
                            f.FieldType = LabelFieldType.Text;
                            f.FontHeight = fontH;
                            f.FontWidth = fontW;
                            f.Name = "Text" + (++fieldCount);
                            f.Value = data;
                            break;
                    }
                    t.Fields.Add(f);
                    continue;
                }
                if (tok.StartsWith("FS")) { mode = null; continue; }
            }

            return t;
        }

        private static string LeadingDigits(string s)
        {
            if (string.IsNullOrEmpty(s)) return "0";
            var sb = new StringBuilder();
            foreach (var c in s.TrimStart())
            {
                if (char.IsDigit(c) || (sb.Length == 0 && c == '-')) sb.Append(c);
                else break;
            }
            return sb.Length == 0 ? "0" : sb.ToString();
        }

        private static char FirstRot(string s, char fallback)
        {
            if (!string.IsNullOrEmpty(s) && "NRIB".IndexOf(s[0]) >= 0) return s[0];
            return fallback;
        }

        private static int NthInt(string s, int index, int fallback)
        {
            var parts = s.Split(',');
            if (parts.Length > index && int.TryParse(LeadingDigits(parts[index]), out var v) && v > 0) return v;
            return fallback;
        }

        private static LabelFieldRotation MapRot(char r)
        {
            switch (r)
            {
                case 'R': return LabelFieldRotation.Rotate90;
                case 'I': return LabelFieldRotation.Rotate180;
                case 'B': return LabelFieldRotation.Rotate270;
                default: return LabelFieldRotation.Normal;
            }
        }

        // QR ^FD는 "LA,데이터" / "QA,데이터" 같이 에러정정+마스크 2글자 + 콤마 prefix가 붙음 → 제거
        private static string StripQrPrefix(string data)
        {
            if (string.IsNullOrEmpty(data)) return data;
            if (data.Length >= 3 && data[2] == ',' && char.IsLetter(data[0]))
                return data.Substring(3);
            return data;
        }

        // ^FH 모드의 _XX hex escape를 디코딩. raw 문자는 그대로 두고 UTF-8로 재조합.
        private static string DecodeFieldData(string raw)
        {
            if (string.IsNullOrEmpty(raw) || raw.IndexOf('_') < 0) return raw;
            var bytes = new List<byte>();
            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i] == '_' && i + 2 < raw.Length && IsHex(raw[i + 1]) && IsHex(raw[i + 2]))
                {
                    bytes.Add(byte.Parse(raw.Substring(i + 1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                    i += 2;
                }
                else
                {
                    bytes.AddRange(Encoding.UTF8.GetBytes(raw[i].ToString()));
                }
            }
            try { return Encoding.UTF8.GetString(bytes.ToArray()); }
            catch { return raw; }
        }

        private static bool IsHex(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
        }
    }
}
