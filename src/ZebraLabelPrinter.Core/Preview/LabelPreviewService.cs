using System;
using System.IO;
using BinaryKits.Zpl.Viewer;

namespace ZebraLabelPrinter.Core.Preview
{
    public class LabelPreviewService
    {
        private readonly IPrinterStorage _storage = new PrinterStorage();

        public void RegisterFont(char storageDevice, string fileName, byte[] fontBytes)
        {
            if (fontBytes == null || fontBytes.Length == 0) return;
            _storage.AddFile(storageDevice, fileName, fontBytes);
        }

        public bool TryRegisterFontFromFile(char storageDevice, string fileName, string localPath)
        {
            if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath)) return false;
            try
            {
                _storage.AddFile(storageDevice, fileName, File.ReadAllBytes(localPath));
                return true;
            }
            catch
            {
                return false;
            }
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

            var widthMm = labelWidthDots / (double)printDensityDpmm;
            var heightMm = labelHeightDots / (double)printDensityDpmm;

            var drawer = new ZplElementDrawer(_storage);
            return drawer.Draw(info.LabelInfos[0].ZplElements, widthMm, heightMm, printDensityDpmm);
        }

        public void RenderPngToFile(string zpl, string outputPath, int labelWidthDots = 800, int labelHeightDots = 400, int printDensityDpmm = 8)
        {
            var bytes = RenderPng(zpl, labelWidthDots, labelHeightDots, printDensityDpmm);
            File.WriteAllBytes(outputPath, bytes);
        }
    }
}
