            label1.Visible = true;
            label1.Text = "Start Download From FTP";
            string selected_file = listBox1.GetItemText(listBox1.SelectedItem);
            string local = @"C:\Local"; //yourlocallocation
            String url = "yourftp"; //yourftpserver
            NetworkCredential credentials = new NetworkCredential("user", "password");
            if (!Directory.Exists(local))
            {
                Directory.CreateDirectory(local);
            }

            FtpWebRequest listRequest = (FtpWebRequest)WebRequest.Create(url);
            listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            listRequest.Credentials = credentials;
            
            

            List<string> lines = new List<string>();

            using (var listResponse = (FtpWebResponse)listRequest.GetResponse())
            using (Stream listStream = listResponse.GetResponseStream())
            using (var listReader = new StreamReader(listStream))
            {
                while (!listReader.EndOfStream)
                {
                    lines.Add(listReader.ReadLine());
                }
            }

            foreach (string line in lines)
            {
                string[] tokens =
                    line.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                string name = tokens[8];
                string permissions = tokens[0];

                if (name == selected_file)
                {
                    string localFilePath = Path.Combine(local, name);
                    string fileUrl = url + name;

                    if (permissions[0] == 'd')
                    {
                        if (!Directory.Exists(localFilePath))
                        {
                            Directory.CreateDirectory(localFilePath);
                        }
                    }
                    else
                    {
                        // Query_sizeof_the_file_to_be_downloaded
                        WebRequest sizeRequest = WebRequest.Create(fileUrl);
                        sizeRequest.Credentials = credentials;
                        sizeRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                        int size = (int)sizeRequest.GetResponse().ContentLength;
                        progressBar1.Visible = true;
                        progressBar1.Invoke(
                            (MethodInvoker)(() => progressBar1.Maximum = size));


                        // Downloadthefile
                        FtpWebRequest downloadRequest =
                            (FtpWebRequest)WebRequest.Create(fileUrl);
                        downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                        downloadRequest.Credentials = credentials;

                       
                        using (FtpWebResponse downloadResponse = (FtpWebResponse)downloadRequest.GetResponse())
                        using (Stream sourceStream = downloadResponse.GetResponseStream())
                        using (Stream targetStream = File.Create(localFilePath))
                        {
                            byte[] buffer = new byte[10240];
                            int read;
                            int x = 0;
                            
                            while ((read = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                targetStream.Write(buffer, 0, read);
                                Console.WriteLine("Downloaded {0} bytes", targetStream.Length);
                                x = (int)targetStream.Position;
                                float d = x / 102400;
                                string g = d.ToString();
                                progressBar1.Invoke((MethodInvoker)(() => progressBar1.Value = x));
                                
                                progressBar1.CreateGraphics().DrawString(g+"  kb", SystemFonts.SmallCaptionFont,
                                    Brushes.Azure,
                                    new PointF(progressBar1.Width / 2 - (progressBar1.CreateGraphics().MeasureString(g+ "  kb",
                                    SystemFonts.DefaultFont).Width / 2.0F),
                                    progressBar1.Height / 2 - (progressBar1.CreateGraphics().MeasureString(g + "  kb",
                                    SystemFonts.DefaultFont).Height / 2.0F)));
                            }
                           
                        }
                    }
                    progressBar1.Visible = false;
                    break;
                }
