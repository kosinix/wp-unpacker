using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.IO.Compression; // Built-in zip available in .NET 4.5+
using System.Threading.Tasks;

namespace WPUnpacker
{
    public partial class frmMain : Form
    {
        string downloadedZip;
        string tmpDir;
        string downloadDir;

        public frmMain()
        {
            InitializeComponent();
            
            // Get defaults 
            txtVersion.Text = Properties.Settings.Default.wpVersion;
            txtTargetFolder.Text = Properties.Settings.Default.targetFolder;
            
            if (txtTargetFolder.Text == "") // No path saved yet
            {
                Properties.Settings.Default.targetFolder = txtTargetFolder.Text = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // Path to User's My Documents
            }

        }

        private void btnSaveTo_Click(object sender, EventArgs e)
        {
            if (fbdFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtTargetFolder.Text = fbdFolder.SelectedPath;
            }
        }

        private async void btnRun_Click(object sender, EventArgs e)
        {
            txtTargetFolder.Enabled = false;
            txtVersion.Enabled = false;
            btnRun.Enabled = false;
            btnSaveTo.Enabled = false;

            this.WindowState = FormWindowState.Normal;
            this.Activate();

            return;
            // Download dir sits beside the .exe file
            downloadDir = AppDomain.CurrentDomain.BaseDirectory + @"downloads";
            
            // Create if non existent
            if (Directory.Exists(downloadDir) == false)
            {
                Directory.CreateDirectory(downloadDir);
            }

            // Zip full path in local machine
            // Eg. D:\wp-unpacker\downloads\wordpress-4.3.1.zip
            downloadedZip = downloadDir + @"\wordpress-" + txtVersion.Text + ".zip";

            if (File.Exists(downloadedZip) == true) // Already downloaded this zip before
            {

                await processZip();
                finish();

            } else { // Non existent, download from repo

                string remoteZip = "http://wordpress.org/wordpress-" + txtVersion.Text + ".zip";

                lblStatus.Text = "Connecting to wordpress.org ...";

                if (RemoteFileExists(remoteZip) == true)
                {


                    WebClient client = new WebClient();
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);


                    // Start the download
                    client.DownloadFileAsync(new Uri(remoteZip), downloadedZip);


                    
                    progressBar.Visible = true;
                }
                else
                {
                    lblStatus.Text = remoteZip + " does not exist!";
                    finish();
                }
            }

            
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            progressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
            lblStatus.Text = "Downloading... " + Convert.ToString(Math.Truncate(percentage)) + "%";
        }

        async void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            await processZip();

            finish();
        }

        // Extract and copy zip to target folder
        private async Task processZip()
        {
            lblStatus.Text = "Extracting zip...";

            // Extract zip to target dir in a non blocking way. See await
            await Task.Run(() => ZipFile.ExtractToDirectory(downloadedZip, txtTargetFolder.Text));

            lblStatus.Text = "Extraction completed. Copying files...";

            // File is extracted to \wordpress inside targetfolder so we copy it a level up
            // Using the awesome await 
            await Task.Run(
                () => // Anonymous function here
                {
                    CopyDirectory.Copy(txtTargetFolder.Text + @"\wordpress", txtTargetFolder.Text);
                    // And delete \wordpress dir
                    Directory.Delete(txtTargetFolder.Text + @"\wordpress", true);
                }
            );


            lblStatus.Text = "Completed";

            
        }

        private void finish()
        {
            txtTargetFolder.Enabled = true;
            txtVersion.Enabled = true;
            btnRun.Enabled = true;
            btnSaveTo.Enabled = true;
            Properties.Settings.Default.wpVersion = txtVersion.Text;
            Properties.Settings.Default.targetFolder = txtTargetFolder.Text;
            Properties.Settings.Default.Save();
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
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }
    }
}
