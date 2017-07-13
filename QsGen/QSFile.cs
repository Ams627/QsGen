using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TlvSerialise;

namespace QsGen
{
    class QSSection : TlvSerialisable
    {
        [Tlv(TlvTypes.UInt, TlvTags.ID_QCKSEL_NUMERO_VERSION)]
        public int Version { get; set; }

        [Tlv(TlvTypes.String, TlvTags.ID_QCKSEL_IAP)]
        public string TVMId { get; set; }

        [Tlv(TlvTypes.Array, TlvTags.ID_QCKSEL_PRODUCT)]
        public List<QuickSelect> QuickSelects { get; set; }
    }
}
