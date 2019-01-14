using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDPListener.Helpers
{
    public static class GlobalData
    {
        public static List<MessageData> messages = new List<MessageData>();

        public static void removeMessage(Guid messageID)
        {
            for (int i = messages.Count -1; i >= 0; i--)
            {
                if (messages[i].messageID == messageID)
                {
                    messages.RemoveAt(i);
                }
            }
        }
    }
}
