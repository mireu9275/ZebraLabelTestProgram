namespace ZebraLabelPrinter.Core.Models
{
    public enum PrinterConnectionType
    {
        Tcp = 0,
        WindowsSpooler = 1
    }

    public class PrinterConfig
    {
        public string PrinterCode { get; set; }
        public string DisplayName { get; set; }

        public PrinterConnectionType ConnectionType { get; set; } = PrinterConnectionType.Tcp;

        public string IpAddress { get; set; }
        public int Port { get; set; } = 9100;

        public string SpoolerName { get; set; }

        public int TargetDpi { get; set; } = 203;

        public int SendTimeoutMs { get; set; } = 5000;
        public int ReceiveTimeoutMs { get; set; } = 5000;
    }
}
