using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BinaryKits.Zpl.Viewer;
using BinaryKits.Zpl.Viewer.ElementDrawers;

namespace ZebraLabelPrinter.Core.Preview
{
    public class LabelPreviewService
    {
        private readonly IPrinterStorage _storage = new PrinterStorage();
        private readonly List<string> _preferredFontFamilies = new List<string>();

        public bool TryRegisterCjkFontByFamilyName(string familyName)
        {
            if (string.IsNullOrEmpty(familyName)) return false;
            if (!_preferredFontFamilies.Contains(familyName))
            {
                _preferredFontFamilies.Insert(0, familyName);
            }
            return true;
        }

        public byte[] RenderPng(string zpl, int labelWidthDots = 800, int labelHeightDots = 400, int printDensityDpmm = 8)
        {
            if (string.IsNullOrEmpty(zpl)) throw new ArgumentException("ZPL is empty", nameof(zpl));

            var analyzer = new ZplAnalyzer(_storage);
            var info = analyzer.Analyze(zpl);
            if (info == null || info.LabelInfos == null || info.LabelInfos.Length == 0)
            {
                throw new InvalidOperationException("ZPL analysis returned no labels");
            }

            var options = new DrawerOptions
            {
                OpaqueBackground = true
            };
            ApplyPreferredFonts(options.FontManager);

            var widthMm = labelWidthDots / (double)printDensityDpmm;
            var heightMm = labelHeightDots / (double)printDensityDpmm;

            var drawer = new ZplElementDrawer(_storage, options);
            return drawer.Draw(info.LabelInfos[0].ZplElements, widthMm, heightMm, printDensityDpmm);
        }

        public void RenderPngToFile(string zpl, string outputPath, int labelWidthDots = 800, int labelHeightDots = 400, int printDensityDpmm = 8)
        {
            var bytes = RenderPng(zpl, labelWidthDots, labelHeightDots, printDensityDpmm);
            File.WriteAllBytes(outputPath, bytes);
        }

        private void ApplyPreferredFonts(FontManager fm)
        {
            if (fm == null || _preferredFontFamilies.Count == 0) return;

            fm.FontStack0 = _preferredFontFamilies.Concat(fm.FontStack0 ?? new List<string>()).Distinct().ToList();
            fm.FontStackA = _preferredFontFamilies.Concat(fm.FontStackA ?? new List<string>()).Distinct().ToList();
        }
    }
}
