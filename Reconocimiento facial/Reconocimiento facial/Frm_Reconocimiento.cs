using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Data.OleDb;
using System.Speech.Synthesis;
using System.Media;
using System.Runtime.InteropServices;

namespace Reconocimiento_facial
{
    public partial class Frm_Reconocimiento : Form
    {
        public int heigth, width;
        public string[] Labels;
        public string[] LabelsMatricula;
        AccesDB dbc = new AccesDB();
        int con = 0;
        SoundPlayer media = new SoundPlayer();
        SpeechSynthesizer vos = new SpeechSynthesizer();
        //DECLARANDO TODAS LAS VARIABLES, vectores y  haarcascades
        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        HaarCascade face;
        HaarCascade eye;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.4d, 0.4d);

        Image<Gray, byte> result, TrainedFace = null;
        Image<Gray, byte> resultMat, TrainedMatr = null;
        Image<Gray, byte> resultCursos, TrainedCursos = null;

        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();

        List<string> labels = new List<string>();
        List<string> labelsMatricula = new List<string>();
        List<string> labelsCurso = new List<string>();

        List<string> NamePersons = new List<string>();
        List<string> VerMatriculas = new List<string>();
        List<string> VerCurso = new List<string>();

        int ContTrain, NumLabels, t;
        string name, Labelsinfo, names = null;
        string matricula, Labelsinfoo, matriculas = null;
        string curso, Labelsinfoos, cursos = null;



        public Frm_Reconocimiento()
        {
            InitializeComponent();
            heigth = this.Height; width = this.Width;
            //GARGAMOS LA DETECCION DE LAS CARAS POR  haarcascades 
            face = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {
                dbc.ObtenerBytesImagen();
                //carga de caras y etiquetas para cada imagen               
                string[] Labels = dbc.Name;
                string[] LabelsMatriculas = dbc.Code;
                string[] LabelsCursos = dbc.Curso;
                NumLabels = dbc.TotalUser;
                ContTrain = NumLabels;
                string LoadFaces;
                
                for (int tf = 0; tf < NumLabels; tf++)
                {
                    con = tf;
                    Bitmap bmp = new Bitmap(dbc.ConvertByteToImg(con));
                    //LoadFaces = "face" + tf + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(bmp));//cargo la foto con ese nombre
                    labels.Add(Labels[tf]);//cargo el nombre que se encuentre en la posicion del tf
                    labelsMatricula.Add(LabelsMatriculas[tf]);
                    labelsCurso.Add(LabelsCursos[tf]);
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e + "No hay ningun rosto registrado).", "Cargar rostros", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Reconocer()
        {
            try
            {
                //Iniciar el dispositivo de captura
                grabber = new Capture();
                grabber.QueryFrame();
                //Iniciar el evento FrameGraber
                Application.Idle += new EventHandler(FrameGrabber);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void FrameGrabber(object sender, EventArgs e)
        {
           
            NamePersons.Add("");
            VerMatriculas.Add("");
            VerCurso.Add("");
            try
            {
                currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                //Convertir a escala de grises
                gray = currentFrame.Convert<Gray, Byte>();

                //Detector de Rostros
                MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(face, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));

                //Accion para cada elemento detectado
                foreach (MCvAvgComp f in facesDetected[0])
                {
                    t = t + 1;
                    result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);
                    resultMat = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);
                    resultCursos = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, INTER.CV_INTER_CUBIC);

                    //Dibujar el cuadro para el rostro
                    currentFrame.Draw(f.rect, new Bgr(Color.LightGreen), 1);

                    if (trainingImages.ToArray().Length != 0)
                    {
                        //Clase para reconocimiento con el nùmero de imagenes
                        MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);

                        //Clase Eigen para reconocimiento de rostro
                        EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), ref termCrit);
                        EigenObjectRecognizer recognizerMat = new EigenObjectRecognizer(trainingImages.ToArray(), labelsMatricula.ToArray(), ref termCrit);
                        EigenObjectRecognizer recognizerCursos = new EigenObjectRecognizer(trainingImages.ToArray(), labelsCurso.ToArray(), ref termCrit);

                        var fa = new Image<Gray, byte>[trainingImages.Count]; //currentFrame.Convert<Bitmap>();

                        name = recognizer.Recognize(result);
                        matricula = recognizerMat.Recognize(resultMat);
                        curso = recognizerCursos.Recognize(resultCursos);

                        //Dibujar el nombre para cada rostro detectado y reconocido
                        currentFrame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.YellowGreen));

                    }

                    NamePersons[t - 1] = name;
                    NamePersons.Add("");

                    VerMatriculas[t - 1] = matriculas;
                    VerMatriculas.Add("");

                    VerCurso[t - 1] = cursos;
                    VerCurso.Add("");


                    //Establecer el nùmero de rostros detectados

                    lblNadie.Text = name;

                    lblMatricula.Text = matricula;

                    lblCurso.Text = curso;
                  
                    

                }
                t = 0;

                //Nombres concatenados de todos los rostros reconocidos
                for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
                {
                    names = names + NamePersons[nnn] + ", ";
                    matriculas = matriculas + VerMatriculas[nnn] + ", ";
                    cursos = cursos + VerCurso[nnn] + ", ";
                }

      

                //Mostrar los rostros procesados y reconocidos
                imageBoxFrameGrabber.Image = currentFrame;
                name = "";
                matricula = "";
                curso = "";
                //Borrar la lista de nombres            
                NamePersons.Clear();
                VerMatriculas.Clear();
                VerCurso.Clear();


                    

                
            }
            catch (Exception ex)
            {

                lblEntrar.Visible = true;
                lblEntrar.Text = "Puedes entrar al autobus";
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Intro laIntro = new Intro();
            this.Close();
           
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

 

        private void Frm_Reconocimiento_Load(object sender, EventArgs e)
        {
            Reconocer();
            media.SoundLocation = "sounds/2.wav";
            media.Play();
        }

        private void btn_Salir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_mini_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void btn_Registrar_Click(object sender, EventArgs e)
        {
           try
            {
                btn_Registrar.Font = new Font("Lucida", 12, FontStyle.Bold);
                Desconectar();
                Frm_Registrar re = new Frm_Registrar();
                Frm_Filtro fil = new Frm_Filtro();
                fil.Show();
                re.ShowDialog();
                fil.Hide();

            }
            catch
            {

            }
        
        }

        private void button4_Click(object sender, EventArgs e)
        {
            switch (button4.Text)
            {
                case "Conectar":
                    Reconocer();
                    button4.Text = "Desconectar";
                    button4.BackgroundStyle.SolidColor = Color.YellowGreen;
                    button4.BorderStyle.SolidColor = Color.YellowGreen;
                    button4.StateStyles.HoverStyle.BackgroundSolidColor = Color.YellowGreen;
                    button4.StateStyles.HoverStyle.BorderSolidColor = Color.YellowGreen;
                    break;
                case "Desconectar":
                    Desconectar();
                    break;
            }
        }
        private void Desconectar()
        {
            Application.Idle -= new EventHandler(FrameGrabber);
            grabber.Dispose();
            imageBoxFrameGrabber.ImageLocation = "img/1.jpg";
            lblNadie.Text = string.Empty;
            lblMatricula.Text = string.Empty;
            lblCurso.Text = string.Empty;
            button4.Text = "Conectar";
            button4.BackgroundStyle.SolidColor = Color.FromArgb(192, 0, 0);
            button4.BorderStyle.SolidColor= Color.FromArgb(192, 0, 0);
            button4.StateStyles.HoverStyle.BackgroundSolidColor = Color.FromArgb(192, 0, 0);
            button4.StateStyles.HoverStyle.BorderSolidColor = Color.FromArgb(192, 0, 0);
        }
    }
}
