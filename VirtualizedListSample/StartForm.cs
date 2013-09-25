using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VirtualizedListSample
{
    public partial class StartForm : Form
    {
        public StartForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            frmMain frm = new frmMain();
            frm.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            frmTestWeakCache frm = new frmTestWeakCache();
            frm.ShowDialog();
        }
    }
}
