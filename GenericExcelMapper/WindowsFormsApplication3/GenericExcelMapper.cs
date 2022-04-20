using System;
using System.Windows.Forms;

namespace GenericExcelMapper
{
    public partial class GenericExcelMapper : Form
    {
        public GenericExcelMapper()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ExcelTemplate cnt = new ExcelTemplate();
            this.Controls.Add(cnt);
        }
    }
}
