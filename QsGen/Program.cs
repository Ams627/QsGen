using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
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

        private static int GetLineNumber(XElement element)
        {
            var info = element as IXmlLineInfo;
            int lineNumber = info?.LineNumber ?? 0;
            return lineNumber;
        }

        private static int CheckElements<T>(IEnumerable<T> source, Func<T, bool> predicate, string message, bool addLinenumber = true)
        {
            source.Where(predicate).ToList().ForEach(y => Console.Error.WriteLine($"at line {(addLinenumber ? GetLineNumber(y as XElement) : 0)}: {message}"));
            return 0;
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
                    var qsxml = XDocument.Load(args[0], LoadOptions.SetLineInfo);

                    var qElements = qsxml.Descendants("q");
                    CheckElements(qElements, x => x.Attribute("d") == null, "q element contains no d (Destination) attribute.");
                    CheckElements(qElements, x => x.Attribute("r") == null, "element has no r (Route) attribute.");
                    CheckElements(qElements, x => x.Attribute("u") == null, "element has no u (Until(end date)) attribute.");
                    CheckElements(qElements, x => x.Attribute("f") == null, "element has no f (From (start date)) attribute.");
                    CheckElements(qElements, x => x.Attribute("t") == null, "element has no t (Ticket type) attribute.");
                    CheckElements(qElements, x => x.Attribute("fare") == null, "element has no fare attribute.");
                    CheckElements(qElements, x => x.Attribute("res") == null, "element has no t (Ticket type) attribute.");
                    CheckElements(qElements, x => x.Attribute("orient") == null, "element has no res (Restriction) attribute.");
                    CheckElements(qElements, x => x.Attribute("dband") == null, "element has no dband (Date band) attribute.");
                    CheckElements(qElements, x => x.Attribute("tband") == null, "element has no tband (Time band) attribute.");

                    qElements.Where(x => DateTime.TryParseExact(x.Attribute("f").Value, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out var _) != true).ToList().ForEach(y => Console.Error.WriteLine($"f attribute at line {GetLineNumber(y)} has an invalid date (should be yyyy-mm-dd)."));
                    qElements.Where(x => DateTime.TryParseExact(x.Attribute("u").Value, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out var _) != true).ToList().ForEach(y => Console.Error.WriteLine($"u attribute at line {GetLineNumber(y)} has an invalid date (should be yyyy-mm-dd)."));

                    qElements.Where(x => !Regex.Match(x.Attribute("d").Value, "^[A-Z0-9][A-Z0-9][0-9][0-9]$").Success).ToList().ForEach(y => Console.Error.WriteLine($"d element at line {GetLineNumber(y)} has an invalid NLC."));
                    qElements.Where(x => !Regex.Match(x.Attribute("r").Value, "^[0-9]{5}$").Success).ToList().ForEach(y => Console.Error.WriteLine($"r element at line {GetLineNumber(y)} has an invalid route code."));
                    qElements.Where(x => !Regex.Match(x.Attribute("t").Value, "^[0-9A-Z]{3}$").Success).ToList().ForEach(y => Console.Error.WriteLine($"t element at line {GetLineNumber(y)} has an invalid ticket code."));
                    qElements.Where(x => !Regex.Match(x.Attribute("fare").Value, "^[0-9]{1,10}$").Success).ToList().ForEach(y => Console.Error.WriteLine($"fare element at line {GetLineNumber(y)} has an invalid fare."));
                    qElements.Where(x => !Regex.Match(x.Attribute("res").Value, "^[0-9A-Z]{2}$").Success).ToList().ForEach(y => Console.Error.WriteLine($"t element at line {GetLineNumber(y)} has an invalid restriction code."));
                    qElements.Where(x => !Regex.Match(x.Attribute("cli").Value, "^[0-9]$").Success).ToList().ForEach(y => Console.Error.WriteLine($"cli element at line {GetLineNumber(y)} is invalid."));
                    qElements.Where(x => !Regex.Match(x.Attribute("fl").Value, "^[0-9]$").Success).ToList().ForEach(y => Console.Error.WriteLine($"fl element at line {GetLineNumber(y)} is invalid."));
                    qElements.Where(x => !Regex.Match(x.Attribute("orient").Value, "^[0-9]$").Success).ToList().ForEach(y => Console.Error.WriteLine($"orient element at line {GetLineNumber(y)} is invalid."));

                    var qs = qsxml.Element("ParkeonQS").Elements("station").Select(x => new
                    {
                        Origin = x.Attribute("nlc")?.Value ?? throw new Exception("error"),
                        QSList = x.Elements("q").Select(y => new QuickSelect
                        {
                            Origin = x.Attribute("nlc")?.Value ?? throw new Exception("Invalid NLC for origin"),
                            Destination = y.Attribute("d")?.Value ?? throw new Exception("error"),
                            Route = y.Attribute("r")?.Value ?? throw new Exception("error"),
                            EndDate = GetDate(y.Attribute("u")?.Value),
                            StartDate = GetDate(y.Attribute("f")?.Value),
                            Ticket = y.Attribute("t")?.Value ?? throw new Exception("Invalid Ticket Code"),
                            AdultFare = Convert.ToInt32(y.Attribute("fare")?.Value ?? throw new Exception("Invalid Fare")),
                            Restriction = y.Attribute("res")?.Value ?? throw new Exception("Invalid Restriction"),
                            CrossLondonInd = Convert.ToInt32(y.Attribute("cli")?.Value ?? throw new Exception("Invalid Cross London Indicator")),
                            Flag = Convert.ToInt32(y.Attribute("fl")?.Value ?? throw new Exception("Invalid Flag")),
                            Orientation = Convert.ToInt32(y.Attribute("orient")?.Value ?? throw new Exception("Invalid Orientation")),
                            DatebandName = y.Attribute("dband")?.Value ?? throw new Exception("Invalid date band"),
                            TimebandName = y.Attribute("dband")?.Value ?? throw new Exception("Invalid time band"),
                            Status="000"
                        }).ToList()
                    }).ToDictionary(y => y.Origin, y => y.QSList);

                    var qsfile = new QSFile { Version = 90, TVMId = "TVM50", QuickSelects = qs.First().Value };
                    using (var fs = new FileStream(@"q:\temp\QUICK_SE", FileMode.Create, FileAccess.Write))
                    {
                        byte[] header = new UTF8Encoding(true).GetBytes("TLtV0100");
                        fs.Write(header, 0, header.Length);
                        qsfile.Serialise(fs);
                    }
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
