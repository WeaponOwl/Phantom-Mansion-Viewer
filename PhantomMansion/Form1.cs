using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Net;

namespace PhantomMansion
{
    public partial class Form1 : Form
    {
        private string[] basicUrls = new string[]{
            "http://www8.agame.com/mirror/flash/p/phantommansionred",
            "http://www8.agame.com/mirror/flash/p/phantommansionorange",
            "http://www8.agame.com/mirror/flash/p/phantommansionyellow",
            "http://www8.agame.com/mirror/flash/p/phantommansiongreen",
            "http://www8.agame.com/mirror/flash/p/phantommansionblue",
            "http://www8.agame.com/mirror/flash/p/phantommansionindigo",
            "http://www8.agame.com/mirror/flash/p/phantommansionviolet"
        };

        private XmlDocument mapsInfo;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listBox1.SelectedIndex = 0;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            var nodes = mapsInfo.SelectNodes("/project/layergroup");
            var lobby = nodes[listBox2.SelectedIndex];

            // Tiles
            var tilelayer = lobby.SelectNodes("./tilelayer")[1];
            string tilePath = tilelayer.Attributes["xmlFile"].Value;

            var width = int.Parse(tilelayer.SelectSingleNode("./tilemap/width").InnerText);
            var height = int.Parse(tilelayer.SelectSingleNode("./tilemap/height").InnerText);

            string layerUrl = basicUrls[listBox1.SelectedIndex] + "/_maps" + tilePath.Replace('\\', '/');
            string layerString = "";

            using (WebClient client = new WebClient())
                layerString = client.DownloadString(layerUrl);
            richTextBox1.Text = layerString;

            XmlDocument layerInfo = new XmlDocument();
            layerInfo.LoadXml(layerString);

            string layerBody = layerInfo.SelectSingleNode("./tiledLayer/data").Attributes["tiledata"].Value;

            // Items
            var itemlayer = lobby.SelectNodes("./tilelayer")[0];
            string itemPath = itemlayer.Attributes["xmlFile"].Value;

            layerUrl = basicUrls[listBox1.SelectedIndex] + "/_maps" + itemPath.Replace('\\', '/');
            layerString = "";

            using (WebClient client = new WebClient())
                layerString = client.DownloadString(layerUrl);
            richTextBox2.Text = layerString;

            layerInfo = new XmlDocument();
            layerInfo.LoadXml(layerString);

            string itemLayerBody = layerInfo.SelectSingleNode("./tiledLayer/data").Attributes["tiledata"].Value;

            // Objects
            var objlayer = lobby.SelectSingleNode("./objectlayer");
            string objPath = objlayer.Attributes["xmlFile"].Value;

            layerUrl = basicUrls[listBox1.SelectedIndex] + "/_maps" + objPath.Replace('\\', '/');
            layerString = "";
            using (WebClient client = new WebClient())
                layerString = client.DownloadString(layerUrl);
            richTextBox3.Text = layerString;

            layerInfo = new XmlDocument();
            layerInfo.LoadXml(layerString);

            var objNodes = layerInfo.SelectNodes("/objectLayer/data/object");

            // Render
            Bitmap tileSet = null;
            using (WebClient webClient = new WebClient())
            {
                byte[] data = webClient.DownloadData(basicUrls[listBox1.SelectedIndex] + "/_tiles/tiles.png");

                using (MemoryStream mem = new MemoryStream(data))
                {
                    tileSet = new Bitmap(Bitmap.FromStream(mem));
                }
            }

            Bitmap mapSet = new Bitmap(width * 32, height * 32);

            using (Graphics grD = Graphics.FromImage(mapSet))
            {
                string[] tileCells = layerBody.Split(',');
                string[] itemCells = itemLayerBody.Split(',');
                for (int i = 0; i < tileCells.Length; i++)
                {
                    int tileid = int.Parse(tileCells[i]);
                    int itemid = int.Parse(itemCells[i]);

                    grD.DrawImage(tileSet, new Rectangle((i / height) * 32, (i % height) * 32, 32, 32), new Rectangle((tileid % 4) * 32, (tileid / 4) * 32, 32, 32), GraphicsUnit.Pixel);
                    if (itemid > 0)
                    {
                        grD.DrawString(itemid.ToString(), System.Drawing.SystemFonts.CaptionFont, System.Drawing.Brushes.Black, (i / height) * 32 + 7, (i % height) * 32 + 7);
                        grD.DrawString(itemid.ToString(), System.Drawing.SystemFonts.CaptionFont, System.Drawing.Brushes.Black, (i / height) * 32 + 5, (i % height) * 32 + 5);
                        grD.DrawString(itemid.ToString(), System.Drawing.SystemFonts.CaptionFont, System.Drawing.Brushes.Black, (i / height) * 32 + 5, (i % height) * 32 + 7);
                        grD.DrawString(itemid.ToString(), System.Drawing.SystemFonts.CaptionFont, System.Drawing.Brushes.Black, (i / height) * 32 + 7, (i % height) * 32 + 5);
                        grD.DrawString(itemid.ToString(), System.Drawing.SystemFonts.CaptionFont, System.Drawing.Brushes.White, (i / height) * 32 + 6, (i % height) * 32 + 6);
                    }
                }

                foreach (XmlNode node in objNodes)
                {
                    int x = int.Parse(node.Attributes["x"].Value);
                    int y = int.Parse(node.Attributes["y"].Value);

                    string value = node.Attributes["value"].Value;

                    grD.DrawString(value, System.Drawing.SystemFonts.CaptionFont, System.Drawing.Brushes.Black, x * 32 + 7, y * 32 + 7);
                    grD.DrawString(value, System.Drawing.SystemFonts.CaptionFont, System.Drawing.Brushes.Black, x * 32 + 5, y * 32 + 7);
                    grD.DrawString(value, System.Drawing.SystemFonts.CaptionFont, System.Drawing.Brushes.Black, x * 32 + 5, y * 32 + 5);
                    grD.DrawString(value, System.Drawing.SystemFonts.CaptionFont, System.Drawing.Brushes.Black, x * 32 + 7, y * 32 + 5);
                    grD.DrawString(value, System.Drawing.SystemFonts.CaptionFont, System.Drawing.Brushes.Yellow, x * 32 + 6, y * 32 + 6);
                }
            }

            pictureBox1.Image = mapSet;
            pictureBox1.Width = mapSet.Width;
            pictureBox1.Height = mapSet.Height;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string mapsUrl = basicUrls[listBox1.SelectedIndex] + "/_maps/deluxe.xml";
            string mapsData = "";

            using (WebClient client = new WebClient())
                mapsData = client.DownloadString(mapsUrl);
            richTextBox4.Text = mapsData;

            mapsInfo = new XmlDocument();
            mapsInfo.LoadXml(mapsData);

            var nodes = mapsInfo.SelectNodes("/project/layergroup");
            listBox2.Items.Clear();
            foreach (XmlNode node in nodes)
            {
                var nameNode = node.SelectSingleNode("./name");
                listBox2.Items.Add(nameNode.InnerText);
            }
            listBox2.SelectedIndex = 0;

            using (WebClient webClient = new WebClient())
            {
                byte[] data = webClient.DownloadData(basicUrls[listBox1.SelectedIndex] + "/_tiles/tiles.png");

                using (MemoryStream mem = new MemoryStream(data))
                {
                    pictureBox2.Image = new Bitmap(Bitmap.FromStream(mem));
                }
            }

        }
    }
}
