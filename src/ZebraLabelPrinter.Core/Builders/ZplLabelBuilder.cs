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
                    sb.Append("^A").Append(profile.FontAlias).Append(rot).Append(',')
                      .Append(Scale(field.FontHeight, scale)).Append(',')
                      .Append(Scale(field.FontWidth, scale));
                    sb.Append("^FH^FD").Append(EncodeText(value, profile.TextEncoding)).Append("^FS");
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

        private static string EncodeText(string value, Encoding encoding)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var enc = encoding ?? Encoding.UTF8;
            var sb = new StringBuilder(value.Length);
            foreach (var ch in value)
            {
                if (ch == '^' || ch == '~' || ch == '\\')
                {
                    sb.Append('_').Append(((int)ch).ToString("X2", CultureInfo.InvariantCulture));
                }
                else if (ch > 0x7F)
                {
                    var bytes = enc.GetBytes(new[] { ch });
                    foreach (var b in bytes)
                        sb.Append('_').Append(b.ToString("X2", CultureInfo.InvariantCulture));
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
