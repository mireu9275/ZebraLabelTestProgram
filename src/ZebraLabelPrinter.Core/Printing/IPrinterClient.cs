using ZebraLabelPrinter.Core.Models;

namespace ZebraLabelPrinter.Core.Printing
{
    public interface IPrinterClient
    {
        PrintResult Send(PrinterConfig config, string zpl);
    }
}
