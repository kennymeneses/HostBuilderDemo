using System.Collections.Generic;

namespace HostBuilderDemo.Host.Models
{
    public class QueueMessage
    {
        public List<string> listQueue { get; set; }

        public QueueMessage()
        {
            listQueue = new List<string>();
        }
    }
}
