using System;
using System.Collections.Generic;
using System.Text;

namespace landerist_orels.ES
{
    public class ListingComparer : IComparer<Listing>
    {
        public int Compare(Listing x, Listing y)
        {
            return x.guid.CompareTo(y.guid);
        }
    }
}
