using System.Collections.Generic;
using TlvSerialise;
namespace QsGen
{
    internal class PopularList : TlvSerialisable
    {
        [Tlv(TlvTypes.UInt, TlvTags.ID_POP_NUMERO_VERSION)]
        public int Version { get; set; }

        [Tlv(TlvTypes.String, TlvTags.ID_POP_IAP)]
        public string TVMId { get; set; }

        [Tlv(TlvTypes.Array, TlvTags.ID_POP_POPULAR)]
        public List<PopularDestination> PopularTickets { get; set;}
    }
}
