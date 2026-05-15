using System;
using System.Text;
using ZebraLabelPrinter.Core.Models;

namespace ZebraLabelPrinter.Core.Printing
{
    public class WindowsSpoolerClient : IPrinterClient
    {
        public PrintResult Send(PrinterConfig config, string zpl)
        {
            if (config == null) return PrintResult.Fail("PrinterConfig is null");
            if (string.IsNullOrEmpty(config.SpoolerName)) return PrintResult.Fail("SpoolerName is empty");
            if (string.IsNullOrEmpty(zpl)) return PrintResult.Fail("ZPL is empty");

            try
            {
                var bytes = Encoding.UTF8.GetBytes(zpl);
                string error;
                var ok = RawPrinterHelper.SendBytes(config.SpoolerName, bytes, out error);
                if (!ok) return PrintResult.Fail(error ?? "Unknown spooler error");
                return PrintResult.Ok(bytes.Length);
            }
            catch (Exception ex)
            {
                return PrintResult.Fail("Spooler send error: " + ex.Message);
            }
        }
    }
}
