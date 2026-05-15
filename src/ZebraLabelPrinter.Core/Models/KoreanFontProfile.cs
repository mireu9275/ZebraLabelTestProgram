namespace ZebraLabelPrinter.Core.Models
{
    public class KoreanFontProfile
    {
        public string DisplayName { get; set; } = "기본 (^CI28, ^A0)";

        public string HeaderCommands { get; set; } = "^CI28";

        public char FontAlias { get; set; } = '0';

        public static KoreanFontProfile Default()
        {
            return new KoreanFontProfile();
        }

        public static KoreanFontProfile Kfont3()
        {
            return new KoreanFontProfile
            {
                DisplayName = "Kfont3 UTF-8 (^CI28, ^A1, KFONT3)",
                HeaderCommands = "^SEE:UHANGUL.DAT^FS^CW1,E:KFONT3.FNT^CI28",
                FontAlias = '1'
            };
        }
    }
}
