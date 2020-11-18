using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace California_Downloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            label2.Text = "File download: " + downloadto;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (UserName == "E1897" || UserName == "E1878" || UserName == "E2348" || UserName == "HOANG HA")
            {
                button1.Show();
            }
            else
            {
                button1.Hide();
            }
        }

        public string UserName = Environment.UserName.ToString().ToUpper();
        private void download(string url, string filename)
        {
            
            using (var client = new WebClient())
            {
                client.DownloadFile(url, filename);
            }
        }

        private bool RemoteFileExists(string url)
        {
            try
            {
                
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //Any exception will returns false.
                return false;
            }
        }

        private void CertificateAuth()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                ((sender, certificate, chain, sslPolicyErrors) => true);
            WebProxy proxy = new WebProxy("172.19.2.200:9090", true);
            proxy.Credentials = new NetworkCredential("E1872", "spivn@123", "SPI-GLOBAL");
            WebRequest.DefaultWebProxy = proxy;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            button1.Enabled = false;
            await Task.Run(() =>
            {
                Process();
            });
            button1.Enabled = true;
        }
        public string downloadto = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\";
        public string server = "https://www.courts.ca.gov/opinions/nonpub/";
        public string serverPublic = "https://www.courts.ca.gov/opinions/documents/";
        public bool isdone = false;
        private void Process()
        {
            string txtPath = textBox1.Text;
            if (Directory.Exists(txtPath))
            {
                int i = 0;
                string[] manifest = Directory.GetFiles(txtPath, "manifest.xml", SearchOption.AllDirectories);
                foreach (string m in manifest)
                {
                    string readManifest = File.ReadAllText(m);
                    string pattern = @"guid=""(.*?)""";
                    Match msid = Regex.Match(readManifest, pattern);
                    string sid = msid.Groups[1].ToString();
                    // string pattern2 = "CLINV_docket\" value=\"(.*?)\"";
                    // Match mDocket = Regex.Match(readManifest, pattern2);
                    // string Docket = mDocket.Groups[1].ToString();

                    if (readManifest.Contains("COURT OF APPEAL OF CALIFORNIA"))
                    {
                        dataGridView1.Rows.Add(++i, sid, getDocName(m));
                    }
                }
            }
            else
            {
                MessageBox.Show("Input not found or empty.");
            }
            int rowsCount = dataGridView1.Rows.Count;
            for (int j = 0; j < rowsCount; j++)
            {
                bool isExits = false;
                string docValue = dataGridView1.Rows[j].Cells[2].Value.ToString();
                string GuidValue = dataGridView1.Rows[j].Cells[1].Value.ToString();
                dataGridView1.Rows[j].Cells[3].Value = "Checking.... " + docValue;
                if (RemoteFileExists(server + docValue + ".DOC"))
                {
                    dataGridView1.Rows[j].Cells[3].Value = "Downloading file " + GuidValue + ".doc ...";
                    download(server + docValue + ".DOC", downloadto + GuidValue + ".DOC");
                    dataGridView1.Rows[j].Cells[3].Value = "Downloaded: " + GuidValue + ".doc";
                    isdone = true;
                    isExits = true;
                }
                else if (RemoteFileExists(server + docValue + ".DOCX"))
                {
                    dataGridView1.Rows[j].Cells[3].Value = "Downloading file " + GuidValue + ".DOCx ...";
                    download(server + docValue + ".DOCX", downloadto + GuidValue + ".DOCX");
                    dataGridView1.Rows[j].Cells[3].Value = "Downloaded: " + GuidValue + ".docx";
                    isdone = true;
                    isExits = true;
                }
                else if (RemoteFileExists(serverPublic + docValue + ".DOCX"))
                {
                    dataGridView1.Rows[j].Cells[3].Value = "Downloading file " + GuidValue + ".DOCx [Public] ...";
                    download(server + docValue + ".DOCX", downloadto + GuidValue + ".DOCX");
                    dataGridView1.Rows[j].Cells[3].Value = "Downloaded: " + GuidValue + ".docx [Public]";
                    isdone = true;
                    isExits = true;
                }
                else if (RemoteFileExists(serverPublic + docValue + ".DOC"))
                {
                    dataGridView1.Rows[j].Cells[3].Value = "Downloading file " + GuidValue + ".DOC [Public] ...";
                    download(server + docValue + ".DOC", downloadto + GuidValue + ".DOC");
                    dataGridView1.Rows[j].Cells[3].Value = "Downloaded: " + GuidValue + ".doc [Public]";
                    isdone = true;
                    isExits = true;
                }
                else if(isExits == false)
                {
                    dataGridView1.Rows[j].Cells[3].Value = "Not found.";
                    isdone = false;
                }
            }
            if (isdone)
            {
                MessageBox.Show("Download Completed.");
            }
            //dataGridView1.Rows[1].Cells[3].Value = 9;
        }


        private string getDocName(string manifest)
        {
            string text = File.ReadAllText(manifest);
            //string pattern = @"<item id=""([A-Z].*)_";
            string pattern = @"([A-H]\d+.*?)\.?_";
            Match docName = Regex.Match(text, pattern);
            string Name = docName.Groups[1].ToString();
            return Name;
        }

    }
}
