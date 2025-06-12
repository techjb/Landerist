using System.Collections.Generic;

namespace landerist_orels
{
    public class SourceComparer : IComparer<Source>
    {
        public int Compare(Source x, Source y)
        {
            string yCompared = y.sourceUrl.ToString().ToLower().Trim();
            return x.sourceUrl.ToString().ToLower().Trim().CompareTo(yCompared);
        }
    }
}
