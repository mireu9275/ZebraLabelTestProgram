using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using ZebraLabelPrinter.Core.Models;

namespace ZebraLabelPrinter.UI.Forms
{
    public class CodeExportForm : Form
    {
        private TextBox _txt;
        private Button _btnCopy;
        private Button _btnClose;

        public CodeExportForm(LabelTemplate template)
        {
            this.Text = "C# 코드 내보내기 — 내 프로그램에 붙여넣기";
            this.ClientSize = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(600, 400);

            _txt = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font("Consolas", 10f),
                Dock = DockStyle.Fill,
                ReadOnly = false  // 사용자가 수정 후 복사도 가능
            };

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10)
            };

            _btnCopy = new Button
            {
                Text = "📋 클립보드 복사",
                Size = new Size(150, 32),
                Location = new Point(10, 10),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            _btnCopy.Click += BtnCopy_Click;

            _btnClose = new Button
            {
                Text = "닫기",
                Size = new Size(80, 32),
                Location = new Point(170, 10)
            };
            _btnClose.Click += (s, e) => Close();

            pnlBottom.Controls.Add(_btnCopy);
            pnlBottom.Controls.Add(_btnClose);

            this.Controls.Add(_txt);
            this.Controls.Add(pnlBottom);

            _txt.Text = GenerateCode(template);
            _txt.SelectionStart = 0;
            _txt.SelectionLength = 0;
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(_txt.Text);
                MessageBox.Show(this, "클립보드에 복사 완료", "복사", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "복사 실패: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GenerateCode(LabelTemplate t)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// ZebraLabelPrinter 자동 생성 코드");
            sb.AppendLine("// 필요한 using:");
            sb.AppendLine("//   using ZebraLabelPrinter.Core.Builders;");
            sb.AppendLine("//   using ZebraLabelPrinter.Core.Models;");
            sb.AppendLine("//   using ZebraLabelPrinter.Core.Preview;");
            sb.AppendLine("//   using ZebraLabelPrinter.Core.Printing;");
            sb.AppendLine();
            sb.AppendLine("var template = new LabelTemplate");
            sb.AppendLine("{");
            sb.AppendLine("    TemplateCode = " + CSharpString(t.TemplateCode) + ",");
            sb.AppendLine("    TemplateName = " + CSharpString(t.TemplateName) + ",");
            sb.AppendLine("    WidthDots = " + t.WidthDots + ",");
            sb.AppendLine("    HeightDots = " + t.HeightDots + ",");
            sb.AppendLine("    SourceDpi = " + t.SourceDpi + ",");
            sb.AppendLine("    Copies = " + t.Copies);
            sb.AppendLine("};");
            sb.AppendLine();

            for (int i = 0; i < t.Fields.Count; i++)
            {
                var f = t.Fields[i];
                sb.AppendLine("template.Fields.Add(new LabelField");
                sb.AppendLine("{");
                sb.AppendLine("    Name = " + CSharpString(f.Name) + ",");
                sb.AppendLine("    FieldType = LabelFieldType." + f.FieldType + ",");
                sb.AppendLine("    X = " + f.X + ", Y = " + f.Y + ",");
                if (f.Width != 0 || f.Height != 0)
                    sb.AppendLine("    Width = " + f.Width + ", Height = " + f.Height + ",");
                if (f.FontWidth != 20 || f.FontHeight != 20)
                    sb.AppendLine("    FontWidth = " + f.FontWidth + ", FontHeight = " + f.FontHeight + ",");
                if (f.Rotation != LabelFieldRotation.Normal)
                    sb.AppendLine("    Rotation = LabelFieldRotation." + f.Rotation + ",");
                if (f.Thickness != 2)
                    sb.AppendLine("    Thickness = " + f.Thickness + ",");
                if (f.BoldStrength > 0)
                    sb.AppendLine("    BoldStrength = " + f.BoldStrength + ",");
                if (!string.IsNullOrEmpty(f.Value))
                    sb.AppendLine("    Value = " + CSharpString(f.Value) + ",");
                if (!string.IsNullOrEmpty(f.DataBindingKey))
                    sb.AppendLine("    DataBindingKey = " + CSharpString(f.DataBindingKey) + ",");
                if (!string.IsNullOrEmpty(f.ImagePath))
                    sb.AppendLine("    ImagePath = " + CSharpString(f.ImagePath) + ",");

                // 마지막 trailing comma 제거 (C# 5.x 이하 호환을 위해)
                if (sb.Length >= 3 && sb[sb.Length - 3] == ',') sb.Remove(sb.Length - 3, 1);

                sb.AppendLine("});");
                sb.AppendLine();
            }

            sb.AppendLine("// =============================================");
            sb.AppendLine("// 사용 예 1 — Zebra 프린터로 ZPL 직접 전송");
            sb.AppendLine("// =============================================");
            sb.AppendLine("var data = new System.Collections.Generic.Dictionary<string, string>");
            sb.AppendLine("{");
            sb.AppendLine("    // { \"PART_NO\", \"ABC-12345\" }, ...");
            sb.AppendLine("};");
            sb.AppendLine("var zpl = new ZplLabelBuilder(template).Build(data, " + t.SourceDpi + ", KoreanFontProfile.Kfont3());");
            sb.AppendLine("new WindowsSpoolerClient().Send(");
            sb.AppendLine("    new PrinterConfig {");
            sb.AppendLine("        ConnectionType = PrinterConnectionType.WindowsSpooler,");
            sb.AppendLine("        SpoolerName = \"여기에 프린터 이름\"");
            sb.AppendLine("    },");
            sb.AppendLine("    zpl);");
            sb.AppendLine();
            sb.AppendLine("// =============================================");
            sb.AppendLine("// 사용 예 2 — A4 시트로 N장 출력 (PrintDocument)");
            sb.AppendLine("// =============================================");
            sb.AppendLine("var renderer = new LabelPreviewService();");
            sb.AppendLine("renderer.TryRegisterCjkFontByFamilyName(\"Malgun Gothic\");");
            sb.AppendLine();
            sb.AppendLine("var dataList = new System.Collections.Generic.List<System.Collections.Generic.IDictionary<string, string>>();");
            sb.AppendLine("for (int i = 0; i < 10; i++) dataList.Add(data);  // 같은 데이터로 10장");
            sb.AppendLine();
            sb.AppendLine("new SheetPrintService().Print(new SheetPrintRequest {");
            sb.AppendLine("    Template = template,");
            sb.AppendLine("    Profile = KoreanFontProfile.Kfont3(),");
            sb.AppendLine("    Renderer = renderer,");
            sb.AppendLine("    Sheet = new SheetLayout {");
            sb.AppendLine("        PageType = SheetPageType.A4,");
            sb.AppendLine("        MarginLeftMm = 10, MarginTopMm = 10,");
            sb.AppendLine("        MarginRightMm = 10, MarginBottomMm = 10,");
            sb.AppendLine("        GapXMm = 2, GapYMm = 2,");
            sb.AppendLine("        DrawCutLines = true");
            sb.AppendLine("    },");
            sb.AppendLine("    DataPerLabel = dataList,");
            sb.AppendLine("    WindowsPrinterName = \"여기에 A4 프린터 이름\"");
            sb.AppendLine("});");

            return sb.ToString();
        }

        private static string CSharpString(string s)
        {
            if (s == null) return "null";
            return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n") + "\"";
        }
    }
}
