using System;
using System.ComponentModel;

namespace ZebraLabelPrinter.Core.Models
{
    public enum LabelFieldType
    {
        Text = 0,
        Barcode128 = 1,
        BarcodeEan13 = 2,
        QrCode = 3,
        Image = 4,
        Box = 5,
        Line = 6
    }

    public enum LabelFieldRotation
    {
        Normal = 0,
        Rotate90 = 90,
        Rotate180 = 180,
        Rotate270 = 270
    }

    public class LabelField
    {
        [Category("1. 기본"), DisplayName("이름"), Description("필드 식별용 이름. 디자이너 캔버스 라벨에 표시되고 DataBindingKey 기본값으로 쓰임")]
        public string Name { get; set; }

        [Category("1. 기본"), DisplayName("필드 타입"), Description("Text / Barcode128 / BarcodeEan13 / QrCode / Box / Line")]
        public LabelFieldType FieldType { get; set; }

        [Category("1. 기본"), DisplayName("회전"), Description("Normal / 90 / 180 / 270도 회전 (ZPL ^A 의 N/R/I/B)")]
        public LabelFieldRotation Rotation { get; set; } = LabelFieldRotation.Normal;

        [Category("2. 위치"), DisplayName("X (dots)"), Description("라벨 좌측 가장자리로부터 X 좌표 (dots 단위)")]
        public int X { get; set; }

        [Category("2. 위치"), DisplayName("Y (dots)"), Description("라벨 상단 가장자리로부터 Y 좌표 (dots 단위)")]
        public int Y { get; set; }

        [Category("3. 크기"), DisplayName("폭 (dots)"), Description("바코드/박스/선의 폭. 텍스트/QR은 폰트 너비로 자동 계산")]
        public int Width { get; set; }

        [Category("3. 크기"), DisplayName("높이 (dots)"), Description("바코드/박스/선의 높이. 텍스트는 폰트 높이 사용")]
        public int Height { get; set; }

        [Category("3. 크기"), DisplayName("선 굵기 (dots)"), Description("박스/선의 테두리/선 굵기. ^GB의 thickness 파라미터")]
        public int Thickness { get; set; } = 2;

        [Category("4. 텍스트/QR"), DisplayName("폰트 너비 (dots)"), Description("텍스트: 글자 너비. QR: magnification(모듈 크기). 클수록 큼")]
        public int FontWidth { get; set; } = 20;

        [Category("4. 텍스트/QR"), DisplayName("폰트 높이 (dots)"), Description("텍스트 글자 높이. QR/바코드에는 사용 안 함")]
        public int FontHeight { get; set; } = 20;

        [Category("5. 데이터"), DisplayName("값 (기본)"), Description("바인딩 키가 비었거나 데이터 없을 때 이 값을 사용")]
        public string Value { get; set; }

        [Category("5. 데이터"), DisplayName("바인딩 키"), Description("이 키가 좌측 '데이터 바인딩' 패널에 입력란으로 자동 노출됨. 비우면 고정 값 사용")]
        public string DataBindingKey { get; set; }

        [Category("6. 이미지"), DisplayName("이미지 경로"), Description("이미지 필드일 때 로컬 파일 경로 (현재 미구현)")]
        public string ImagePath { get; set; }

        public string Resolve(System.Collections.Generic.IDictionary<string, string> data)
        {
            if (string.IsNullOrEmpty(DataBindingKey)) return Value ?? string.Empty;
            if (data == null) return Value ?? string.Empty;
            string v;
            return data.TryGetValue(DataBindingKey, out v) ? v : (Value ?? string.Empty);
        }
    }
}
