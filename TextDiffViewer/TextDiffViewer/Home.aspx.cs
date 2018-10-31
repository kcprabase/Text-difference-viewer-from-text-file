using my.utils;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Hosting;
using System.Net;

namespace TextDiffViewer
{
    public partial class Home : System.Web.UI.Page
    {
        private string oldFileName;
        private string newFileName;
        private string ViewType;
        private string fileName;
        private string[] aLines;
        private string[] bLines;
        private Diff.Item[] f;
        private Utility utl = new Utility();
        private string time;
        private Stopwatch s = new Stopwatch();
        private string logPath = Path.Combine(HttpRuntime.AppDomainAppPath, "log.txt");
        private bool readRemoteFile = false;
        private bool noError = false;

        public int lineCount;
        public int lastId = 0;
        public bool diffOnly;

        protected void Page_Load(object sender, EventArgs e)
        {            
            if (!IsPostBack) {
            oldFileName = "Original.txt";
            newFileName = "Changed.txt";
                //oldFileName = "Diff.cs.v1";
                //    newFileName = "Diff.cs.v2";
            ViewType = "inline";
            //    Compare();
            }
                      
        }

        /// <summary>
        /// Takes two filenames saved in string variables, oldfilename and newfilename 
        /// finds them in CompareFiles Directory on server,
        /// reads them to end
        /// sends to diff.cs to get diffdata array which is saved in f
        /// calls function to render difference according to user requirement
        /// invoked from code block embedded in aspx file
        /// </summary>
        public void Compare()
        {
            string a = null, b = null;
            if (noError) { 
            if (readRemoteFile || radioFileMode.SelectedIndex == 1) {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(oldFileName);
                StreamReader reader = new StreamReader(stream);
                a = reader.ReadToEnd();
                stream.Close();
                reader.Close();

                stream = client.OpenRead(newFileName);
                reader = new StreamReader(stream);
                b = reader.ReadToEnd();
                stream.Close();
                reader.Close();
            }
            }
            else { 
            StreamReader aFile;
            fileName = Server.MapPath("~/CompareFiles/" + oldFileName.Replace("\\", "").Replace("/", ""));
            aFile = File.OpenText(fileName);
            a = aFile.ReadToEnd();
            aFile.Close();

            fileName = Server.MapPath("~/CompareFiles/" + newFileName.Replace("\\", "").Replace("/", ""));
            aFile = File.OpenText(fileName);
            b = aFile.ReadToEnd();
            aFile.Close();
            }
            s.Start();
            f = Diff.DiffText(a, b, true, true, false, false);
            lastId = f.Length;
            if (f.Length!=0 && (f[0].StartA == 0 || f[0].StartB == 0))
                lastId--;
            int inserted = 0, deleted = 0;
            for (int i = 0; i < f.Length; i++)
            {
                inserted += f[i].insertedB;
                deleted += f[i].deletedA;
            }
            footer.InnerHtml = "Insertion: " + inserted + "  Deletion: " + deleted;

            s.Stop();
            time = s.Elapsed.ToString();
            Utility.Log(logPath, "Time Spent to create DiffItems: " + time);
//            s.Reset();

            aLines = a.Split('\n');
            bLines = b.Split('\n');
            
             if (ViewType == "inline")
            {
                utl.PrintDiffBody(f, aLines, bLines, diffOnly);
            }

             else if (ViewType == "side")
             {
                 utl.PrintDiffBody(f, aLines, bLines, diffOnly, "side");
             }
 //            spnFirstName.InnerText = time;
 //            readRemoteFile = false;
        }

        /// <summary>
        /// Uploads files chosen in two file upload control for individual comparision
        /// by default sets oldFileName and newFileName to default value
        /// checks different conditions to upload new file or just assign filenames already existing in server 
        /// </summary>
        private void UploadFiles() {
            Utility.Log(logPath, "******************************* New Entry *******************************");
            try
            {
                if ((!fileUploadOld.HasFile && OldTextBoxContent == "") || (!fileUploadNew.HasFile && NewTextBoxContent == ""))
                {
                    oldFileName = "Original.txt";
                    newFileName = "Changed.txt";
                }
                else if (fileUploadOld.HasFile && fileUploadNew.HasFile)
                {
                    oldFileName = SaveFile(fileUploadOld);
                    newFileName = SaveFile(fileUploadNew);
                    OldTextBoxContent = oldFileName;
                    NewTextBoxContent = newFileName;

                }
                else if (fileUploadOld.HasFile)
                {
                    oldFileName = SaveFile(fileUploadOld);
                    newFileName = txtSourceNew.Text;
                    OldTextBoxContent = oldFileName;
                }
                else if (fileUploadNew.HasFile)
                {
                    newFileName = SaveFile(fileUploadNew);
                    oldFileName = txtSourceOld.Text;
                    NewTextBoxContent = newFileName;
                }
                else if (txtSourceOld.Text != "" && txtSourceNew.Text != "")
                {
                    oldFileName = txtSourceOld.Text;
                    newFileName = txtSourceNew.Text;
                }
                else {
                    oldFileName = "Original.txt";
                    newFileName = "Changed.txt";
                }
            }
            catch (Exception)
            {
                lblResult.Text = "Upload Failed";
            }
            BrowseIndividualFiles.Attributes.Add("style", "display:table-row");
            LoadMultipleFiles.Attributes.Add("style", "display:none");
        }
        
        /// <summary>
        /// on click listener for Compare button for individual comparision
        /// checks user requirements to show diff and calls UploadFiles()
        /// </summary>
        protected void btnCompare_Click(object sender, EventArgs e)
        {
            CheckForDiffOnlyAndViewType();
            UploadFiles();
            spnFirstName.InnerText = null;
        }

        /// <summary>
        /// Following 3 getters and setters set and get values from textbox associated with 3 file upload controls in this website
        /// </summary>
        public string OldTextBoxContent
        {
            get
            {
                return txtSourceOld.Text;
            }
            set
            {
                txtSourceOld.Text = value;
            }
        }
        public string NewTextBoxContent
        {
            get
            {
                return txtSourceNew.Text;
            }
            set
            {
                txtSourceNew.Text = value;
            }
        }
        public string LoadTextBoxContent
        {
            get
            {
                return txtSourceLoadFile.Text;
            }
            set
            {
                txtSourceLoadFile.Text = value;
            }
        }
                
        /// <summary>
        /// onChangeListener for RadioButtonList to select inline or side view
        /// </summary>
        protected void OnChangeRadio(object sender, System.EventArgs e)
        {
            CheckForDiffOnlyAndViewType();
            //if (radioFileMode.SelectedValue == "individual")
                UploadFiles();
            //else if (radioFileMode.SelectedValue == "multiple")
            //    ContinueBtnStuffs();
        }
               

    /// <summary>
    /// onCheckChangedListener for checkbox difference only to select difference only
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
        protected void cbDiffOnly_CheckedChanged(object sender, EventArgs e)
        {
            CheckForDiffOnlyAndViewType();
            if (radioFileMode.SelectedValue == "individual")
                UploadFiles();
            else if (radioFileMode.SelectedValue == "multiple")
                ContinueBtnStuffs();
        }

        /// <summary>
        /// onclicklistener for next and previous button for multiple comparision mode
        /// </summary>
        protected void btnContinue_Click(object sender, EventArgs e)
        {
            readRemoteFile = true;
            ContinueBtnStuffs();
        }

        /// <summary>
        /// check for status of radiobtnViewMode and checkbox cbDiffOnly
        /// </summary>
        private void CheckForDiffOnlyAndViewType() {
            ViewType = radioBtnViewMode.SelectedValue;
            if (cbDIffOnly.Checked)
                diffOnly = true;
            else
                diffOnly = false;
        }

        /// <summary>
        /// tasks to be done when multiple comparision mode is selected
        /// </summary>
        private void ContinueBtnStuffs() {
            noError = true;
            int skipNum = Convert.ToInt32(Request.Form["hiddenSkipNum"]);
            spnFirstName.InnerText = "Comparision No. : " + (skipNum - 1); //printed from here instead of clientside so that postback doesnot removes it
            string loadFileName = null;
            if (fileUploadLoad.HasFile)
            {
                loadFileName = Path.GetFileName(fileUploadLoad.FileName);
                fileUploadLoad.SaveAs(Server.MapPath("~/FilesToBeLoaded/") + loadFileName);
                LoadTextBoxContent = loadFileName;
            }
            else if (txtSourceLoadFile.Text != "")
            {
                loadFileName = txtSourceLoadFile.Text;
            }
            else {
                noError = false;
                oldFileName = "noLoadFileError.txt";
                newFileName = "noLoadFileError.txt";
            }

            string line;
            string[] a;
            try
            {
            fileName = Server.MapPath("~/FilesToBeLoaded/" + loadFileName.Replace("\\", "").Replace("/", ""));
            lineCount = File.ReadAllLines(fileName).Length;
            if (skipNum >= lineCount) {
                noError = false;
                spnFirstName.InnerText = "End of Files";
                oldFileName = "noLoadFileError.txt";
                newFileName = "noLoadFileError.txt";
            } else { 
            line = File.ReadLines(fileName).Skip(skipNum).Take(1).First();
            a = line.Split(',');
            
            
                oldFileName = a[0];
                newFileName = a[1];
            }
            }
            catch (Exception ex)
            {
                noError = false;
                spnFirstName.InnerText = "Please Upload a Valid Load File";
                oldFileName = "noLoadFileError.txt";
                newFileName = "noLoadFileError.txt";
            }
            CheckForDiffOnlyAndViewType();
            BrowseIndividualFiles.Attributes.Add("style", "display:none");
            LoadMultipleFiles.Attributes.Add("style","display:table-row");
        }

        /// <summary>
        /// Uploads the file from respective file upload control to a folder in server
        /// Update was made on august 31, 2016 to rename the uploaded files which have same names
        /// with files already present in server
        /// </summary>
        /// <param name="nameFileUpload">html id of file upload control</param>
        /// <returns> filename selected by file upload control. if renamed, returns new name</returns>
        private string SaveFile(FileUpload nameFileUpload) {
            string myFileName = nameFileUpload.FileName;
            
            int count = 1;
            string fullPath = Server.MapPath("~/CompareFiles/") + myFileName;
            string fileNameOnly = Path.GetFileNameWithoutExtension(fullPath);
            string extension = Path.GetExtension(fullPath);
            string path = Path.GetDirectoryName(fullPath);
            string newFullPath = fullPath;

            while (File.Exists(newFullPath))
            {
                string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                myFileName = tempFileName + extension;
                newFullPath = Path.Combine(path, myFileName);
            }
            nameFileUpload.SaveAs(newFullPath);
            int fileSize = nameFileUpload.PostedFile.ContentLength;
            Utility.Log(logPath, "File Uploaded: " + fileName + " Size: " + (fileSize) / 1024 + " KB");
            return myFileName;
        }
    }
}