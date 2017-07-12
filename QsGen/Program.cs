using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TlvSerialise;
namespace QsGen
{
    internal class Program
    {
        static DateTime GetDate(string s)
        {
            if (s == null)
            {
                throw new Exception("date attribute expected");
            }
            bool res = DateTime.TryParseExact(s, "yyyy-MM-dd",
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.None, out var tempDateTime);

            if (!res)
            {
                throw new Exception("invalid date");
            }
            return tempDateTime;

        }
        private static void Main(string[] args)
        {
            try
            {
                if (args.Count() == 0)
                {

                }
                else if (args[0] == "-sample")
                {

                }
                else
                {
                    var qsxml = XDocument.Load(args[0]);
                    var qs = qsxml.Element("ParkeonQS").Elements("station").Select(x => new
                    {
                        Origin = x.Attribute("nlc")?.Value ?? throw new Exception("error"),
                        QSList = x.Elements("q").Select(y => new QuickSelect
                        {
                            Destination = y.Attribute("d")?.Value ?? throw new Exception("error"),
                            Route = y.Attribute("r")?.Value ?? throw new Exception("error"),
                            EndDate = GetDate(y.Attribute("u")?.Value),
                            StartDate = GetDate(y.Attribute("f")?.Value),
                            Ticket = y.Attribute("t")?.Value ?? throw new Exception("Invalid Ticket Code"),
                            AdultFare = Convert.ToInt32(y.Attribute("af")?.Value ?? throw new Exception("Invalid Ticket Code")),
                            CrossLondonInd = Convert.ToInt32(y.Attribute("cli")?.Value ?? throw new Exception("Invalid Cross London Indicator")),
                            Flag = Convert.ToInt32(y.Attribute("fl")?.Value ?? throw new Exception("Invalid Flag")),
                            Orientation = Convert.ToInt32(y.Attribute("orient")?.Value ?? throw new Exception("Invalid Orientation")),
                            DatebandName = y.Attribute("dband")?.Value ?? throw new Exception("Invalid date band"),
                            TimebandName = y.Attribute("dband")?.Value ?? throw new Exception("Invalid time band"),
                    }).ToList()
                    }).ToDictionary(y => y.Origin, y => y.QSList);
                }
            }
            catch (Exception ex)
            {
                var codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                var progname = Path.GetFileNameWithoutExtension(codeBase);
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
            }

        }
    }
}
