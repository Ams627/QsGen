using System;
using TlvSerialise;

namespace QsGen
{
    class Timeband : TlvSerialisable
    {
        [Tlv(TlvTypes.Date, TlvTags.ID_QCKSEL_TIMEBAND_START)]
        public DateTime Start { get; set; }
        [Tlv(TlvTypes.Date, TlvTags.ID_QCKSEL_TIMEBAND_END)]
        public DateTime End { get; set; }
    }
}
