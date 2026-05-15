using ZebraLabelPrinter.Core.Models;

namespace ZebraLabelPrinter.UI.Sample
{
    public static class SampleTemplates
    {
        public static LabelTemplate PartLabel100x50()
        {
            var t = new LabelTemplate
            {
                TemplateCode = "PART_100x50",
                TemplateName = "부품 라벨 100x50mm",
                SourceDpi = 203,
                WidthDots = 800,
                HeightDots = 400,
                UseUtf8 = true,
                Copies = 1
            };

            t.Fields.Add(new LabelField
            {
                Name = "Title",
                FieldType = LabelFieldType.Text,
                X = 30, Y = 20,
                FontWidth = 30, FontHeight = 30,
                Value = "부품 라벨"
            });

            t.Fields.Add(new LabelField
            {
                Name = "PartNoLabel",
                FieldType = LabelFieldType.Text,
                X = 30, Y = 80,
                FontWidth = 24, FontHeight = 24,
                Value = "PART NO:"
            });

            t.Fields.Add(new LabelField
            {
                Name = "PartNo",
                FieldType = LabelFieldType.Text,
                X = 180, Y = 80,
                FontWidth = 28, FontHeight = 28,
                DataBindingKey = "PART_NO",
                Value = "PART-12345"
            });

            t.Fields.Add(new LabelField
            {
                Name = "LotNoLabel",
                FieldType = LabelFieldType.Text,
                X = 30, Y = 130,
                FontWidth = 24, FontHeight = 24,
                Value = "LOT NO:"
            });

            t.Fields.Add(new LabelField
            {
                Name = "LotNo",
                FieldType = LabelFieldType.Text,
                X = 180, Y = 130,
                FontWidth = 28, FontHeight = 28,
                DataBindingKey = "LOT_NO",
                Value = "2605150099"
            });

            t.Fields.Add(new LabelField
            {
                Name = "Barcode",
                FieldType = LabelFieldType.Barcode128,
                X = 30, Y = 200,
                Height = 100,
                DataBindingKey = "PART_NO",
                Value = "PART-12345"
            });

            t.Fields.Add(new LabelField
            {
                Name = "Qr",
                FieldType = LabelFieldType.QrCode,
                X = 600, Y = 80,
                FontWidth = 5,
                DataBindingKey = "QR",
                Value = "PART-12345|2605150099"
            });

            return t;
        }
    }
}
