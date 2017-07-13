using System.Collections.Generic;
using TlvSerialise;

namespace QsGen
{
    class TimebandSection : TlvSerialisable
    {
        [Tlv(TlvTypes.Array, TlvTags.ID_QCKSEL_TIMEBAND_TABLE)]
        public List<TimeBandGroup> TBGroupList { get; set; }
    }
}
