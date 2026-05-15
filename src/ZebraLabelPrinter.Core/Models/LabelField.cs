using System;

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
        public string Name { get; set; }
        public LabelFieldType FieldType { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public int FontWidth { get; set; } = 20;
        public int FontHeight { get; set; } = 20;
        public LabelFieldRotation Rotation { get; set; } = LabelFieldRotation.Normal;

        public string Value { get; set; }

        public string DataBindingKey { get; set; }

        public int Thickness { get; set; } = 2;

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
