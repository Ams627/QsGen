using TlvSerialise;
namespace QsGen
{
    internal class PopularDestination : TlvSerialisable
    {
        [Tlv(TlvTypes.String, TlvTags.ID_POP_CODE)]
        public string Nlc { get; set; }
    }
}