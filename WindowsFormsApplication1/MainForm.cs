using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.IO;




namespace WindowsFormsApplication1
{

    public partial class MainForm : Form
    {


        //initialize the space for our dictionary data
        DictionaryData DictData = new DictionaryData();


        //this is what runs at initialization
        public MainForm()
        {

            InitializeComponent();

            

            foreach (var encoding in Encoding.GetEncodings())
            {
                OutputEncodingDropdown.Items.Add(encoding.Name);
            }

            
            try
            {
                OutputEncodingDropdown.SelectedIndex = OutputEncodingDropdown.FindStringExact("utf-8");
            }
            catch
            {
                OutputEncodingDropdown.SelectedIndex = OutputEncodingDropdown.FindStringExact(Encoding.Default.BodyName);
            }




        }







        private void StartButton_Click(object sender, EventArgs e)
        {


           

                    FolderBrowser.Description = "Please choose the location of your .txt files to analyze";
                    if (FolderBrowser.ShowDialog() != DialogResult.Cancel) {

                        DictData.TextFileFolder = FolderBrowser.SelectedPath.ToString();
                
                        if (DictData.TextFileFolder != "")
                        {


                            saveFileDialog.InitialDirectory = DictData.TextFileFolder;
                            if (saveFileDialog.ShowDialog() != DialogResult.Cancel) {


                     

                                DictData.OutputFileLocation = saveFileDialog.FileName;
                                DictData.FileExtension = FileTypeTextbox.Text.Trim();


                                if (DictData.OutputFileLocation != "") {



                                    StartButton.Enabled = false;
                                    ScanSubfolderCheckbox.Enabled = false;
                                    FileTypeTextbox.Enabled = false;
                            
                                    BgWorker.RunWorkerAsync(DictData);
                                }
                            }
                        }

                    }

                

        }

        




        private void BgWorkerClean_DoWork(object sender, DoWorkEventArgs e)
        {


            DictionaryData DictData = (DictionaryData)e.Argument;


            //selects the text encoding based on user selection

            Encoding OutputSelectedEncoding = null;
            this.Invoke((MethodInvoker)delegate ()
            {
                OutputSelectedEncoding = Encoding.GetEncoding(OutputEncodingDropdown.SelectedItem.ToString());
            });

            

            //get the list of files
            var SearchDepth = SearchOption.TopDirectoryOnly;
            if (ScanSubfolderCheckbox.Checked)
            {
                SearchDepth = SearchOption.AllDirectories;
            }
            var files = Directory.EnumerateFiles(DictData.TextFileFolder, DictData.FileExtension, SearchDepth);



            //try {

                

                using (StreamWriter outputFile = new StreamWriter(new FileStream(DictData.OutputFileLocation, FileMode.Create), OutputSelectedEncoding))
                {


                    outputFile.WriteLine("\"Filename\",\"Created\",\"FileSizeKB\",\"Encoding\"");

                    //add some CODE TO WRITE THE HEADER FOR YOUR CSV FILE HERE!!!!




                    foreach (string fileName in files) {
                    
                    
                    
                        //set up our variables to report
                        string Filename_Clean = Path.GetFileName(fileName);

                        //report what we're working on
                        FilenameLabel.Invoke((MethodInvoker)delegate {
                            FilenameLabel.Text = "Analyzing: " + Filename_Clean;
                            FilenameLabel.Invalidate();
                            FilenameLabel.Update();
                            FilenameLabel.Refresh();
                            Application.DoEvents();
                        });


                        string[] OutputString = new string[4];

                        FileInfo oFileInfo = new FileInfo(fileName);
                        string FileEncodingDetected = ExamineTXT.SimpleHelpers.FileEncoding.DetectFileEncoding(fileName);

                        string DetectedEncodingString = "[UNKNOWN]";

                        if(FileEncodingDetected != null)
                        {
                            DetectedEncodingString = FileEncodingDetected;
                        }
                        
                        OutputString[0] = fileName;
                        OutputString[1] = oFileInfo.CreationTime.ToString();
                        OutputString[2] = (oFileInfo.Length / 1024.0).ToString("#.##");
                        OutputString[3] = DetectedEncodingString;
                        

                        
                        
                        outputFile.WriteLine("\"" + string.Join("\",\"", OutputString) + "\"");



                   }

                }

            //}
            //catch
            //{
            //    MessageBox.Show("ExamineTXT encountered an issue somewhere while trying to analyze your texts. The most common cause of this is trying to open your output file(s) while the program is still running. Did any of your input files move, or is your output file being opened/modified by another application?", "Error while transcoding", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

            
        }


        //when the bgworker is done running, we want to re-enable user controls and let them know that it's finished
        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StartButton.Enabled = true;
            ScanSubfolderCheckbox.Enabled = true;
            FileTypeTextbox.Enabled = true;
            FilenameLabel.Text = "Finished!";
            MessageBox.Show("ExamineTXT has finished analyzing your texts.", "Transcode Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }







        public class DictionaryData
        {

            public string TextFileFolder { get; set; }
            public string OutputFileLocation { get; set; }
            public string FileExtension { get; set; }

        }


    }
    


}
