using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace landerist_library.Parse.Listing.OpenAI
{

#pragma warning disable CS8618
#pragma warning disable IDE1006

    public class BatchMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class Body
    {
        public string model { get; set; }
        public List<BatchMessage> messages { get; set; }        
    }

    public class RequestData
    {
        public string custom_id { get; set; }
        public string method { get; set; }
        public string url { get; set; }
        public Body body { get; set; }
    }

#pragma warning restore CS8618
#pragma warning restore IDE1006
}
