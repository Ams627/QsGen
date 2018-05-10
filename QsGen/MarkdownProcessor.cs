using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace QsGen
{
    internal class QSNlcSection
    {
        string nlc;
        private List<string> tvms;
        public QSNlcSection(string nlc, List<string> tvms)
        {
            this.nlc = nlc;
            this.tvms = tvms;
        }
        public QSNlcSection MakeCopy()
        {
            var qs = new QSNlcSection(this.nlc, this.tvms);
            return qs;
        }
    }
    internal class MarkdownProcessor
    {
        private string filename;
        List<QSNlcSection> qsNlcs = new List<QSNlcSection>();
        enum State { Init, Populars, Quicks, Bands};
        public MarkdownProcessor(string filename)
        {
            this.filename = filename;
            bool first = false;
            

            QSNlcSection qsNlc = null;
            var recovery = false;
            State state = State.Init;

            var lineStack = new Stack<string>();
            foreach (var line in File.ReadAllLines(filename))
            {
                var trimmedLine = line.Trim();
                if (line.StartsWith(".nlc"))
                {
                    var split = line.Split(' ');
                    if (split.Length < 2)
                    {
                        Console.Error.WriteLine(".nlc must be followed by at least an NLC and then optionally one or more TVM ids.");
                        recovery = true;
                    }
                    if (qsNlc != null)
                    {
                        // add the previous NLC section to this list and create a new one:
                        qsNlcs.Add(qsNlc);
                        var nlc = split[0];
                        qsNlc = new QSNlcSection(nlc, split.Skip(1).ToList());
                    }
                }
                else if (qsNlc != null && trimmedLine.StartsWith(".pops"))
                {
                    state = State.Populars;
                    ProcessPopulars(line);
                }
                else if (qsNlc != null && trimmedLine.StartsWith(".quicks"))
                {
                    state = State.Quicks;
                }
                else if (qsNlc != null && trimmedLine.StartsWith(".bands"))
                {
                    state = State.Bands;
                }
                else if (state == State.Populars)
                {
                    ProcessPopulars(line);
                }
                else if (state == State.Quicks)
                {
                    var quick = ProcessQuickSelect(line);
                }
                else if (state == State.Bands)
                {
                    var quick = ProcessBand(line);
                }

            }
        }

        private object ProcessQuickSelect(string line)
        {
            throw new NotImplementedException();
        }

        private void ProcessPopulars(string line)
        {
            throw new NotImplementedException();
        }
    }
}