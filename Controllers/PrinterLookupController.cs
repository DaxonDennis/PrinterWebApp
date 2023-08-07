using System.Collections.Generic;
using System.Management;
using System.Text.Json;
using System.Web.Mvc;

namespace CheckPrintWebApp.Controllers
{
    public class PrinterLookupController : Controller
    {
        // GET: PrinterLookup
        public class PrinterStatus
        {
            public string Name { get; set; }
            public string Status { get; set; }
        }

        // Enum to represent printer status codes
        private enum PrinterStatusCode
        {
            Other = 1,
            Ready = 3
        }

        public string LookupPrinters()
        {
            var printerStatusList = GetPrinterStatusList();

            // JSON data structure
            var jsonData = new
            {
                Action = "Connection",
                Content = new
                {
                    Printers = printerStatusList
                }
            };

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            // Serialize the data structure to a JSON string
            string jsonString = System.Text.Json.JsonSerializer.Serialize(jsonData, jsonOptions);

            return jsonString;
        }

        public JsonResult GetPrinterStatusList()
        {
            List<PrinterStatus> printerStatusList = new List<PrinterStatus>();

            using (var printerQuery = new ManagementObjectSearcher("SELECT * FROM Win32_Printer"))
            {
                foreach (var printer in printerQuery.Get())
                {
                    var name = printer.GetPropertyValue("Name") as string;
                    var statusCode = (ushort)printer.GetPropertyValue("PrinterStatus");
                    var status = GetPrinterStatusString(statusCode);

                    printerStatusList.Add(new PrinterStatus
                    {
                        Name = name,
                        Status = status
                    });
                }
            }

            return Json(printerStatusList, JsonRequestBehavior.AllowGet);
        }

        private static string GetPrinterStatusString(uint statusCode)
        {
            string statusString;

            // Using the enum for switch case
            switch ((PrinterStatusCode)statusCode)
            {
                case PrinterStatusCode.Other:
                    statusString = "Paused or Out of Paper";
                    break;

                case PrinterStatusCode.Ready:
                    statusString = "Ready";
                    break;

                default:
                    statusString = "Error";
                    break;
            }

            return statusString;
        }

        public static bool IsPrinterReady(string printerName)
        {
            // Use string interpolation to construct the query
            string query = $"SELECT * FROM Win32_Printer WHERE Name = '{printerName.Replace("\\", "\\\\")}'";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject printer in searcher.Get())
                {
                    var statusCode = (ushort)printer.GetPropertyValue("PrinterStatus");
                    // Compare the status code with the enum
                    if (statusCode == (ushort)PrinterStatusCode.Ready)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}