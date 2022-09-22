using System;
using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// A product booking record & process.
    /// </summary>
    public class Book : Entity, IKeyable<int>, IFlowable
    {
        public static readonly Book Empty = new Book();

        // states
        public const short
            STA_CREATED = 0, // shop
            STA_PAID = 1, // paid
            STA_CANCELLED = 2, // 
            STA_CONFIRMED = 3, // confirmed and reveal to the center
            STA_REJECTED = 4, // center rejected
            STA_DELIVERED = 5, // delivered by the center
            STA_DENIED = 6, // shop returned
            STA_RECEIVED = 7; // shop received


        public new static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STA_CREATED, null},
            {STA_PAID, "已付款"},
            {STA_CANCELLED, "已撤单"},
            {STA_CONFIRMED, "已确单"},
            {STA_REJECTED, "已回拒"},
            {STA_DELIVERED, "已发货"},
            {STA_DENIED, "已拒收"},
            {STA_RECEIVED, "已收货"},
        };


        internal int id;

        internal int shpid; // shop
        internal int mktid; // market
        internal int ctrid; // center
        internal int srcid; // source
        internal int zonid; // zone

        internal int itemid;
        internal short lotid;

        internal string unit;
        internal string unitstd;
        internal short unitx;
        internal decimal price;
        internal decimal off;
        internal short qty;
        internal short cut; // qty cut
        internal decimal pay;
        internal decimal refund; // pay refunded

        internal string oker;
        internal DateTime oked;
        internal short state;


        public override void Read(ISource s, short msk = 0xff)
        {
            base.Read(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(shpid), ref shpid);
                s.Get(nameof(mktid), ref mktid);
                s.Get(nameof(ctrid), ref ctrid);
                s.Get(nameof(zonid), ref zonid);
                s.Get(nameof(srcid), ref srcid);
                s.Get(nameof(itemid), ref itemid);
                s.Get(nameof(lotid), ref lotid);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Get(nameof(unit), ref unit);
                s.Get(nameof(unitstd), ref unitstd);
                s.Get(nameof(unitx), ref unitx);
                s.Get(nameof(price), ref price);
                s.Get(nameof(off), ref off);
                s.Get(nameof(qty), ref qty);
                s.Get(nameof(pay), ref pay);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(oker), ref oker);
                s.Get(nameof(oked), ref oked);
                s.Get(nameof(state), ref state);

                s.Get(nameof(cut), ref cut);
                s.Get(nameof(refund), ref refund);
            }
        }

        public override void Write(ISink s, short msk = 0xff)
        {
            base.Write(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Put(nameof(id), id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Put(nameof(shpid), shpid);
                s.Put(nameof(mktid), mktid);
                s.Put(nameof(ctrid), ctrid);
                s.Put(nameof(zonid), zonid);
                s.Put(nameof(srcid), srcid);

                s.Put(nameof(itemid), itemid);
                s.Put(nameof(lotid), lotid);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(unit), unit);
                s.Put(nameof(unitstd), unitstd);
                s.Put(nameof(unitx), unitx);
                s.Put(nameof(price), price);
                s.Put(nameof(off), off);
                s.Put(nameof(qty), qty);
                s.Put(nameof(pay), pay);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(oker), oker);
                s.Put(nameof(oked), oked);
                s.Put(nameof(state), state);

                s.Put(nameof(cut), cut);
                s.Put(nameof(refund), refund);
            }
        }

        public int Key => id;

        public bool IsOver(DateTime now) => false;

        public short Status => status;


        public string Oker => oker;

        public DateTime Oked => oked;

        public short State => state;
    }
}