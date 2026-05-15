using System;
using System.IO;
using BinaryKits.Zpl.Viewer;

namespace ZebraLabelPrinter.Core.Preview
{
    public class LabelPreviewService
    {
        public byte[] RenderPng(string zpl, int labelWidthDots = 800, int labelHeightDots = 400, int printDensityDpmm = 8)
        {
            if (string.IsNullOrEmpty(zpl)) throw new ArgumentException("ZPL is empty", nameof(zpl));

            IPrinterStorage storage = new PrinterStorage();
            var analyzer = new ZplAnalyzer(storage);
            var info = analyzer.Analyze(zpl);
            if (info == null || info.LabelInfos == null || info.LabelInfos.Length == 0)
            {
                throw new InvalidOperationException("ZPL analysis returned no labels");
            }

            var widthMm = labelWidthDots / (double)printDensityDpmm;
            var heightMm = labelHeightDots / (double)printDensityDpmm;

            var drawer = new ZplElementDrawer(storage);
            return drawer.Draw(info.LabelInfos[0].ZplElements, widthMm, heightMm, printDensityDpmm);
        }

        public void RenderPngToFile(string zpl, string outputPath, int labelWidthDots = 800, int labelHeightDots = 400, int printDensityDpmm = 8)
        {
            var bytes = RenderPng(zpl, labelWidthDots, labelHeightDots, printDensityDpmm);
            File.WriteAllBytes(outputPath, bytes);
        }
    }
}
