using System;
using TlvSerialise;
namespace QsGen
{
    class QuickSelect : TlvSerialisable
    {
        [Tlv(TlvTypes.UInt, TlvTags.ID_QCKSEL_CODE)] public int Code { get; set; }

        [Tlv(TlvTypes.Date, TlvTags.ID_QCKSEL_END_DATE), TlvEndDate]
        public DateTime EndDate { get; set; }


        [Tlv(TlvTypes.Date, TlvTags.ID_QCKSEL_START_DATE)]
        public DateTime StartDate { get; set; }


        [Tlv(TlvTypes.String, TlvTags.ID_QCKSEL_ORIGIN)]
        public string Origin { get; set; }


        [Tlv(TlvTypes.String, TlvTags.ID_QCKSEL_DESTINATION)]
        public string Destination { get; set; }


        [Tlv(TlvTypes.String, TlvTags.ID_QCKSEL_ROUTE)]
        public string Route { get; set; }


        [Tlv(TlvTypes.String, TlvTags.ID_QCKSEL_TICKET)]
        public string Ticket { get; set; }


        [Tlv(TlvTypes.String, TlvTags.ID_QCKSEL_RESTRICTION)]
        public string Restriction { get; set; }


        [Tlv(TlvTypes.UInt, TlvTags.ID_QCKSEL_ADULT_FARE)]
        public int AdultFare { get; set; }


        [Tlv(TlvTypes.UInt, TlvTags.ID_QCKSEL_CROSS_LONDON_IND)]
        public int CrossLondonInd { get; set; }


        [Tlv(TlvTypes.UInt, TlvTags.ID_QCKSEL_FLAG)]
        public int Flag { get; set; }


        [Tlv(TlvTypes.String, TlvTags.ID_QCKSEL_STATUS)]
        public string Status { get; set; }


        [Tlv(TlvTypes.UInt, TlvTags.ID_QCKSEL_ORIENTATION)]
        public int Orientation { get; set; }


        [Tlv(TlvTypes.String, TlvTags.ID_QCKSEL_DATEBAND_NAME)]
        public string DatebandName { get; set; }


        [Tlv(TlvTypes.String, TlvTags.ID_QCKSEL_TIMEBAND_NAME)]
        public string TimebandName { get; set; }

        public QuickSelect(
            string code,
            string origin,
            string destination,
            string route,
            string ticket,
            string restriction,
            string orientation,
            string timeband,
            string dateband
            )
        {
            StartDate = new DateTime(2018, 02, 16);
            EndDate = new DateTime(2166, 05, 18);
            CrossLondonInd = 0;
            Status = "000";
            Flag = 31415;
            AdultFare = 123456;
            Code = Convert.ToInt32(code);
            Origin = origin;
            Destination = destination;
            Route = route;
            Ticket = ticket;
            Restriction = restriction;
            TimebandName = timeband;
            DatebandName = dateband;
        }
    }
}
