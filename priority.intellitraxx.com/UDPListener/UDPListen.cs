using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Configuration;

namespace UDPListener
{
    public class UDPListen
    {
        private Socket udpListener;
        private Thread listenThread;
        private byte[] data = new byte[4096];
        private int listenPort = 0;
        public delegate void newMessageHandler(Helpers.MessageData md);
        public event newMessageHandler handleNewMessage;
        String txt = "";

        #region " UDP Listener "

        public UDPListen()
        {
            setPort();
            listenThread = new Thread(new ThreadStart(UDPListenThread));
            listenThread.Start();
        }

        private void UDPListenThread()
        {
            udpListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint end = new IPEndPoint(IPAddress.Any, listenPort);
            EndPoint Identifier = (EndPoint)end;
            udpListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            if (!udpListener.IsBound)
            {
                udpListener.Bind(end);
            }

            while (true)
            {
                string message = null;
                try
                {
                    int length = udpListener.ReceiveFrom(data, ref Identifier);
                    message = System.Text.Encoding.UTF8.GetString(data, 0, length);
                    var appSettings = ConfigurationManager.AppSettings;
                    string _ipAddr = ((IPEndPoint)Identifier).Address.ToString();
                    Helpers.MessageData md = new Helpers.MessageData();
                    md.messageID = Guid.NewGuid();

                    if (ConfigurationManager.AppSettings["loggingOn"] == "T") {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(appSettings["messageLog"]))
                        {
                            txt += DateTime.Now.ToString() + "|--- " + message + "T:" + Identifier + "\r\n";
                            file.WriteLine(txt);
                        }
                    }

                    //Isolate the VehicleID from the message
                    if (message.Contains("ID:"))
                    {
                        string[] splitIP = message.Split('|');
                        md.VehicleID = splitIP[0].ToString().Replace("ID:", "");
                        md.Message = splitIP[1];


                        Helpers.GlobalData.messages.Add(md);
                        newMessageReceived(md);
                    }
                    else if (message.Contains(">RPV"))
                    {
                        md.VehicleID = message.Substring(38, 4);
                        md.Message = message;


                        Helpers.GlobalData.messages.Add(md);
                        newMessageReceived(md);
                    }
                    else if (message.Substring(0, 3) == "CAN")
                    {
                        string[] splitter = message.Split('|');
                        //0 = CAN identifier
                        //1 = MACAddress
                        //2 = Timestamp in UTC
                        //3 = Message data
                        md.VehicleID = "CAN";
                        md.Message = message;
                        Helpers.GlobalData.messages.Add(md);
                        newMessageReceived(md);
                    }
                    else if (message.Contains("<TR+"))
                    {
                        md.Message = message;
                        Helpers.GlobalData.messages.Add(md);
                        newMessageReceived(md);
                    }
                }
                catch (Exception ex)
                {
                    string err = ex.ToString();
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(ConfigurationManager.AppSettings["errorLog"]))
                    {
                        txt += DateTime.Now.ToString() + "|--- " + err + "\r\n";
                        file.WriteLine(err);
                        file.Close();
                    }
                }
            }
        }

        public void unBind()
        {
            if (udpListener.IsBound)
            {
                udpListener.Close();
            }
        }
        #endregion

        #region " Message Events "

        /// <summary>
        /// Fires when a new message arrives
        /// </summary>
        /// <param name="md">Message data packet to be notified</param>
        protected virtual void newMessageReceived(Helpers.MessageData md)
        {
            if (handleNewMessage != null)
            {
                handleNewMessage(md);
            }
        }

        /// <summary>
        /// Remove a message from the in-memory list of messages
        /// </summary>
        /// <param name="messageID">GUID Message ID to be removed</param>
        public void removeMessage(Guid messageID)
        {
            Helpers.GlobalData.removeMessage(messageID);
        }

        #endregion

        #region " Config "

        private void setPort()
        {
            listenPort = Convert.ToInt32(ConfigurationManager.AppSettings["listenPort"]);
            //listenPort = 9028;
        }

        #endregion
    }
}