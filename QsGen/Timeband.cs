using System;
using System.Text.RegularExpressions;
using TlvSerialise;

namespace QsGen
{
    internal class Timeband : TlvSerialisable
    {
        int _startMinutes;
        int _endMinutes;
        public Timeband(int startMinutes, int endMinutes)
        {
            _startMinutes = startMinutes;
            _endMinutes = endMinutes;
            if (_startMinutes >= 1440 || endMinutes >= 1440 || startMinutes < 0 || endMinutes < 0)
            {
                throw new ArgumentOutOfRangeException($"both startMintes and endMinutes need to be less than 1440 and not less than zero");
            }
        }

        public Timeband(string start, string end)
        {
            _startMinutes = ToMinutes(start);
            if (_startMinutes == -1)
            {
                throw new ArgumentOutOfRangeException($"both startMintes and endMinutes need to be less than 1440 and not less than zero");
            }
            _endMinutes = ToMinutes(end);
            if (_startMinutes == -1)
            {
                throw new ArgumentOutOfRangeException($"both startMintes and endMinutes need to be less than 1440 and not less than zero");
            }
        }

        private static int ToMinutes(string s)
        {
            int result = -1;
            if (s != null && Regex.Match(s, @"\d\d:\d\d").Success)
            {
                result = 60 * ((s[0] - '0') * 10 + s[1] - '0') + ((s[3] - '0') * 10) + s[4] - '0';
                if (result < 0 || result >= 1440)
                {
                    result = -1;
                }
            }
            return result;
        }

        [Tlv(TlvTypes.Date, TlvTags.ID_QCKSEL_TIMEBAND_START)]
        public DateTime Start => new DateTime(2018, 01, 01, _startMinutes / 60, _startMinutes % 60, 0);
        [Tlv(TlvTypes.Date, TlvTags.ID_QCKSEL_TIMEBAND_END)]
        public DateTime End => new DateTime(2018, 01, 01, _endMinutes / 60, _endMinutes % 60, 0);
    }
}
