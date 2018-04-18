using System;
using System.Collections.Generic;
using System.Globalization;
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

            return tempDateTime;
        }

        static DateTime GetDateTime(string str)
        {
            DateTime result = DateTime.MinValue;
            if (!string.IsNullOrEmpty(str) && Regex.Match(str, "^[0-9][0-9]?:[0-9][0-9](:[0-9][0-9])?$").Success)
            {
                var hms = str.Split(':');
                var h = Convert.ToInt32(hms[0]);
                var m = Convert.ToInt32(hms[1]);
                var s = (hms.Length == 3) ? Convert.ToInt32(hms[2]) : 0;
                result = new DateTime(1970, 1, 1) + TimeSpan.FromSeconds(3600 * h + 60 * m + s);
            }
            return result;
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

        private static void CheckQElements(IEnumerable<XElement> elements)
        {
            CheckElements(elements, x => x.Attribute("d") == null, "q element contains no d (Destination) attribute.");
            CheckElements(elements, x => x.Attribute("r") == null, "element has no r (Route) attribute.");
            CheckElements(elements, x => x.Attribute("u") == null, "element has no u (Until(end date)) attribute.");
            CheckElements(elements, x => x.Attribute("f") == null, "element has no f (From (start date)) attribute.");
            CheckElements(elements, x => x.Attribute("t") == null, "element has no t (Ticket type) attribute.");
            CheckElements(elements, x => x.Attribute("fare") == null, "element has no fare attribute.");
            CheckElements(elements, x => x.Attribute("res") == null, "element has no t (Ticket type) attribute.");
            CheckElements(elements, x => x.Attribute("orient") == null, "element has no res (Restriction) attribute.");
            CheckElements(elements, x => x.Attribute("dband") == null, "element has no dband (Date band) attribute.");
            CheckElements(elements, x => x.Attribute("tband") == null, "element has no tband (Time band) attribute.");
            CheckElements(elements, x => GetDate(x.Attribute("f")?.Value) == null, "f attribute represents an invalid date (should be yyyy-mm-dd).");
            CheckElements(elements, x => GetDate(x.Attribute("u")?.Value) == null, "u attribute represents an invalid date (should be yyyy-mm-dd).");

            CheckElements(elements, x => !Regex.Match(x.Attribute("d").Value, "^[A-Z0-9][A-Z0-9][0-9][0-9]$").Success, "attribute at line has an invalid NLC.");
            CheckElements(elements, x => !Regex.Match(x.Attribute("r").Value, "^[0-9]{5}$").Success, $"r attribute has an invalid route code.");
            CheckElements(elements, x => !Regex.Match(x.Attribute("t").Value, "^[0-9A-Z]{3}$").Success, "t attribute has an invalid ticket code.");
            CheckElements(elements, x => !Regex.Match(x.Attribute("fare").Value, "^[0-9]{1,10}$").Success, $"fare attribute has an invalid fare.");
            CheckElements(elements, x => !Regex.Match(x.Attribute("res").Value, "^[0-9A-Z ]{2}$").Success, $"res attribute has an invalid restriction code.");
            CheckElements(elements, x => !Regex.Match(x.Attribute("cli").Value, "^[0123]$").Success, $"cli (Cross London Indicator) attribute is invalid.");
            CheckElements(elements, x => !Regex.Match(x.Attribute("fl").Value, "^[0-9]$").Success, $"flag attribute is invalid.");
            CheckElements(elements, x => !Regex.Match(x.Attribute("orient").Value, "^[0-9]$").Success, $"orient attribute at line is invalid.");
        }

        private static void CheckTimebands(IEnumerable<XElement> elements)
        {
            CheckElements(elements, x => !Regex.Match(x.Attribute("start").Value, "^[0-9][0-9]?:[0-9][0-9](:[0-9][0-9])?$").Success, $"start attribute is invalid (should be hh:mm or hh:mm:ss)");
            CheckElements(elements, x => !Regex.Match(x.Attribute("end").Value, "^[0-9][0-9]?:[0-9][0-9](:[0-9][0-9])?$").Success, $"end attribute is invalid (should be hh:mm or hh:mm:ss)");
        }

        static DateTime ParseAttributeDate(string s)
        {
            if (s != null)
            {
                DateTime.TryParseExact(s, "yyyyMMdd",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out var date);
                return date;
            }
            return DateTime.MaxValue;
        }
        private static void Main(string[] args)
        {
            try
            {
                if (args.Count() == 0)
                {
                    var codeBase = System.Reflection.Assembly.GetEntryAssembly().CodeBase;
                    var progname = Path.GetFileNameWithoutExtension(codeBase);
                    Console.Error.WriteLine($"Usage:\n\n    {progname} qs.xml\n\nwhere qs.xml is an xml file representing quick selects for the whole network.\n");
                    Console.Error.WriteLine($"Use \n\n {progname} -sample\n\nto generate a sample xml file.");
                }
                else if (args[0] == "-sample")
                {
                    CreateSampleAndPrint();
                }
                else
                {
                    var version = args.Length > 1 ? args[1] : "40";
                    var qsxml = XDocument.Load(args[0], LoadOptions.SetLineInfo);

                    //var qElements = qsxml.Descendants("q");
                    //CheckQElements(qElements);

                    var qs = qsxml.Element("ProductDefinition").Elements("Products").Where(v=>v.Attribute("Version").Value == version). Select(x => new
                    {
                        Nlc = x.Attribute("Nlc").Value,
                        TvmId = x.Attribute("TvmId").Value,
                        Version = x.Attribute("Version").Value,
                        QuickSelects = x.Elements("Product").Where(y => y.Attribute("Type").Value == "QuickSelect").Select(z => new
                        {
                            Code = z.Attribute("Code").Value,
                            Origin = z.Attribute("Origin").Value,
                            Destination = z.Attribute("Destination").Value,
                            Route = z.Attribute("Route").Value,
                            TicketCode = z.Attribute("TicketCode").Value,
                            Restriction = z.Attribute("Restriction").Value,
                            Orientation = z.Attribute("Orientation").Value,
                            Timeband = z.Attribute("Timeband").Value,
                            Dayband = z.Attribute("Dayband").Value,
                        }),
                        Populars = x.Elements("Products").Where(y => y.Attribute("Type").Value == "Popular").Select(z => z.Attribute("Destination").Value).ToList()
                    });

                    var timebands = qsxml.Element("ProductDefinition").Element("TimeAndDateValidity")
                        .Elements("Timebands")
                        .Where(v => v.Attribute("Version").Value == version)
                        .SelectMany(tbs=>tbs.Elements("Timeband")
                        .Select(tb=> new
                        {
                            Version = tb.Attribute("Version").Value,
                            Timebands = tb.Elements("Timeband").Select(timeband => new
                            {
                                End = tb.Attribute("End")?.Value ?? "wonk",
                                Start = tb.Attribute("Start")?.Value ?? "plonk",
                                Id = tb.Attribute("Id")?.Value ?? "hello",
                                //End = x.Attribute("End")?.Value ?? throw new Exception("End time invalid"),
                                //Start = x.Attribute("Start")?.Value ?? throw new Exception("Start time invalid"),
                                //Id = x.Attribute("Id")?.Value ?? throw new Exception("Start time invalid"),
                            }
                        }
                    });

                    var daybands = qsxml.Element("ProductDefinition").Element("TimeAndDateValidity").Elements("Daybands").Where(v => v.Attribute("Version").Value == version).Select(x => new
                    {
                        Version = x.Attribute("Version").Value,
                        Timebands = x.Elements("Dayband").Select(timeband => new
                        {
                            End = x.Attribute("End")?.Value ?? throw new Exception("End time invalid"),
                            Start = x.Attribute("Start")?.Value ?? throw new Exception("Start time invalid"),
                            Id = x.Attribute("Id")?.Value ?? throw new Exception("Start time invalid"),
                        })
                    });

                    Console.WriteLine();

                    //    Elements("station").Select(x => new
                    //{
                    //    Nlc = x.Attribute("nlc")?.Value ?? throw new Exception("missing nlc"),
                    //    TvmId = x.Attribute("tvmid")?.Value ?? throw new Exception("missing tvmid"),
                    //    QSList = x.Elements("q").Select((y, index) => new QuickSelect
                    //    {
                    //        Code = index + 1,
                    //        Origin = y.Attribute("o")?.Value ?? x.Attribute("nlc")?.Value, 
                    //        Destination = y.Attribute("d")?.Value, 
                    //        Route = y.Attribute("r")?.Value,
                    //        EndDate = GetDate(y.Attribute("u")?.Value),
                    //        StartDate = GetDate(y.Attribute("f")?.Value),
                    //        Ticket = y.Attribute("t")?.Value,
                    //        AdultFare = Convert.ToInt32(y.Attribute("fare")?.Value),
                    //        Restriction = y.Attribute("res")?.Value,
                    //        CrossLondonInd = Convert.ToInt32(y.Attribute("cli")?.Value),
                    //        Flag = Convert.ToInt32(y.Attribute("fl")?.Value),
                    //        Orientation = Convert.ToInt32(y.Attribute("orient")?.Value),
                    //        DatebandName = y.Attribute("dband")?.Value,
                    //        TimebandName = y.Attribute("tband")?.Value,
                    //        Status="000"
                    //    }).ToList(),
                    //}).ToDictionary(y => y.Nlc + '-' + y.TvmId, y => y.QSList);

                    var stimebands = qsxml.Element("ParkeonQS").Element("timebands")?.Elements("timeband");

                    //var timebands = qsxml.Element("ParkeonQS").Element("timebands")
                    //    .Elements("timeband")
                    //    .Select(x => new TimeBandGroup {
                    //        TimebandGroupName = x.Attribute("name")?.Value,
                    //        TimebandList = (x.Elements("t")
                    //            .Select(y => new Timeband
                    //            {
                    //                Start = GetDateTime(y.Attribute("start")?.Value),
                    //                End = GetDateTime(y.Attribute("end")?.Value),
                    //            })).ToList()
                    //    }
                    //    ).ToList();


                    // MakeDoc(qsxml.Elements());

                    // same timeband list for all TVMs
                    // var timebandSection = new TimebandSection { TBGroupList = timebands };

                    //foreach (var stationKey in qs.Keys)
                    //{
                    //    var qsSection = new QSSection { Version = 90, TVMId = "TVM50", QuickSelects = qs.First().Value };

                    //    var filename = "QUICK_SE." + stationKey;
                    //    using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                    //    {
                    //        byte[] header = new UTF8Encoding(true).GetBytes("TLtV0100");
                    //        fs.Write(header, 0, header.Length);
                    //        qsSection.Serialise(fs);
                    //        timebandSection.Serialise(fs);
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                var codeBase = System.Reflection.Assembly.GetEntryAssembly().CodeBase;
                var progname = Path.GetFileNameWithoutExtension(codeBase);
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
                Console.WriteLine();
            }

        }

        private static int ToMinutesOrNull(XAttribute xAttribute)
        {
            int result = -1;
            var value = xAttribute?.Value;
            if (value != null && Regex.Match(value, @"\d\d:\d\d").Success)
            {
                result = 60 * ((value[0] * 10) - '0' + value[1] - '0') + 60 * (value[3] - '0') + value[4] - '0';
            }
            return result;
        }

        class Indent
        {
            public int n = 0;
        }

        //private static void MakeDoc(IEnumerable<XElement> elements, Indent n = null)
        //{
        //    var indent = n?.n ?? 0;
        //    if (indent == 0)
        //    {
        //        Console.WriteLine("new XDocument(");
        //    }
        //    foreach (var element in elements)
        //    {
        //        Console.Write($"new XElement(\"{element.Name}\"");
        //        var atts = element.Attributes().ToList().Aggregate("", (current, next)=>current + ", " + $"    new XAttribute(\"{next.Name}\", \"{next.Value}\")");
        //        Console.WriteLine(atts);
        //        if (element.IsEmpty)
        //        {
        //            Console.WriteLine($"),");
        //        }
        //        else
        //        {
        //            Console.Write(", ");
        //            indent += 4;
        //            MakeDoc(element.Elements(), new Indent { n = indent });
        //        }
        //    }
        //    Console.WriteLine(")");
        //    indent -= 4;
        //}

        private static void CreateSampleAndPrint()
        {
            var qslist = new List<QuickSelect>
            {
                new QuickSelect { Code = 2971, EndDate = new DateTime(2999, 12, 31), StartDate = new DateTime(2014, 1, 2), Route = "00000", Origin="8048", Destination = "8126", Ticket = "SDS", Restriction = "  ", AdultFare = 660, CrossLondonInd = 0, Orientation = 0, DatebandName = "YYYYYNN", TimebandName="10 Peak" },
                new QuickSelect { Code = 2972, EndDate = new DateTime(2999, 12, 31), StartDate = new DateTime(2014, 1, 2), Route = "00000", Origin="8048", Destination = "8126", Ticket = "SDR", Restriction = "  ", AdultFare = 780, CrossLondonInd = 0, Orientation = 1, DatebandName = "YYYYYNN", TimebandName="10 Peak" },
            };

            //var doc = new XDocument(
            //    new XElement(
            //        "ParkeonQS", new XElement("stations", new XElement("station", 
            //    );

        }
    }
}
