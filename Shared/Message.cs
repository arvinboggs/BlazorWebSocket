using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorWebSocket.Shared
{
    public class Message
    {
        public enum eType
        {
            Text,
            System,
            Error,
            LoginSuccess
        }
        public eType MessageType;
        public string Text;
        public string Sender;

        public Message()
        {

        }

        public Message(eType vMessageType, string vText)
        {
            MessageType = vMessageType;
            Text = vText;
        }

        public static string ToString(eType vMessageType, string vText)
        {
            var p = new Message(vMessageType, vText);
            var pOut = JsonConvert.SerializeObject(p);
            return pOut;
        }

        public static string ToString(eType vMessageType, string vSender, string vText)
        {
            var p = new Message(vMessageType, vText);
            p.Sender = vSender;
            var pOut = JsonConvert.SerializeObject(p);
            return pOut;
        }

        public static Message FromString(string vString)
        {
            if (string.IsNullOrWhiteSpace(vString))
                return new Message();
            try
            {
                var pOut = JsonConvert.DeserializeObject<Message>(vString);
                return pOut;
            }
            catch (Exception ex)
            {
                return new Message();
            }
        }
    }
}
