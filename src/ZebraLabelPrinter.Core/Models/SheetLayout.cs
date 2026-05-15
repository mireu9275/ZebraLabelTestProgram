using System;

namespace ZebraLabelPrinter.Core.Models
{
    public enum SheetPageType
    {
        A4,
        Letter,
        Custom
    }

    public class SheetLayout
    {
        public SheetPageType PageType { get; set; } = SheetPageType.A4;

        // A4: 210x297mm, Letter: 215.9x279.4mm (Custom일 때만 수동 지정)
        public double PageWidthMm { get; set; } = 210;
        public double PageHeightMm { get; set; } = 297;

        public double MarginLeftMm { get; set; } = 10;
        public double MarginTopMm { get; set; } = 10;
        public double MarginRightMm { get; set; } = 10;
        public double MarginBottomMm { get; set; } = 10;

        // 라벨 사이 간격
        public double GapXMm { get; set; } = 2;
        public double GapYMm { get; set; } = 2;

        public void ApplyPageType()
        {
            switch (PageType)
            {
                case SheetPageType.A4:
                    PageWidthMm = 210; PageHeightMm = 297; break;
                case SheetPageType.Letter:
                    PageWidthMm = 215.9; PageHeightMm = 279.4; break;
                // Custom: 사용자 입력 유지
            }
        }

        // 페이지 한 장에 들어갈 라벨 grid 계산
        public (int rows, int cols, int perPage) ComputeGrid(double labelWidthMm, double labelHeightMm)
        {
            var availW = PageWidthMm - MarginLeftMm - MarginRightMm;
            var availH = PageHeightMm - MarginTopMm - MarginBottomMm;
            // n개 라벨 + (n-1)개 간격 ≤ avail → n ≤ (avail + gap) / (label + gap)
            var cols = (int)Math.Floor((availW + GapXMm) / (labelWidthMm + GapXMm));
            var rows = (int)Math.Floor((availH + GapYMm) / (labelHeightMm + GapYMm));
            cols = Math.Max(0, cols);
            rows = Math.Max(0, rows);
            return (rows, cols, rows * cols);
        }
    }
}
