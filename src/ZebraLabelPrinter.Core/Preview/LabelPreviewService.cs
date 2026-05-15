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
            var drawer = new ZplElementDrawer(storage, new DrawerOptions
            {
                LabelHeight = labelHeightDots,
                LabelWidth = labelWidthDots,
                Dpmm = printDensityDpmm,
                OpaqueBackground = true
            });

            var bytes = drawer.DrawSingleLabelAsByteArray(zpl);
            return bytes;
        }

        public void RenderPngToFile(string zpl, string outputPath, int labelWidthDots = 800, int labelHeightDots = 400, int printDensityDpmm = 8)
        {
            var bytes = RenderPng(zpl, labelWidthDots, labelHeightDots, printDensityDpmm);
            File.WriteAllBytes(outputPath, bytes);
        }
    }
}
