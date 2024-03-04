using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SystAnalys_lr1
{
    public partial class FormInput : Form
    {
        public FormInput()
        {
            InitializeComponent();
        }
        public TextBox TextBox1 { get => this.textBox1; }
        public DialogResult DialogRes { get; set;}

        private void button1_Click(object sender, EventArgs e)
        {
            DialogRes = DialogResult.OK;
            this.Close();
        }
        //public DialogResult ShowDialog(Form form)
        //{
        //     return DialogRes;
        //}
    }
}
