using System.Collections.Generic;
using TlvSerialise;

namespace QsGen
{
    class TimeBandGroup : TlvSerialisable
    {
        [Tlv(TlvTypes.String, TlvTags.ID_QCKSEL_TIMEBAND_NAME)]
        public string TimebandGroupName { get; set; }

        [Tlv(TlvTypes.Array, TlvTags.ID_QCKSEL_TIMEBAND_ARRAY)]
        public List<Timeband> TimebandList { get; set; }
    }
}
