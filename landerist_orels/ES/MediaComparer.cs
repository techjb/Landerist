﻿using System;
using System.Collections.Generic;
using System.Text;

namespace landerist_orels.ES
{
    public class MediaComparer : IComparer<Media>
    {
        public int Compare(Media x, Media y)
        {
            return x.url.ToString().CompareTo(y.url.ToString());
        }
    }
}