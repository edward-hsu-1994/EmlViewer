using MimeKit;
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

namespace EmlViewer {
    public partial class MainForm : Form {
        public MainForm(string filePath = null) {
            InitializeComponent();

            if (filePath != null) {
                LoadFile(filePath);
            }
        }

        public void LoadFile(string filePath) {
            var html = FilterScript(File.ReadAllText(Application.StartupPath + @"\assets\template.html"));

            var message = MimeMessage.Load(filePath);

            var varList = typeof(MimeMessage).GetProperties();

            foreach (var pro in varList) {
                if (pro.Name.Contains("Html")) {
                    html = html.Replace("{{" + pro.Name + "}}", FilterScript(pro.GetValue(message)?.ToString() ?? ""));
                    continue;
                }
                html = html
                    .Replace("{{" + pro.Name + "}}", MimeMessageProConvert(pro.GetValue(message)) ?? "");
            }

            #region 內嵌圖片
            var images = message.BodyParts
                .Where(x => x.ContentType.Matches("image", "*"))
                .ToArray();

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.DocumentNode.SelectNodes("//img")?.ToList();
            if (nodes != null) {
                foreach (var node in nodes) {
                    var cid = node.Attributes["src"]?.Value;
                    if (cid == null) continue;
                    if (cid.IndexOf("cid:") == -1) continue;
                    cid = cid.Replace("cid:", "");
                    var image = images.FirstOrDefault(x => x.ContentId == cid);

                    if (image == null) continue;

                    MemoryStream imageTempStream = new MemoryStream();
                    image.WriteTo(imageTempStream);

                    var base64 = Encoding.UTF8.GetString(imageTempStream.ToArray()).Replace("\r", "").Replace("\n", "");
                    base64 = base64.Substring(base64.IndexOf("base64") + 6);
                    node.SetAttributeValue("src", "data:" + image.ContentType.MimeType + ";base64," + base64);
                }
            }
            #endregion

            if (htmlViewer.Document == null) {
                htmlViewer.DocumentText = doc.DocumentNode.OuterHtml;
            } else {
                htmlViewer.Document.Write(doc.DocumentNode.OuterHtml);
            }

            Text = "電子郵件檢視器 - " + filePath.Split('/').Last();
            htmlViewer.Visible = true;
        }

        public static string MimeMessageProConvert(object obj) {
            if (obj == null) return null;

            return WebUtility.HtmlEncode(obj.ToString());
        }

        public string FilterScript(string html) {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.DocumentNode.SelectNodes("//script")?.ToList();

            if (nodes == null) return html;

            foreach (var node in nodes) {
                node.Remove();
            }

            return doc.DocumentNode.OuterHtml;
        }

        private void 結束XToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void 開啟舊檔OToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog op = new OpenFileDialog();
            op.Multiselect = true;
            op.Filter = "電子郵件檔案 (*.eml)|*.eml";
            if (op.ShowDialog() != DialogResult.OK) return;

            if (op.FileNames.Length == 1) {
                LoadFile(op.FileNames[0]);
            } else {
                LoadFile(op.FileNames[0]);
                OpenForms(op.FileNames.Skip(1).ToArray());
            }
        }

        private void 關於AToolStripMenuItem_Click(object sender, EventArgs e) {
            new AboutForm().ShowDialog();
        }
        private void MainForm_DragEnter(object sender, DragEventArgs e) {
            e.Effect = DragDropEffects.Copy;
        }
        private void MainForm_DragDrop(object sender, DragEventArgs e) {
            string[] files = ((string[])e.Data.GetData(DataFormats.FileDrop)).Where(x => IsEml(x)).ToArray();
            if (files.Length == 0) return;
            LoadFile(files[0]);
            foreach (string file in files.Skip(1)) {
                var instance = new MainForm(file);
                instance.Show();
                Program.forms.Add(instance);
            }
        }

        public static bool IsEml(string path) {
            Regex eml = new Regex(@".+\.eml$");
            return eml.IsMatch(path);
        }

        public static void OpenForms(string[] filePaths, bool startUp = false) {
            if (filePaths?.Length == 0) {
                if (startUp) {
                    Program.forms.Add(new MainForm());
                }
            } else {
                Program.forms.AddRange(filePaths.Select(x => new MainForm(x)));
            }
            foreach (var form in Program.forms) form.Show();
        }


        private void MainForm_FormClosed(object sender, FormClosedEventArgs e) {
            Program.forms.Remove(this);
            if (Program.forms.Count == 0) {
                Application.Exit();
            }
        }
    }
}
