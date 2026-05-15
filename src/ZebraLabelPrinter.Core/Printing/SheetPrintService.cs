using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using ZebraLabelPrinter.Core.Builders;
using ZebraLabelPrinter.Core.Models;
using ZebraLabelPrinter.Core.Preview;

namespace ZebraLabelPrinter.Core.Printing
{
    public class SheetPrintRequest
    {
        public string WindowsPrinterName { get; set; }   // null이면 기본 프린터
        public LabelTemplate Template { get; set; }
        public KoreanFontProfile Profile { get; set; }
        public SheetLayout Sheet { get; set; }
        public int TargetDpi { get; set; } = 203;        // ZPL 좌표 스케일링용
        public int RenderDpmm { get; set; } = 24;        // 라벨 PNG 화질 (24dpmm ≈ 609dpi, A4 인쇄용)
        public LabelPreviewService Renderer { get; set; } // 폰트 등록된 인스턴스 재사용
        public List<IDictionary<string, string>> DataPerLabel { get; set; }
                                                          // 각 라벨에 바인딩될 데이터.
                                                          // null이면 빈 dictionary로 Count장 출력
        public int Count { get; set; } = 1;               // DataPerLabel 없을 때 출력할 장수
    }

    public class SheetPrintService
    {
        // PrintDocument를 빌드만 함. 호출자가 doc.Print() 또는 PrintPreviewDialog로 활용.
        // 라벨 이미지는 doc.EndPrint에서 자동 dispose됨.
        public PrintDocument BuildPrintDocument(SheetPrintRequest req)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));
            if (req.Template == null) throw new ArgumentException("Template is null");
            if (req.Sheet == null) throw new ArgumentException("Sheet is null");
            if (req.Renderer == null) throw new ArgumentException("Renderer is null");

            var profile = req.Profile ?? KoreanFontProfile.Default();
            var labelWidthMm = req.Template.WidthDots * 25.4 / Math.Max(1, req.Template.SourceDpi);
            var labelHeightMm = req.Template.HeightDots * 25.4 / Math.Max(1, req.Template.SourceDpi);

            var data = req.DataPerLabel;
            if (data == null || data.Count == 0)
            {
                data = new List<IDictionary<string, string>>();
                for (int i = 0; i < Math.Max(1, req.Count); i++) data.Add(new Dictionary<string, string>());
            }

            // 라벨 PNG 미리 렌더 — 모든 페이지에서 같은 인덱스로 재사용
            var labelImages = new List<Bitmap>();
            var builder = new ZplLabelBuilder(req.Template);
            foreach (var d in data)
            {
                var zpl = builder.Build(d, req.TargetDpi, profile);
                var png = req.Renderer.RenderPng(zpl, req.Template.WidthDots, req.Template.HeightDots, req.RenderDpmm);
                labelImages.Add(new Bitmap(new MemoryStream(png)));
            }

            var doc = new PrintDocument();
            if (!string.IsNullOrEmpty(req.WindowsPrinterName))
                doc.PrinterSettings.PrinterName = req.WindowsPrinterName;

            if (req.Sheet.PageType == SheetPageType.A4)
                doc.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            else if (req.Sheet.PageType == SheetPageType.Letter)
                doc.DefaultPageSettings.PaperSize = new PaperSize("Letter", 850, 1100);
            else
                doc.DefaultPageSettings.PaperSize = new PaperSize("Custom",
                    (int)Math.Round(req.Sheet.PageWidthMm / 25.4 * 100),
                    (int)Math.Round(req.Sheet.PageHeightMm / 25.4 * 100));

            int labelIndex = 0;
            doc.BeginPrint += (s, e) => { labelIndex = 0; };
            doc.PrintPage += (s, e) =>
            {
                e.Graphics.PageUnit = GraphicsUnit.Millimeter;
                var (rows, cols, perPage) = req.Sheet.ComputeGrid(labelWidthMm, labelHeightMm);
                if (perPage <= 0) { e.HasMorePages = false; return; }

                for (int r = 0; r < rows && labelIndex < labelImages.Count; r++)
                {
                    for (int c = 0; c < cols && labelIndex < labelImages.Count; c++)
                    {
                        var img = labelImages[labelIndex];
                        var x = (float)(req.Sheet.MarginLeftMm + c * (labelWidthMm + req.Sheet.GapXMm));
                        var y = (float)(req.Sheet.MarginTopMm + r * (labelHeightMm + req.Sheet.GapYMm));
                        e.Graphics.DrawImage(img, new RectangleF(x, y, (float)labelWidthMm, (float)labelHeightMm));
                        labelIndex++;
                    }
                }
                e.HasMorePages = labelIndex < labelImages.Count;
            };
            doc.EndPrint += (s, e) =>
            {
                foreach (var bmp in labelImages) bmp.Dispose();
                labelImages.Clear();
            };

            return doc;
        }

        // 직접 출력 (미리보기 없이)
        public void Print(SheetPrintRequest req)
        {
            using (var doc = BuildPrintDocument(req))
            {
                doc.Print();
            }
        }
    }
}
