using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime;
using System.Text.RegularExpressions;
using Ntk.ToolsProject.Windows.AvahangHelper.Models;
using Ntk.ToolsProject.Windows.AvahangHelper.Poco;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using TagLib.Id3v2;
using static System.Net.Mime.MediaTypeNames;

namespace Ntk.ToolsProject.Windows.AvahangHelper
{



    public partial class FormMain : Form
    {
        public FormMain()
        {

            InitializeComponent();
            var assem = Assembly.GetExecutingAssembly();
            var aName = assem.GetName();
            //assem.GetName().Version;
            this.Text = this.Text + " (" + assem.GetName().Version + ") ";
        }

        readonly bool configDemo = false;
        readonly string configFolderTools = "tools";
        readonly string configTitle = "avahang";
        readonly string configDomain = "Avahang.com";
        readonly string configSingleAlbum = "single";
        readonly string configSiteUrl = "https://avahang.com";
        private int configImageWidth = 800;
        private int configImageHeight = 600;
        OpenFileDialog folderBrowser = new OpenFileDialog();
        CancellationToken token;
        private int CountActionFolderAll = 0;
        private int CountActionFolderRename = 0;
        private int CountActionFilesAll = 0;
        private int CountActionFilesRename = 0;
        private int CountActionCreate128BitFrom320Bit = 0;
        private int CountActionChangeMp3Tag = 0;
        private int CountActionChangeMp3Image = 0;
        private int CountActionCreateWeaveJson = 0;

        private int CountActionCreate128BitFrom320BitError = 0;
        private int CountActionChangeMp3TagError = 0;
        private int CountActionChangeMp3ImageError = 0;
        private int CountActionCreateWeaveJsonError = 0;

        public List<ActionStatusModel> ActionLogList { get; set; }
        static object lockprogressBarActionUpdate = new object();
        static object lockAddList = new object();

        private void MessageBoxShow(string text, string caption, MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            MessageBox.Show(text, caption, buttons);
        }

        internal static Version GetAssemblyVersion()
        {
            var assem = Assembly.GetExecutingAssembly();
            var aName = assem.GetName();
            return assem.GetName().Version;
        }

        private void buttonActionFolderSelect_Click(object sender, EventArgs e)
        {

            // Set validate names and check file exists to false otherwise windows will
            // not let you select "Folder Selection."
            folderBrowser.ValidateNames = false;
            folderBrowser.CheckFileExists = false;
            folderBrowser.CheckPathExists = true;
            // Always default to Folder Selection.
            folderBrowser.FileName = "Folder Selection.";
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                textBoxFolderPath.Text = Path.GetDirectoryName(folderBrowser.FileName);
                // ...
            }
        }

        private async void buttonActionAvahagFull_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxFolderPath.Text))
            {
                buttonActionFolderSelect_Click(sender, e);
            }

            if (string.IsNullOrEmpty(textBoxFolderPath.Text))
            {
                MessageBoxShow("دوست نداری فولدر انتخاب کنی چرا اجرا را می زنی", "هشدار");
                return;
            }

            textBoxFolderPath.Enabled = false;
            buttonActionAvahagFull.Enabled = false;
            buttonActionFolderSelect.Enabled = false;
            ActionLogList = new List<ActionStatusModel>();

            #region Count Ziro

            CountActionFolderAll = 0;
            CountActionFolderRename = 0;
            CountActionFilesAll = 0;
            CountActionFilesRename = 0;

            CountActionCreate128BitFrom320Bit = 0;
            CountActionChangeMp3Tag = 0;
            CountActionChangeMp3Image = 0;
            CountActionCreateWeaveJson = 0;

            CountActionCreate128BitFrom320BitError = 0;
            CountActionChangeMp3TagError = 0;
            CountActionChangeMp3ImageError = 0;
            CountActionCreateWeaveJsonError = 0;

            #endregion Count

            textBoxFolderAllCount.Text = CountActionFolderAll.ToString();
            textBoxFolderRenameCount.Text = CountActionFolderRename.ToString();

            textBoxFileAllCount.Text = CountActionFilesAll.ToString();
            textBoxFileRenameCount.Text = CountActionFilesRename.ToString();

            textBoxCreate128BitFrom320BitCount.Text = CountActionCreate128BitFrom320Bit.ToString();
            textBoxCountActionChangeMp3TagCount.Text = CountActionChangeMp3Tag.ToString();
            textBoxChangeMp3ImageCount.Text = CountActionChangeMp3Image.ToString();
            textBoxCreateWeaveJsonCount.Text = CountActionCreateWeaveJson.ToString();

            textBoxCreate128BitFrom320BitErrorCount.Text = CountActionCreate128BitFrom320BitError.ToString();
            textBoxCountActionChangeMp3TagErrorCount.Text = CountActionChangeMp3TagError.ToString();
            textBoxChangeMp3ImageErrorCount.Text = CountActionChangeMp3ImageError.ToString();
            textBoxCreateWeaveJsonErrorCount.Text = CountActionCreateWeaveJsonError.ToString();

            progressBarAction.Minimum = 0;
            progressBarAction.Maximum = 0;
            progressBarAction.Value = 0;
            //
            progressBarRename.Minimum = 0;
            progressBarRename.Maximum = 0;
            progressBarRename.Value = 0;
            //
            progressBar128b.Minimum = 0;
            progressBar128b.Maximum = 0;
            progressBar128b.Value = 0;
            //
            progressBarTag.Minimum = 0;
            progressBarTag.Maximum = 0;
            progressBarTag.Value = 0;
            //
            progressBarImage.Minimum = 0;
            progressBarImage.Maximum = 0;
            progressBarImage.Value = 0;
            //
            progressBarCreateWeaveJson.Minimum = 0;
            progressBarCreateWeaveJson.Maximum = 0;
            progressBarCreateWeaveJson.Value = 0;
            //

            labelProgressBarAction.Text = progressBarAction.Value + @"/" + progressBarAction.Maximum;
            labelProgressBarRename.Text = progressBarRename.Value + @"/" + progressBarRename.Maximum;
            labelProgressBar128b.Text = progressBar128b.Value + @"/" + progressBar128b.Maximum;
            labelProgressBarTag.Text = progressBarTag.Value + @"/" + progressBarTag.Maximum;
            labelProgressBarImage.Text = progressBarImage.Value + @"/" + progressBarImage.Maximum;
            labelProgressBarCreateWeaveJson.Text =
                progressBarCreateWeaveJson.Value + @"/" + progressBarCreateWeaveJson.Maximum;


            Task.Run(() => RunMethod(token));

        }

        private async void RunMethod(CancellationToken token)
        {
            var dirMainInfo = new DirectoryInfo(textBoxFolderPath.Text);
            if (!dirMainInfo.Exists)
            {
                MessageBoxShow("دوست نداری فولدر انتخاب کنی چرا اجرا را می زنی", "هشدار");
                return;
            }

            ActioRenameDirectories(textBoxFolderPath.Text);
            //ActionRenameImageFiles(textBoxFolderPath.Text);
            //ActionRenameMp3Files(textBoxFolderPath.Text);

            var ext = new List<string> {"jpg", "gif", "png"};
            ActionRenameFiles(textBoxFolderPath.Text, ext);
            ext = new List<string> {"mp3"};
            ActionRenameFiles(textBoxFolderPath.Text, ext);



            MessageBoxShow("انجام شد", "هشدار");
            Invoke(new Action(() =>
            {
                textBoxFolderPath.Enabled = true;
                buttonActionAvahagFull.Enabled = true;
                buttonActionFolderSelect.Enabled = true;
            }));

        }

        private void ActionRenameFiles(string strDirPath, List<string> ext)
        {

            var fileList = Directory
                .EnumerateFiles(strDirPath, "*.*", SearchOption.AllDirectories)
                .Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant())).ToList();

            //var fileList = Directory.GetFiles(textBoxFolderPath.Text, "*.mp3", SearchOption.AllDirectories).ToList();
            if (fileList == null || fileList.Count == 0)
            {
                MessageBoxShow("فایل  یافت نشد", "هشدار");
                return;
            }

            CountActionFilesAll = fileList.Count;
            Invoke(new Action(() => { textBoxFileAllCount.Text = CountActionFilesAll.ToString(); }));
            foreach (var item in fileList)
            {
                var fileSelect = new FileInfo(item);
                if (!fileSelect.Exists)
                    continue;

                var strType = getTypeFileName(fileSelect.Name).ToLower();
                var actionLog = new ActionStatusModel(ProcessType.Rename, fileSelect.FullName);
                lock (lockAddList)
                {
                    ActionLogList.Add(actionLog);
                }

                var index = ActionLogList.FindIndex(x => x.Id == actionLog.Id);
                Task.Run(() => { progressBarActionUpdate(true, false, ProcessType.Rename); });


                int bitrate = 0;
                try
                {
                    if (strType == "mp3")
                    {
                        var tfile = TagLib.File.Create(fileSelect.FullName);
                        bitrate = tfile.Properties.AudioBitrate;
                    }

                    #region rename

                    var newName = CorectFileName(fileSelect.Name);
                    if (fileSelect.Directory.Name.ToLower() != configSingleAlbum &&
                        newName.IndexOf(fileSelect.Directory.Name.ToLower(), StringComparison.Ordinal) < 0)
                        newName = fileSelect.Directory.Name + "-" + CorectFileName(newName);

                    if (fileSelect.Directory.Parent.Name.IndexOf(":") < 0 &&
                        newName.IndexOf(fileSelect.Directory.Parent.Name.ToLower(), StringComparison.Ordinal) != 0)
                        newName = fileSelect.Directory.Parent.Name + "-" + CorectFileName(newName);
                    if (bitrate > 0)
                    {
                        if (newName.IndexOf("-" + bitrate + "." + strType, StringComparison.Ordinal) < 0)
                            newName = CorectFileName(newName).Replace("." + strType, "-" + bitrate + "." + strType);

                        if (newName.IndexOf("-" + configDomain.ToLower().Replace(".", "-"), StringComparison.Ordinal) <
                            0)
                            newName = CorectFileName(newName).Replace("-" + bitrate + "." + strType,
                                "-" + configDomain.ToLower().Replace(".", "-") + "-" + bitrate + "." + strType);
                    }

                    var fRename = new FileInfo(Path.Combine(fileSelect.DirectoryName, newName));
                    if (!fRename.Exists && fileSelect.FullName != fRename.FullName)
                    {
                        try
                        {
                            if (!configDemo)
                                File.Move(fileSelect.FullName, fRename.FullName);
                            CountActionFilesRename++;
                            Invoke(new Action(() =>
                            {
                                textBoxFileRenameCount.Text = CountActionFilesRename.ToString();
                            }));
                        }
                        catch (Exception exception)
                        {
                            ActionLogList[index].Error = exception.Message;
                        }
                    }

                    fileSelect = new FileInfo(fRename.FullName);

                    #endregion rename
                }
                catch (Exception exception)
                {
                    ActionLogList[index].Error = exception.Message;
                }

                ActionLogList[index].CompleteStatus = true;
                Task.Run(() => { progressBarActionUpdate(false, true, ProcessType.Rename); });

                if (strType == "mp3")
                {
                    Task.Run(() => { ActionMp3ImageEdit(fileSelect); });
                    if (bitrate >= 320)
                    {
                        Task.Run(() => { ActionMp3Create128BitFrom320BitFiles(fileSelect); });
                    }
                }
                else if (strType == "jpg" || strType == "gif" || strType == "png")
                {
                    Task.Run(() => { ActionImageCreateWebp(fileSelect); });
                }


            }
        }

      
        private void ActioRenameDirectories(string strDirPath)
        {
            var dirMainInfo = new DirectoryInfo(strDirPath);
            var dirMainRenameInfo =
                new DirectoryInfo(Path.Combine(dirMainInfo.Parent.FullName, CorectFolderName(dirMainInfo.Name)));
            
            try
            {
                if (dirMainInfo.FullName != dirMainRenameInfo.FullName)
                    Directory.Move(dirMainInfo.FullName, dirMainRenameInfo.FullName);
                strDirPath = dirMainRenameInfo.FullName;
            }
            catch (Exception e)
            {
                strDirPath = dirMainInfo.FullName;
            }
            

            textBoxFolderPath.Text = strDirPath;


            var mp3Dir = Directory.GetDirectories(strDirPath, "", SearchOption.TopDirectoryOnly).ToList();
            CountActionFolderAll = CountActionFolderAll + mp3Dir.Count;
            Invoke(new Action(() => { textBoxFolderAllCount.Text = CountActionFolderAll.ToString(); }));
            foreach (var itemDic in mp3Dir)
            {
                var dirInfo = new DirectoryInfo(itemDic);
                var renameDirInfo =
                    new DirectoryInfo(Path.Combine(dirInfo.Parent.FullName, CorectFolderName(dirInfo.Name)));

                try
                {
                    if (dirInfo.FullName != renameDirInfo.FullName)
                    {
                        if (!configDemo)
                            Directory.Move(dirInfo.FullName, renameDirInfo.FullName);
                        CountActionFolderRename++;
                        Invoke(new Action(() =>
                        {
                            textBoxFolderRenameCount.Text = CountActionFolderRename.ToString();
                        }));
                    }

                    ActioRenameDirectories(renameDirInfo.FullName);
                }
                catch (Exception exception)
                {
                }

            }
        }

        private async Task ActionMp3Create128BitFrom320BitFiles(FileInfo item)
        {
            if (!item.Exists)
                return;
            var actionLog = new ActionStatusModel(ProcessType.Create128b, item.FullName);
            lock (lockAddList)
            {
                ActionLogList.Add(actionLog);
            }

            var index = ActionLogList.FindIndex(x => x.Id == actionLog.Id);
            Task.Run(() => { progressBarActionUpdate(true, false, ProcessType.Create128b); });

            var fileCorrect = new FileInfo(Path.Combine(item.DirectoryName,
                CorectFileName(item.Name).Replace("-320.mp3", "-128.mp3")));
            if (!fileCorrect.Exists)
            {
                try
                {
                    var commandStr = "ffmpeg -i " + item.FullName +
                                     " -codec:a libmp3lame  -loglevel quiet  -b:a 128k -map_metadata 0 -id3v2_version 3 " +
                                     fileCorrect.FullName;
                    var commandHelper = new CommandHelper();
                    var retC = "";
                    if (!configDemo)
                        retC = commandHelper.ExecuteCommand(commandStr,
                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFolderTools), 0);
                    if (string.IsNullOrEmpty(commandHelper.Error))
                    {
                        CountActionCreate128BitFrom320Bit++;
                        Invoke(new Action(() =>
                        {
                            textBoxCreate128BitFrom320BitCount.Text = CountActionCreate128BitFrom320Bit.ToString();
                        }));
                    }
                    else
                    {
                        CountActionCreate128BitFrom320BitError++;
                        Invoke(new Action(() =>
                        {
                            textBoxCreate128BitFrom320BitErrorCount.Text =
                                CountActionCreate128BitFrom320BitError.ToString();
                        }));
                    }
                }
                catch (Exception exception)
                {
                    CountActionCreate128BitFrom320BitError++;
                    Invoke(new Action(() =>
                    {
                        textBoxCreate128BitFrom320BitErrorCount.Text =
                            CountActionCreate128BitFrom320BitError.ToString();
                    }));

                    ActionLogList[index].Error = exception.Message;
                }
            }

            ActionLogList[index].CompleteStatus = true;
            Task.Run(() => { progressBarActionUpdate(false, true, ProcessType.Create128b); });
            Task.Run(() =>
            {
                ActionMp3TagEdit(item);
                ActionCreateWeaveJson(item);
            });



        }

        private async Task ActionImageCreateWebp(FileInfo item)
        {
            if (!item.Exists)
                return;
            var actionLog = new ActionStatusModel(ProcessType.CreateImageWbm, item.FullName);
            lock (lockAddList)
            {
                ActionLogList.Add(actionLog);
            }

            var index = ActionLogList.FindIndex(x => x.Id == actionLog.Id);
            Task.Run(() => { progressBarActionUpdate(true, false, ProcessType.CreateImageWbm); });

            var strType = getTypeFileName(item.Name);

            var fileCorrect = new FileInfo(Path.Combine(item.DirectoryName,
                CorectFileName(item.Name).Replace("." + strType, ".webp")));
            if (!fileCorrect.Exists)
            {
                try
                {

                    // Load image from file
                    using (var image = SixLabors.ImageSharp.Image.Load(item.FullName))
                    {
                        // Change the size of the image
                        image.Mutate(x => x.Resize(new SixLabors.ImageSharp.Size(configImageWidth, configImageHeight)));

                        // Save the modified image
                        image.Save(fileCorrect.FullName);
                    }


                }
                catch (Exception exception)
                {

                    ActionLogList[index].Error = exception.Message;
                }
            }

            ActionLogList[index].CompleteStatus = true;
            Task.Run(() => { progressBarActionUpdate(false, true, ProcessType.CreateImageWbm); });
        }

        private async Task ActionCreateWeaveJson(FileInfo item)
        {
            if (!item.Exists)
                return;
            var actionLog = new ActionStatusModel(ProcessType.CreateWeaveJson, item.FullName);
            lock (lockAddList)
            {
                ActionLogList.Add(actionLog);
            }

            var index = ActionLogList.FindIndex(x => x.Id == actionLog.Id);
            Task.Run(() => { progressBarActionUpdate(true, false, ProcessType.CreateWeaveJson); });
            var fileCorrect =
                new FileInfo(Path.Combine(item.DirectoryName, CorectFileName(item.Name).Replace(".mp3", ".json")));
            if (!fileCorrect.Exists)
            {
                try
                {
                    var commandStr = "audiowaveform -i " + item.FullName + " --pixels-per-second 10 -b 8 -o " +
                                     fileCorrect.FullName;
                    var commandHelper = new CommandHelper();
                    var retC = "";
                    if (!configDemo)
                        retC = commandHelper.ExecuteCommand(commandStr,
                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFolderTools), 0);
                    if (string.IsNullOrEmpty(commandHelper.Error))
                    {
                        CountActionCreateWeaveJson++;
                        Invoke(new Action(() =>
                            {
                                textBoxCreateWeaveJsonCount.Text = CountActionCreateWeaveJson.ToString();
                            }
                        ));
                    }
                    else
                    {
                        CountActionCreateWeaveJsonError++;
                        Invoke(new Action(() =>
                            {
                                textBoxCreateWeaveJsonErrorCount.Text = CountActionCreateWeaveJsonError.ToString();
                            }
                        ));
                    }
                }
                catch (Exception exception)
                {
                    CountActionCreateWeaveJsonError++;
                    Invoke(new Action(() =>
                        {
                            textBoxCreateWeaveJsonErrorCount.Text = CountActionCreateWeaveJsonError.ToString();
                        }
                    ));

                    ActionLogList[index].Error = exception.Message;

                }
            }

            ActionLogList[index].CompleteStatus = true;
            Task.Run(() => { progressBarActionUpdate(false, true, ProcessType.CreateWeaveJson); });
        }

        private async Task ActionMp3ImageEdit(FileInfo item)
        {
            if (!item.Exists)
                return;
            lock (item.FullName)
            {

                var actionLog = new ActionStatusModel(ProcessType.Image, item.FullName);
                lock (lockAddList)
                {
                    ActionLogList.Add(actionLog);
                }

                var index = ActionLogList.FindIndex(x => x.Id == actionLog.Id);
                Task.Run(() => { progressBarActionUpdate(true, false, ProcessType.Image); });

                var tfile = TagLib.File.Create(item.FullName);
                string title = tfile.Tag.Title;
                TimeSpan duration = tfile.Properties.Duration;
                var bitrate = tfile.Properties.AudioBitrate;
                TagLib.Id3v2.Tag.DefaultVersion = 3;
                TagLib.Id3v2.Tag.ForceDefaultVersion = true;

                #region image

                var ext = new List<string> {"jpg", "gif", "png"};
                var imageFiles = Directory
                    .EnumerateFiles(item.Directory.FullName, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant())).ToList();
                if (imageFiles != null && imageFiles.Count > 0)
                {

                    var fileSelectImagePath = imageFiles[0];
                    if (imageFiles.Any(x => item.Name.IndexOf(x) >= 0))
                        fileSelectImagePath = imageFiles.FirstOrDefault(x => item.Name.IndexOf(x) >= 0);
                    var fileSelectImage = new FileInfo(fileSelectImagePath);
                    if (fileSelectImage.Exists)
                    {
                        try
                        {
                            Bitmap watermark = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                configFolderTools,
                                @"watermark.png"));
                            Bitmap bitmap = new Bitmap(fileSelectImage.FullName);
                            bitmap = FileHelper.WatermarkImage(bitmap, watermark);
                            //bitmap.Save(@"C:/watermarkedImage.png");

                            AttachedPictureFrame cover = new TagLib.Id3v2.AttachedPictureFrame
                            {
                                Type = TagLib.PictureType.FrontCover,
                                Description = "Cover",
                                MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg,
                                //Data = File.ReadAllBytes(fileSelectImage.FullName),
                                Data = bitmap.ToByteArray(ImageFormat.Jpeg),
                                TextEncoding = TagLib.StringType.UTF16
                            };
                            tfile.Tag.Pictures = new TagLib.IPicture[] {cover};
                            if (!configDemo)
                                tfile.Save();
                            CountActionChangeMp3Image++;
                            Invoke(new Action(() =>
                            {
                                textBoxChangeMp3ImageCount.Text = CountActionChangeMp3Image.ToString();
                            }));

                        }
                        catch (Exception exception)
                        {
                            CountActionChangeMp3ImageError++;
                            Invoke(new Action(() =>
                            {
                                textBoxChangeMp3ImageErrorCount.Text = CountActionChangeMp3ImageError.ToString();
                            }));
                            ActionLogList[index].Error = exception.Message;

                        }
                    }
                }
                else if (tfile.Tag.Pictures != null && tfile.Tag.Pictures.Length > 0)
                {
                    try
                    {
                        Bitmap watermark = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                            configFolderTools,
                            @"watermark.png"));
                        // Load you image data in MemoryStream
                        TagLib.IPicture pic = tfile.Tag.Pictures[0];
                        MemoryStream ms = new MemoryStream(pic.Data.Data);
                        ms.Seek(0, SeekOrigin.Begin);

                        // ImageSource for System.Windows.Controls.Image
                        Bitmap bitmap = new Bitmap(ms);

                        bitmap = FileHelper.WatermarkImage(bitmap, watermark);
                        //bitmap.Save(@"C:/watermarkedImage.png");

                        AttachedPictureFrame cover = new TagLib.Id3v2.AttachedPictureFrame
                        {
                            Type = TagLib.PictureType.FrontCover,
                            Description = "Cover",
                            MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg,
                            //Data = File.ReadAllBytes(fileSelectImage.FullName),
                            Data = bitmap.ToByteArray(ImageFormat.Jpeg),
                            TextEncoding = TagLib.StringType.UTF16
                        };
                        tfile.Tag.Pictures = new TagLib.IPicture[] {cover};
                        if (!configDemo)
                            tfile.Save();
                        CountActionChangeMp3Image++;
                        Invoke(new Action(() =>
                        {
                            textBoxChangeMp3ImageCount.Text = CountActionChangeMp3Image.ToString();
                        }));

                    }
                    catch (Exception exception)
                    {
                        CountActionChangeMp3ImageError++;
                        Invoke(new Action(() =>
                        {
                            textBoxChangeMp3ImageErrorCount.Text = CountActionChangeMp3ImageError.ToString();
                        }));
                        ActionLogList[index].Error = exception.Message;

                    }
                }

                #endregion

                ActionLogList[index].CompleteStatus = true;
                Task.Run(() => { progressBarActionUpdate(false, true, ProcessType.Image); });
                Task.Run(() => { ActionMp3TagEdit(item); });
            }
        }

        private async Task ActionMp3TagEdit(FileInfo item)
        {
            if (!item.Exists)
                return;
            lock (item.FullName)
            {

                var actionLog = new ActionStatusModel(ProcessType.Tag, item.FullName);
                lock (lockAddList)
                {
                    ActionLogList.Add(actionLog);
                }

                var index = ActionLogList.FindIndex(x => x.Id == actionLog.Id);
                Task.Run(() => { progressBarActionUpdate(true, false, ProcessType.Tag); });

                var tfile = TagLib.File.Create(item.FullName);

                #region info

                if (item.Directory.Parent != null && item.Directory.Parent.Name.IndexOf(":") < 0)
                {
                    tfile.Tag.Genres = new string[] {configDomain};
                    tfile.Tag.Description = configSiteUrl;
                    tfile.Tag.Comment = configDomain;

                    tfile.Tag.Title = CorrectTitleMp3
                    (    item.Name.Replace(item.Directory.Parent.Name, "").Replace(item.Directory.Name, " ") 
                        );
                    
                    tfile.Tag.Artists = new string[] {item.Directory.Parent.Name.Replace("-", " ")};
                    tfile.Tag.AlbumArtists = new string[] {item.Directory.Parent.Name.Replace("-", " ")};
                    tfile.Tag.Album = item.Directory.Name.Replace("-", " ");
                }
                else
                {
                    tfile.Tag.Genres = new string[] {configDomain};
                    tfile.Tag.Description = configSiteUrl;
                    tfile.Tag.Comment = configDomain;

                    tfile.Tag.Title = CorrectTitleMp3(item.Name.Replace(item.Directory.Name, " "));
                    tfile.Tag.Artists = new string[] {item.Directory.Name.Replace("-", " ")};
                    tfile.Tag.AlbumArtists = new string[] {item.Directory.Name.Replace("-", " ")};
                    tfile.Tag.Album = item.Directory.Name.Replace("-", " ");
                }

                try
                {

                    if (!configDemo)
                        tfile.Save();
                    CountActionChangeMp3Tag++;
                    Invoke(new Action(() =>
                    {
                        textBoxCountActionChangeMp3TagCount.Text = CountActionChangeMp3Tag.ToString();
                    }));
                }
                catch (Exception exception)
                {
                    CountActionChangeMp3TagError++;
                    Invoke(new Action(() =>
                    {
                        textBoxCountActionChangeMp3TagErrorCount.Text = CountActionChangeMp3TagError.ToString();
                    }));
                    ActionLogList[index].Error = exception.Message;
                }

                #endregion

                ActionLogList[index].CompleteStatus = true;
                Task.Run(() => { progressBarActionUpdate(false, true, ProcessType.Tag); });
                Task.Run(() => { ActionCreateWeaveJson(item); });
            }
        }


        private string getTypeFileName(string str)
        {
            str = str.ToLower();
            var strType = "";
            if (str.LastIndexOf(".") >= 0 && str.LastIndexOf(".") < str.Length - 1)
            {
                strType = str.Substring(str.LastIndexOf(".") + 1);
            }

            return strType;
        }

        private string CorrectTitleMp3(string str)
        {
            var indexDomain = str.ToLower().IndexOf(configDomain.ToLower());
            if (indexDomain > 0)
            {
                str = str.Substring(0, indexDomain - 1);
            }

            indexDomain = str.ToLower().IndexOf(configDomain.ToLower().Replace(".", "-"));
            if (indexDomain > 0)
            {
                str = str.Substring(0, indexDomain - 1);
            }

            while (str.IndexOf("-")>=0)
            {
                str=str.Replace("-", " ");
            }

            while (str.IndexOf("  ")>=0)
            {
                str=str.Replace("  ", " ");
            }
            return str.Trim()+" "+ configTitle;
        }
        private string CorectFileName(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "";
            str = str.ToLower();
            var strType = "";
            if (str.LastIndexOf(".") >= 0)
            {
                strType = str.Substring(str.LastIndexOf("."));
                str = str.Substring(0, str.LastIndexOf("."));
            }

            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            str = rgx.Replace(str, " ");
            if (string.IsNullOrEmpty(str))
                return "";

            while (str.IndexOf(" ", StringComparison.Ordinal) >= 0)
            {
                str = str.Replace(" ", "-");
            }

            while (str.IndexOf("--", StringComparison.Ordinal) >= 0)
            {
                str = str.Replace("--", "-");
            }

            if (str.LastIndexOf("-") == str.Length - 1)
                str = str.Substring(0, str.Length - 1);
            if (str.IndexOf("-") == 0)
                str = str.Substring(1);
            return str + strType;
        }

        private string CorectFolderName(string str)
        {

            if (string.IsNullOrEmpty(str))
                return "";
            str = str.ToLower();
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            str = rgx.Replace(str, " ");
            if (string.IsNullOrEmpty(str))
                return "";

            while (str.IndexOf(" ", StringComparison.Ordinal) >= 0)
            {
                str = str.Replace(" ", "-");
            }

            while (str.IndexOf("--", StringComparison.Ordinal) >= 0)
            {
                str = str.Replace("--", "-");
            }

            if (str.LastIndexOf("-") == str.Length - 1)
                str = str.Substring(0, str.Length - 1);
            if (str.IndexOf("-") == 0)
                str = str.Substring(1);
            return str;
        }

        private void progressBarActionUpdate(bool MaximumAdd, bool DoAdd, ProcessType pType)
        {
            lock (lockprogressBarActionUpdate)
            {
                Invoke(new Action(() =>
                {
                    if (MaximumAdd)
                        progressBarAction.Maximum++;
                    if (DoAdd)
                    {
                        if ((progressBarAction.Value + 1) > progressBarAction.Maximum)
                            progressBarAction.Maximum = (progressBarAction.Value + 1);
                        progressBarAction.Value++;
                        
                    }
                    Invoke(new Action(() =>
                    {
                        labelProgressBarAction.Text = progressBarAction.Value + "/" + progressBarAction.Maximum;
                    }));




                    switch (pType)
                    {
                        case ProcessType.Rename:
                            //
                            if (MaximumAdd)
                                progressBarRename.Maximum++;
                            if (DoAdd)
                            {
                                if ((progressBarRename.Value + 1) > progressBarRename.Maximum)
                                    progressBarRename.Maximum=(progressBarRename.Value + 1);
                                progressBarRename.Value++;
                            }
                            Invoke(new Action(() =>
                            {
                                labelProgressBarRename.Text =
                                    progressBarRename.Value + @"/" + progressBarRename.Maximum;
                            }));
                            break;
                        case ProcessType.Create128b:
                            //
                            if (MaximumAdd)
                                progressBar128b.Maximum++;
                            if (DoAdd)
                            {
                                if ((progressBar128b.Value + 1) > progressBar128b.Maximum)
                                    progressBar128b.Maximum = (progressBar128b.Value + 1);
                                progressBar128b.Value++;
                            }

                            Invoke(new Action(() =>
                            {
                                labelProgressBar128b.Text = progressBar128b.Value + @"/" + progressBar128b.Maximum;
                            }));
                            break;
                        case ProcessType.Tag:
                            //
                            if (MaximumAdd)
                                progressBarTag.Maximum++;
                            if (DoAdd)
                            {
                                if ((progressBarTag.Value + 1) > progressBarTag.Maximum)
                                    progressBarTag.Maximum = (progressBarTag.Value + 1);
                                progressBarTag.Value++;
                            }

                            Invoke(new Action(() =>
                            {
                                labelProgressBarTag.Text = progressBarTag.Value + @"/" + progressBarTag.Maximum;
                            }));
                            break;
                        case ProcessType.Image:
                            //
                            if (MaximumAdd)
                                progressBarImage.Maximum++;
                            if (DoAdd)
                            {
                                if ((progressBarImage.Value + 1) > progressBarImage.Maximum)
                                    progressBarImage.Maximum = (progressBarImage.Value + 1);
                                progressBarImage.Value++;
                            }
                            
                            Invoke(new Action(() =>
                            {
                                labelProgressBarImage.Text =
                                    progressBarImage.Value + @"/" + progressBarImage.Maximum;
                            }));
                            break;
                        case ProcessType.CreateWeaveJson:
                            //
                            if (MaximumAdd)
                                progressBarCreateWeaveJson.Maximum++;
                            if (DoAdd)
                            {
                                if ((progressBarCreateWeaveJson.Value + 1) > progressBarCreateWeaveJson.Maximum)
                                    progressBarCreateWeaveJson.Maximum = (progressBarCreateWeaveJson.Value + 1);
                                progressBarCreateWeaveJson.Value++;
                            }
                            
                            Invoke(new Action(() =>
                            {
                                labelProgressBarCreateWeaveJson.Text = progressBarCreateWeaveJson.Value + @"/" +
                                                                       progressBarCreateWeaveJson.Maximum;
                            }));
                            break;
                    }




                    dataGridViewActionLog.DataSource = null;
                    dataGridViewActionLog.DataSource = ActionLogList;

                }));


            }
        }

    }
}