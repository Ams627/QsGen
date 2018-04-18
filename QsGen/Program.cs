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
                    var qsxml = XDocument.Load(args[0], LoadOptions.SetLineInfo);

                    // read all Products (quick selects and popular destinations):
                    var qs = qsxml.Element("ProductDefinition").Elements("Products")
                        .Select(x => new
                        {
                            Nlc = x.Attribute("Nlc").Value,
                            TvmId = x.Attribute("TvmId").Value,
                            Version = x.Attribute("Version").Value,
                            QuickSelects = x.Elements("Product").Where(y => y.Attribute("Type").Value == "QuickSelect")
                            .Select(z => new QuickSelect
                            (
                                code: z.Attribute("Code")?.Value,
                                origin: z.Attribute("Origin")?.Value,
                                destination: z.Attribute("Destination")?.Value,
                                route: z.Attribute("Route")?.Value,
                                ticket: z.Attribute("TicketCode")?.Value,
                                restriction: z.Attribute("Restriction")?.Value,
                                orientation: z.Attribute("Orientation")?.Value,
                                timeband: z.Attribute("Timeband")?.Value,
                                dateband: z.Attribute("Dayband")?.Value
                            )),

                            Populars = x.Elements("Products")
                                .Where(y => y.Attribute("Type")?.Value == "Popular")
                                .Select(z => z.Attribute("Destination")?.Value).ToList()
                        });

                    // read all timebands:
                    var timebands = qsxml.Element("ProductDefinition").Element("TimeAndDateValidity")
                        .Elements("Timebands")
                        .Select(tbs => new
                        {
                            Version = tbs.Attribute("Version").Value,
                            Timebands = tbs.Elements("Timeband")
                                .Select(band => new
                                        {
                                           End = band.Attribute("End").Value,
                                           Start = band.Attribute("Start").Value,
                                           Id = band.Attribute("Id").Value
                                        }).ToLookup(x=>x.Id, x=> new Timeband(start: x.Start, end: x.End))
                        }).ToLookup(y=>y.Version, y=>y.Timebands);

                    // read all daybands:
                    var daybands = qsxml.Element("ProductDefinition").Element("TimeAndDateValidity")
                        .Elements("Daybands")
                        .Select(dbs => new
                        {
                            Version = dbs.Attribute("Version").Value,
                            Daybands = dbs.Elements("Dayband").Select(db => new
                            {
                                Valid = db.Attribute("Valid")?.Value ?? throw new Exception("End time invalid"),
                                Id = db.Attribute("Id")?.Value ?? throw new Exception("Start time invalid"),
                            }).ToLookup(x => x.Id, x => x.Valid)
                        }).ToLookup(x=>x.Version, x=>x.Daybands);

                    // check for duplicates in timebands:
                    var versionDups = timebands.Where(x => x.Count() > 1);
                    foreach (var v in versionDups)
                    {
                        Console.Error.WriteLine($"WARNING: version {v.Key} occcurs more than once in Timebands list. Will used first entry in file.");
                    }

                    foreach (var tb in timebands)
                    {
                        var cnt = tb.Count();
                        var tbDups = tb.First().Where(x => x.Count() > 1);
                        foreach (var item in tbDups)
                        {
                            Console.WriteLine($"WARNING timeband name '{item.Key}' occurs more than once for version {tb.Key}");
                        }
                    }

                    // check for duplicates in daybands:
                    var dbVersionDups = daybands.Where(x => x.Count() > 1);
                    foreach (var v in dbVersionDups)
                    {
                        Console.Error.WriteLine($"WARNING: version {v.Key} occcurs more than once in Daybands list.");
                    }

                    foreach (var db in daybands)
                    {
                        var dbDups = db.First().Where(x => x.Count() > 1);
                        foreach (var dbdup in dbDups)
                        {
                            Console.Error.WriteLine($"WARNING: dayband {dbdup} used more than once in version {db.Key}.");
                        }
                    }

                    var qlookup = qs.ToLookup(x => x.Nlc, x=>(x.Version, x.TvmId, x.QuickSelects));

                    Parallel.ForEach(qlookup, qsfile =>
                    {
                        var dirname = qsfile.Key;
                        Directory.CreateDirectory(dirname);
                        var pathname = Path.Combine(dirname, "QUICK_SE");
                        var firstTvm = qsfile.First();
                        var qsSection = new QSSection
                        {
                            Version = Convert.ToInt32(qsfile.First().Version),
                            TVMId = firstTvm.TvmId,
                            QuickSelects = qsfile.First().QuickSelects.ToList()
                        };
                        var usedTimebands = qsSection.QuickSelects.Select(x => x.TimebandName);

                        var timebandsForVersion = timebands[firstTvm.Version];

                        var timebandSection = new TimebandSection
                        {
                            TBGroupList = (from bandlist in timebandsForVersion
                                           from band in bandlist
                                           select new TimeBandGroup
                                           {
                                               TimebandGroupName = band.Key,
                                               TimebandList = band.ToList()
                                           }).ToList()
                        };

                        using (var fs = new FileStream(pathname, FileMode.Create, FileAccess.Write))
                        {
                            byte[] header = new UTF8Encoding(true).GetBytes("TLtV0100");
                            fs.Write(header, 0, header.Length);
                            qsSection.Serialise(fs);
                            timebandSection.Serialise(fs);
                        }
                    });
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

                
        private static void CreateSampleAndPrint()
        {
            //var qslist = new List<QuickSelect>
            //{
            //    new QuickSelect { Code = 2971, EndDate = new DateTime(2999, 12, 31), StartDate = new DateTime(2014, 1, 2), Route = "00000", Origin="8048", Destination = "8126", Ticket = "SDS", Restriction = "  ", AdultFare = 660, CrossLondonInd = 0, Orientation = 0, DatebandName = "YYYYYNN", TimebandName="10 Peak" },
            //    new QuickSelect { Code = 2972, EndDate = new DateTime(2999, 12, 31), StartDate = new DateTime(2014, 1, 2), Route = "00000", Origin="8048", Destination = "8126", Ticket = "SDR", Restriction = "  ", AdultFare = 780, CrossLondonInd = 0, Orientation = 1, DatebandName = "YYYYYNN", TimebandName="10 Peak" },
            //};

            //var doc = new XDocument(
            //    new XElement(
            //        "ParkeonQS", new XElement("stations", new XElement("station", 
            //    );

        }
    }
}
