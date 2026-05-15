using System.Drawing;
using System.IO;
using System.Text;

namespace ZebraLabelPrinter.Core.Builders
{
    public static class ZplImageEncoder
    {
        // 이미지를 ZPL ^GFA hex로 변환. 1비트 비트맵 (흑백) 형식.
        // 반환 문자열에는 ^GFA 명령이 포함됨 (^FS는 호출자가 붙임).
        public static string ToGfa(string filePath, int? targetWidth = null, int? targetHeight = null)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return string.Empty;
            using (var src = new Bitmap(filePath))
            {
                return ToGfa(src, targetWidth, targetHeight);
            }
        }

        public static string ToGfa(Bitmap src, int? targetWidth = null, int? targetHeight = null)
        {
            if (src == null) return string.Empty;
            int w = targetWidth.HasValue && targetWidth.Value > 0 ? targetWidth.Value : src.Width;
            int h = targetHeight.HasValue && targetHeight.Value > 0 ? targetHeight.Value : src.Height;

            using (var resized = (w == src.Width && h == src.Height) ? new Bitmap(src) : new Bitmap(src, w, h))
            {
                int bytesPerRow = (w + 7) / 8;
                int totalBytes = bytesPerRow * h;
                var sb = new StringBuilder(totalBytes * 2 + 32);
                sb.Append("^GFA,").Append(totalBytes).Append(',').Append(totalBytes).Append(',').Append(bytesPerRow).Append(',');

                // 임계값 기반 dithering 없는 단순 흑백 변환
                // brightness < 384(=255*3/2)면 검정, 아니면 흰색
                for (int y = 0; y < h; y++)
                {
                    int byteVal = 0;
                    int bitPos = 0;
                    for (int x = 0; x < w; x++)
                    {
                        var p = resized.GetPixel(x, y);
                        int brightness = p.R + p.G + p.B;
                        bool isBlack = brightness < 384;
                        byteVal = (byteVal << 1) | (isBlack ? 1 : 0);
                        bitPos++;
                        if (bitPos == 8)
                        {
                            sb.Append(((byte)byteVal).ToString("X2"));
                            byteVal = 0;
                            bitPos = 0;
                        }
                    }
                    if (bitPos > 0)
                    {
                        byteVal <<= (8 - bitPos);
                        sb.Append(((byte)byteVal).ToString("X2"));
                    }
                }
                return sb.ToString();
            }
        }
    }
}
