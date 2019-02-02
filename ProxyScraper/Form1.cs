using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
/**
 * ProxyScraper --- This program gathers free proxies that are publicly avaliable on specific websites.
 * @author    Chris Campone
 */
namespace ProxyScraper
{
    public partial class Form1 : Form
    {
        ArrayList proxyList = new ArrayList();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        //site 1
        private async Task getProxyFromDailyProxy()
        {
            string link = "http://proxy-daily.com/";
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(link);//go to link
            myRequest.Method = "GET";
            WebResponse myResponse = myRequest.GetResponse();//read the source code of the website
            StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);//convert to plaintext just incase
            await Task.Delay(200);
            while (sr.Peek() >= 0)
            {
                try
                {
                    string line = sr.ReadLine();
                    //make sure the the line in the source code is in fact a proxy. 
                    if (line.Contains(":") && line.Contains(".") && !line.Contains(">") && !line.Contains("("))
                    {
                       proxyList.Add(line);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("getProxyFromDailyProxy: "+e.ToString());
                }

            }
            sr.Close();
            myResponse.Close();
            label2.Text = proxyList.Count.ToString();
            cmd("**Daily-Proxy Scrapped**");
        }

        //site 2
        private async void getProxyFromFreeProxyList()
        {
            string link = "https://free-proxy-list.net/";
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(link);//go to site
            myRequest.Method = "GET";
            WebResponse myResponse = myRequest.GetResponse();//get source code
            StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
            await Task.Delay(200);
            string all = sr.ReadToEnd();

            try
            {
                while (all.Length > 0)
                {
                    //loop thru each line and keep stripping the html tags for each proxy

                    all = all.Substring(all.IndexOf("<tr><td>"));//first proxy is found at these tags
                    string ip = "x";
                    string port = "x";
                    all = all.Substring(8);  //How the source will look: 203.126.218.186</td><td>80</td><td>SG</td
                    ip = all.Substring(0, all.IndexOf("</td><td>"));
                    all = all.Substring(all.IndexOf("<td>") + 4);//get the port: 80</td><td>SG</td
                    port = all.Substring(0, all.IndexOf("</td><td>"));
                    proxyList.Add(ip+":"+port);
                }
            }
            catch (Exception)
            {
                
            }
            sr.Close();
            myResponse.Close();
            label2.Text = proxyList.Count.ToString();
            cmd("**Free-Proxy Scrapped**");
        }

        //site 3
        private async void getProxyFromHTMLTunnel()
        {
            try
            {
                cmd("Gathering proxies from HTTP Tunnel");
                ArrayList links = new ArrayList();
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create("http://www.httptunnel.ge/ProxyListForFree.aspx");
                myRequest.Method = "GET";
                WebResponse myResponse = myRequest.GetResponse();
                StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
                await Task.Delay(200);
                while (sr.Peek() >= 0)
                {
                    string line = sr.ReadLine();
                    string s = "";
                    //for this source, each html proxy should contain hyperlink
                    if (line.Contains("HyperLink2"))
                    {
                        s = line.Substring(line.IndexOf("target=\"_new\"") + 14);//232332:8080</a>
                        s = s.Substring(0, s.Length - 4);
                        proxyList.Add(s);
                    }
                }
                if (textBox3.Lines.Length >= 6) textBox3.Text = "";
                label2.Text = proxyList.Count.ToString();
                cmd("**HTTP Tunnel Scrapped**");
                sr.Close();
                myResponse.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("HTML Tunnel: "+e.ToString());
            }
        }

        //site 4
        private  async void getProxyFromProxyServerList()
        {
            string link = "http://www.proxyserverlist24.top/";
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(link);
            myRequest.Method = "GET";
            WebResponse myResponse = myRequest.GetResponse();
            StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
            await Task.Delay(200);
            string all = sr.ReadToEnd();

            try
            {
                //this site has multiple pages that we must increment
                ArrayList proxylinks = new ArrayList();
                while(all.Contains("post-title entry-title")){
                    all = all.Substring(all.IndexOf("post-title entry-title"));
                    all = all.Substring(all.IndexOf("href=") + 6);
                    string plink = all.Substring(0, all.IndexOf("'"));
                    if(!plink.ToLower().Contains("smtp")){
                        proxylinks.Add(plink);
                    }
                }

                foreach (var item in proxylinks)
                {
                    myRequest = (HttpWebRequest)WebRequest.Create(item.ToString());
                    myRequest.Method = "GET";
                    myResponse = myRequest.GetResponse();
                    sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
                    await Task.Delay(200);
                    while (sr.Peek() >= 0)
                    {
                        try
                        {
                            string line = sr.ReadLine();

                            if (line.Contains(":") && line.Contains(".") && !line.Contains(" "))
                            {
                                proxyList.Add(line);
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("getProxyFromProxyServerList: " + e.ToString());
                        }

                    }

                }

               
            }
            catch (Exception)
            {

            }
            sr.Close();
            myResponse.Close();
            label2.Text = proxyList.Count.ToString();
            cmd("**Proxy Server Scrapped**");
        }

        async private Task run()
        {
            proxyList.Clear();
            cmd("Gathering proxies");
            label2.Text = "0";
            label4.Text = "0";
            label6.Text = "0";
            timer1.Start();
            timerCount = 0;
            int sitesScraped = 0;
            try
            {
                
                getProxyFromFreeProxyList();
                sitesScraped++;
                label4.Text = sitesScraped.ToString();
                await Task.Delay(400);
                getProxyFromHTMLTunnel();
                sitesScraped++;
                label4.Text = sitesScraped.ToString();
                await Task.Delay(400);
                await getProxyFromDailyProxy();
                sitesScraped++;
                label4.Text = sitesScraped.ToString();
                await Task.Delay(400);
            }
            catch (Exception x)
            {
                MessageBox.Show("btn 1 click: " + x.ToString());
            }
            cmd("Getting rid of duplicates...");
            await Task.Delay(100);
            for (int i = 0; i < proxyList.Count; i++)
            {
                for (int j = i + 1; j < proxyList.Count; j++)
                    if (proxyList[i].ToString() == proxyList[j].ToString())
                        proxyList.Remove(proxyList[j]);
            }


            cmd("complete");
            label2.Text = proxyList.Count.ToString();
            textBox2.Text = "";
            for (int i = 0; i < 20; i++)
            {
                textBox2.Text += proxyList[i] + Environment.NewLine;
            }
            button2.Enabled = true;
            timer1.Stop();
        }

        //Run proxyscraper
        async private void button1_Click(object sender, EventArgs e)
        {
            await run();
        }

       
        private void writeFile(ArrayList array, string name)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            try
            {
                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(path + @"\" + name + ".txt"))
                {
                    foreach (string line in array)
                    {
                        file.WriteLine(line);
                    }
                }
                cmd("File saved on desktop: " + name + ".txt");
            }
            catch (Exception e)
            {

            }
        }

        
        private string findReplace(string Source, string Find, string Replace)
        {
            int Place = Source.IndexOf(Find);
            string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
            return result;
        }

        //Save btn
        private void button2_Click(object sender, EventArgs e)
        {
            writeFile(proxyList,"Fresh-Proxies");
            button2.Enabled = false;
        }

        //clear btn
        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Text = "Proxy List Preview";
            proxyList.Clear();
            label2.Text = proxyList.Count.ToString();
            cmd("Proxy list cleared");
            button2.Enabled = false;
            label2.Text = "0";
            label4.Text = "0";
            label6.Text = "0";
        }

        private void cmd(string x)
        {
            if(textBox3.Lines.Count() >= 7){
                textBox3.Text = "";
            }
            textBox3.Text += x + Environment.NewLine;
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //This allows me to be able to drag the program from the top panel
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        //keep track of time it takes to gather proxies
        int timerCount = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            timerCount++;
            label6.Text = timerCount+" seconds";
        }


    }
}
