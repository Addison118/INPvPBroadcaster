using System;
using System.IO;
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

        private int width, height, length;

        private PlayerLocation spawn;

        XmlDocument xml = new XmlDocument();
        XmlDocument config = new XmlDocument();

        protected override void OnEnable()
        {
            Logger.Info("Starting InPvP test plugin...");
            XmlWriterSettings settings = new XmlWriterSettings();
            if (!File.Exists("broadcast.xml")) {
                settings.Indent = true;
                XmlWriter writer = XmlWriter.Create("broadcast.xml", settings);
                writer.WriteStartDocument();
                writer.WriteStartElement("Messages");
                writer.WriteElementString("Message", "This is the 1st message");
                writer.WriteElementString("Message", "This is the 2nd message");
                writer.WriteElementString("Message", "This is the 3rd message");
                writer.Close();
            }
            if (!File.Exists("inpvp.xml")) {
                XmlWriter config = XmlWriter.Create("inpvp.xml", settings);
                config.WriteStartDocument();
                config.WriteStartElement("config");
                config.WriteElementString("messageDelay", "5"); //In seconds
                config.WriteElementString("barrierHeight", "7");
                config.WriteElementString("barrierWidth", "7");
                config.WriteElementString("barrierLength", "7");
                config.Close();
            }
            broadcastTimer = new Timer(broadcastMessages, null, 10000, 20000);
            config.Load("inpvp.xml");
            XmlNodeList nodes = xml.SelectNodes("config");
            foreach(XmlNode node in nodes)
            {
                width = Int32.Parse(node["barrierWidth"].InnerText);
                height = Int32.Parse(node["barrierHeight"].InnerText);
                length = Int32.Parse(node["barrierLength"].InnerText);
            }
        }

        private void OnPlayerJoin(object o, PlayerEventArgs e)
        {
            Level level = e.Level;
            Vector3 spawn = (BlockCoordinates)e.Player.SpawnPosition;
            for (double x = spawn.X - width / 2; x < spawn.X + width / 2; x++)
            {
                for (double y = spawn.Y - height / 2; y < spawn.Y; y++)
                {
                    for (double z = spawn.Z - length / 2; z < spawn.Z + length / 2; z++)
                    {
                        BlockCoordinates barrier = new BlockCoordinates(Convert.ToInt32(x), Convert.ToInt32(y), Convert.ToInt32(z));
                        var bar = new InvisibleBedrock { Coordinates = barrier };
                        level.SetBlock(bar);
                    }
                }
            }

        }

        private void broadcastMessages(object state)
        {

            foreach(var level in Context.LevelManager.Levels)
            {
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
                        Thread.Sleep(Int32.Parse(messageDelay) * 1000); //Convert the seconds given to milliseconds
                    }
                }
            }
        }
    }
}
