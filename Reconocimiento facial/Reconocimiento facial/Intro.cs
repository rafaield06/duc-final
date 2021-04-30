using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Reconocimiento_facial
{
    public partial class Intro : Form
    {
        private int conteo;
        public Intro()
        {
            
            InitializeComponent();
            timer1.Start();
        }

        private void Intro_Load(object sender, EventArgs e)
        {
           
            pictureBox1.Image = Image.FromFile(@"C:\Users\Ian Emile\OneDrive\Escritorio\Reconocimiento\colegio.gif");
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            conteo++;
            label1.Text = conteo.ToString();
            if(label1.Text == "6")
            {
                Frm_Reconocimiento elOtro = new Frm_Reconocimiento();

                this.Hide();
                elOtro.ShowDialog();
                this.Close();
                
            }
        }
    }
}
