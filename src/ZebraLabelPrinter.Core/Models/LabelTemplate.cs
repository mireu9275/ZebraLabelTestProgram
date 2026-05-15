using System.Collections.Generic;

namespace ZebraLabelPrinter.Core.Models
{
    public class LabelTemplate
    {
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }

        public int WidthDots { get; set; } = 800;
        public int HeightDots { get; set; } = 400;

        public int SourceDpi { get; set; } = 203;

        public int Copies { get; set; } = 1;

        public bool UseUtf8 { get; set; } = true;

        public string FontReference { get; set; }

        public List<LabelField> Fields { get; set; } = new List<LabelField>();
    }
}
