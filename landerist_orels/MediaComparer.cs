using System.Collections.Generic;

namespace landerist_orels
{
    public class MediaComparer : IComparer<Media>
    {
        public int Compare(Media x, Media y)
        {
            string yCompared = y.url.ToString().ToLower().Trim();
            return x.url.ToString().ToLower().Trim().CompareTo(yCompared);
        }
    }
}
