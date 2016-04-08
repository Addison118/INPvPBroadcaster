﻿using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;
using MiNET;
using MiNET.BlockEntities;
using MiNET.Blocks;
using MiNET.Effects;
using MiNET.Entities;
using MiNET.Entities.ImageProviders;
using MiNET.Entities.World;
using MiNET.Items;
using MiNET.Net;
using MiNET.Particles;
using MiNET.Plugins;
using MiNET.Plugins.Attributes;
using MiNET.Utils;
using MiNET.Worlds;
using log4net;

namespace TestInpvp
{
    [Plugin(PluginName = "InpvpTest", Description = "A plugin for inpvp", PluginVersion = "1.0", Author = "Addison118")]
    public class test : Plugin
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(test));

        private Timer broadcastTimer;

        protected override void OnEnable()
        {
            Logger.Info("Starting InPvP test plugin...");
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create("broadcast.xml", settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("Messages");
            writer.WriteElementString("Message", "This is the 1st message");
            writer.WriteElementString("Message", "This is the 2nd message");
            writer.WriteElementString("Message", "This is the 3rd message");
            writer.Close();
            XmlWriter config = XmlWriter.Create("inpvp.xml", settings);
            config.WriteStartDocument();
            config.WriteStartElement("config");
            config.WriteElementString("messageDelay", "3000");
            config.Close();
            broadcastTimer = new Timer(broadcastMessages, null, 10000, 20000);
        }

        private void broadcastMessages(object state)
        {

            foreach(var level in Context.LevelManager.Levels)
            {
                XmlDocument xml = new XmlDocument();
                XmlDocument config = new XmlDocument();
                config.Load("inpvp.xml");
                xml.Load("broadcast.xml");
                XmlNodeList nodeList = config.GetElementsByTagName("messageDelay");
                XmlNodeList xList = xml.SelectNodes("Messages");
                string messageDelay = null;
                foreach (XmlNode node in nodeList)
                {
                    messageDelay = node.InnerText;
                }
                foreach (XmlNode xm in xList)
                {
                    XmlNodeList messages = xm.SelectNodes("./Message");
                    foreach (XmlNode msg in messages
                        .Cast<XmlNode>()
                        .OrderBy(elem => Guid.NewGuid()))
                    {
                        string currentMessage = msg.InnerText;
                        level.BroadcastMessage(currentMessage);
                        Thread.Sleep(Int32.Parse(messageDelay));
                    }
                }
            }
        }
    }
}