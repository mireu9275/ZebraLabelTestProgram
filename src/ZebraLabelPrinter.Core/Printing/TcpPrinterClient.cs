using System;
using System.Net.Sockets;
using System.Text;
using ZebraLabelPrinter.Core.Models;

namespace ZebraLabelPrinter.Core.Printing
{
    public class TcpPrinterClient : IPrinterClient
    {
        public PrintResult Send(PrinterConfig config, string zpl)
        {
            if (config == null) return PrintResult.Fail("PrinterConfig is null");
            if (string.IsNullOrEmpty(config.IpAddress)) return PrintResult.Fail("IP address is empty");
            if (string.IsNullOrEmpty(zpl)) return PrintResult.Fail("ZPL is empty");

            try
            {
                using (var client = new TcpClient())
                {
                    client.SendTimeout = config.SendTimeoutMs;
                    client.ReceiveTimeout = config.ReceiveTimeoutMs;

                    var connect = client.BeginConnect(config.IpAddress, config.Port, null, null);
                    var connected = connect.AsyncWaitHandle.WaitOne(config.SendTimeoutMs);
                    if (!connected)
                    {
                        return PrintResult.Fail("Connect timeout: " + config.IpAddress + ":" + config.Port);
                    }
                    client.EndConnect(connect);

                    using (var stream = client.GetStream())
                    {
                        var bytes = Encoding.UTF8.GetBytes(zpl);
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Flush();
                        return PrintResult.Ok(bytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                return PrintResult.Fail("TCP send error: " + ex.Message);
            }
        }
    }
}
