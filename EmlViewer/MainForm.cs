using MimeKit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
            var template = File.ReadAllText(@"assets\template.html");
            var message = MimeMessage.Load("D:\\test.eml");

            var varList = typeof(MimeMessage).GetProperties();

            foreach (var pro in varList) {
                template = template
                    .Replace("{{" + pro.Name + "}}", pro.GetValue(message) as string);
            }

            htmlViewer.DocumentText = template;
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
                foreach (var file in op.FileNames.Skip(1)) {
                    new MainForm(file).Show();
                }
            }
        }
    }
}
