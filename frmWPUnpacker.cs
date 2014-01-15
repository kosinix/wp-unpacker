using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using Ionic.Zip; // Using http://dotnetzip.codeplex.com/. Resides in WPUnpacker/lib
using System.IO;

namespace WPUnpacker
{
    public partial class frmMain : Form
    {
        string downloadedZip;
        string tmpDir;

        public frmMain()
        {
            InitializeComponent();

            // Defaults 
            txtTargetFolder.Text = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // Path to User's My Documents
            txtVersion.Text = "3.8";

        }

        private void btnSaveTo_Click(object sender, EventArgs e)
        {
            if (fbdFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtTargetFolder.Text = fbdFolder.SelectedPath;
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            
            btnRun.Enabled = false;

            WebClient client = new WebClient();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);

            // Save download here
            tmpDir = txtTargetFolder.Text + @"\tmp";
            Directory.CreateDirectory(tmpDir);
            downloadedZip = tmpDir + @"\wp.zip";

            // Start the download
            client.DownloadFileAsync(new Uri("http://wordpress.org/wordpress-" + txtVersion.Text + ".zip"), downloadedZip);


            lblStatus.Text = "Connecting...";
            progressBar.Visible = true;
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            progressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
            lblStatus.Text = "Downloading... " + Convert.ToString(Math.Truncate(percentage)) + "%";
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            lblStatus.Text = "Download Completed";
            lblStatus.Text = "Extracting zip...";

            // Extract zip to temporary dir
            extractZip(downloadedZip, tmpDir);

            lblStatus.Text = "Extraction Completed";

            lblStatus.Text = "Copying files...";
            // File is extracted to /wordpress so we copy to root folder and delete /wordpress
            CopyDirectory.Copy(tmpDir + @"\wordpress", txtTargetFolder.Text);
            Directory.Delete(tmpDir, true);

            lblStatus.Text = "Completed";

            btnRun.Enabled = true;
            progressBar.Visible = false;

        }


        private void extractZip(string zipFile, string targetFolder)
        {
            using (ZipFile zip = ZipFile.Read(zipFile))
            {
                zip.ExtractAll(targetFolder, ExtractExistingFileAction.OverwriteSilently);
            }
            
        }
        
    }
}
