using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ZebraLabelPrinter.Core.Models;

namespace ZebraLabelPrinter.Core.Builders
{
    public class ZplLabelBuilder
    {
        private readonly LabelTemplate _template;

        public ZplLabelBuilder(LabelTemplate template)
        {
            _template = template ?? throw new ArgumentNullException(nameof(template));
        }

        public string Build(IDictionary<string, string> data, int targetDpi, KoreanFontProfile profile = null)
        {
            profile = profile ?? KoreanFontProfile.Default();

            var scale = targetDpi > 0 && _template.SourceDpi > 0
                ? (double)targetDpi / _template.SourceDpi
                : 1.0;

            var sb = new StringBuilder();
            sb.Append("^XA");

            if (!string.IsNullOrEmpty(profile.HeaderCommands))
            {
                sb.Append(profile.HeaderCommands);
            }

            sb.Append("^PW").Append(Scale(_template.WidthDots, scale));
            sb.Append("^LL").Append(Scale(_template.HeightDots, scale));
            sb.Append("^LH0,0");

            foreach (var field in _template.Fields)
            {
                AppendField(sb, field, data, scale, profile);
            }

            if (_template.Copies > 1)
            {
                sb.Append("^PQ").Append(_template.Copies);
            }

            sb.Append("^XZ");
            return sb.ToString();
        }

        public static string Format(string zpl)
        {
            if (string.IsNullOrEmpty(zpl)) return string.Empty;
            return zpl.Replace("^", "\r\n^").TrimStart('\r', '\n');
        }

        private static void AppendField(StringBuilder sb, LabelField field, IDictionary<string, string> data, double scale, KoreanFontProfile profile)
        {
            var value = field.Resolve(data);
            var x = Scale(field.X, scale);
            var y = Scale(field.Y, scale);
            var rot = OrientationCode(field.Rotation);

            sb.Append("^FO").Append(x).Append(',').Append(y);

            switch (field.FieldType)
            {
                case LabelFieldType.Text:
                    {
                        var fontH = Scale(field.FontHeight, scale);
                        var fontW = Scale(field.FontWidth, scale);
                        var enc = EncodeText(value);
                        // 첫 출력 — ^FO는 위에서 emit됨
                        sb.Append("^A").Append(profile.FontAlias).Append(rot).Append(',')
                          .Append(fontH).Append(',').Append(fontW);
                        sb.Append("^FH^FD").Append(enc).Append("^FS");
                        // 굵기: 1픽셀씩 오프셋 overprint
                        var offsets = new[] { (1, 0), (0, 1), (1, 1) };
                        var bold = Math.Min(Math.Max(field.BoldStrength, 0), offsets.Length);
                        for (int i = 0; i < bold; i++)
                        {
                            var dx = offsets[i].Item1;
                            var dy = offsets[i].Item2;
                            sb.Append("^FO").Append(x + dx).Append(',').Append(y + dy);
                            sb.Append("^A").Append(profile.FontAlias).Append(rot).Append(',')
                              .Append(fontH).Append(',').Append(fontW);
                            sb.Append("^FH^FD").Append(enc).Append("^FS");
                        }
                    }
                    break;

                case LabelFieldType.Barcode128:
                    sb.Append("^BY2,2,").Append(Scale(field.Height > 0 ? field.Height : 80, scale));
                    sb.Append("^BC").Append(rot).Append(',')
                      .Append(Scale(field.Height > 0 ? field.Height : 80, scale))
                      .Append(",Y,N,N");
                    sb.Append("^FD").Append(EscapeData(value)).Append("^FS");
                    break;

                case LabelFieldType.BarcodeEan13:
                    sb.Append("^BE").Append(rot).Append(',')
                      .Append(Scale(field.Height > 0 ? field.Height : 80, scale))
                      .Append(",Y,N");
                    sb.Append("^FD").Append(EscapeData(value)).Append("^FS");
                    break;

                case LabelFieldType.QrCode:
                    var magnification = field.FontWidth > 0 ? field.FontWidth : 4;
                    sb.Append("^BQ").Append(rot).Append(",2,").Append(magnification);
                    sb.Append("^FDLA,").Append(EscapeData(value)).Append("^FS");
                    break;

                case LabelFieldType.Box:
                    sb.Append("^GB").Append(Scale(field.Width, scale)).Append(',')
                      .Append(Scale(field.Height, scale)).Append(',')
                      .Append(Scale(field.Thickness, scale)).Append("^FS");
                    break;

                case LabelFieldType.Line:
                    sb.Append("^GB").Append(Scale(field.Width, scale)).Append(',')
                      .Append(Scale(field.Thickness, scale)).Append(',')
                      .Append(Scale(field.Thickness, scale)).Append("^FS");
                    break;

                case LabelFieldType.Image:
                    if (!string.IsNullOrEmpty(field.ImagePath))
                    {
                        var imgW = field.Width > 0 ? Scale(field.Width, scale) : 0;
                        var imgH = field.Height > 0 ? Scale(field.Height, scale) : 0;
                        var gfa = ZplImageEncoder.ToGfa(field.ImagePath,
                            imgW > 0 ? (int?)imgW : null,
                            imgH > 0 ? (int?)imgH : null);
                        if (!string.IsNullOrEmpty(gfa)) sb.Append(gfa);
                    }
                    sb.Append("^FS");
                    break;
            }
        }

        private static string OrientationCode(LabelFieldRotation rot)
        {
            switch (rot)
            {
                case LabelFieldRotation.Rotate90: return "R";
                case LabelFieldRotation.Rotate180: return "I";
                case LabelFieldRotation.Rotate270: return "B";
                default: return "N";
            }
        }

        private static int Scale(int v, double scale)
        {
            if (scale == 1.0) return v;
            return (int)Math.Round(v * scale, MidpointRounding.AwayFromZero);
        }

        private static string EncodeText(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var sb = new StringBuilder(value.Length);
            foreach (var ch in value)
            {
                // ^FH 모드에서 ZPL 컨트롤 문자(^/~/\\)만 hex escape, 한글 등은 raw 유지.
                // 전송 시 UTF-8 바이트로 나가고 프린터의 ^CI28 모드가 해석.
                if (ch == '^' || ch == '~' || ch == '\\')
                {
                    sb.Append('_').Append(((int)ch).ToString("X2", CultureInfo.InvariantCulture));
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }

        private static string EscapeData(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace("^", "_5E").Replace("~", "_7E");
        }
    }
}
