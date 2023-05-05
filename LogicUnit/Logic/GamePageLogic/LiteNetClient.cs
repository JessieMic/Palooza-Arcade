﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
namespace LogicUnit.Logic.GamePageLogic

{
    //make it singleton
    public class LiteNetClient
    {
        private static EventBasedNetListener m_Listener = new EventBasedNetListener();
        private readonly NetManager r_NetManager = new NetManager(m_Listener);
        //private readonly ILogger<LiteNetClient> r_Logger;

        public LiteNetClient(/*ILogger<LiteNetClient> i_Logger*/)
        {
            //r_Logger = i_Logger;
            r_NetManager.Start();
            r_NetManager.Connect("127.0.0.1", 5555, "myKey");
            m_Listener.NetworkReceiveEvent += OnReceive;
        }

        public void Send(string i_Message)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put(i_Message);
            r_NetManager.FirstPeer.Send(writer, DeliveryMethod.ReliableSequenced);
            //r_Logger.LogInformation($"Sent: {i_Message}");
        }

        private void OnReceive(NetPeer i_Peer, NetPacketReader i_Reader, byte i_Channel, DeliveryMethod i_Deliverymethod)
        {
            string message = i_Reader.GetString();
            Console.WriteLine(message);
            //r_Logger.LogInformation($"Received: {message}");
            i_Reader.Recycle();
        }

    }
}
