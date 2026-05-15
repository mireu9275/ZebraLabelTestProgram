using System.Text;

namespace ZebraLabelPrinter.Core.Models
{
    public class KoreanFontProfile
    {
        public string DisplayName { get; set; } = "기본 (UTF-8, ^A0)";

        public string HeaderCommands { get; set; } = "^CI28";

        public char FontAlias { get; set; } = '0';

        public Encoding TextEncoding { get; set; } = Encoding.UTF8;

        public static KoreanFontProfile Default()
        {
            return new KoreanFontProfile();
        }

        public static KoreanFontProfile Kfont3()
        {
            Encoding enc;
            try { enc = Encoding.GetEncoding(949); }
            catch { enc = Encoding.UTF8; }

            return new KoreanFontProfile
            {
                DisplayName = "Kfont3 (^CI26 + ^A1 + KFONT3.FNT)",
                HeaderCommands = "^SEE:UHANGUL.DAT^FS^CW1,E:KFONT3.FNT^CI26",
                FontAlias = '1',
                TextEncoding = enc
            };
        }
    }
}
