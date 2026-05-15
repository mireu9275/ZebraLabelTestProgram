namespace ZebraLabelPrinter.Core.Printing
{
    public class PrintResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int BytesSent { get; set; }

        public static PrintResult Ok(int bytesSent)
        {
            return new PrintResult { Success = true, BytesSent = bytesSent, Message = "OK" };
        }

        public static PrintResult Fail(string message)
        {
            return new PrintResult { Success = false, Message = message };
        }
    }
}
