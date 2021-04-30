using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconocimiento_facial
{
    public class AccesDB
    {
        private OleDbConnection conn;
        public string[] Name;
        public string[] Code;
        public string[] Curso;
        private byte[] face;
        public List<byte[]> Face = new List<byte[]>();
        public int TotalUser;
        public AccesDB()
        {
            conn = new OleDbConnection("Provider = Microsoft.Jet.OLEDB.4.0; Data Source = UsersFace.mdb");
            conn.Open();
        }
        public bool GuardarImagen(string Name, string Code, string curso, byte[] abImagen)
        {
            conn.Open();
            OleDbCommand comm = new OleDbCommand("INSERT INTO UserFaces (Name,Code,Curso,Face) VALUES ('" + Name + "','" + Code + "','" + curso + "' ,?)", conn);           
            OleDbParameter parImagen = new OleDbParameter("@Face", OleDbType.VarBinary, abImagen.Length);
            parImagen.Value = abImagen;
            comm.Parameters.Add(parImagen);            
            int iResultado = comm.ExecuteNonQuery();
            conn.Close();
            return Convert.ToBoolean(iResultado);
        }

        public DataTable ObtenerBytesImagen()
        {
            string sql = "SELECT IdImage,Name,Code,Curso,Face FROM UserFaces";
            OleDbDataAdapter adaptador = new OleDbDataAdapter(sql, conn);
            DataTable dt = new DataTable();
            adaptador.Fill(dt);
            int cont = dt.Rows.Count;
            Name = new string[cont];
            Code = new string[cont];
            Curso = new string[cont];

            for (int i = 0; i < cont; i++)
            {
                Name[i] = dt.Rows[i]["Name"].ToString();
                Code[i] = dt.Rows[i]["Code"].ToString();
                Curso[i] = dt.Rows[i]["Curso"].ToString();
                face = (byte[])dt.Rows[i]["Face"];
                Face.Add(face);
            }
            TotalUser = dt.Rows.Count;
            conn.Close();
            return dt;
        }

        public void ConvertImgToBinary(string Name, string Code, string curso, Image Img)
        {
            Bitmap bmp = new Bitmap(Img);
            MemoryStream MyStream = new MemoryStream();
            bmp.Save(MyStream, System.Drawing.Imaging.ImageFormat.Bmp);

            byte[] abImagen = MyStream.ToArray();
            GuardarImagen(Name, Code, curso, abImagen);
        }

        public Image ConvertByteToImg( int con)
        {
            Image FetImg;
            byte[] img = Face[con];
            MemoryStream ms = new MemoryStream(img);
            FetImg = Image.FromStream(ms);
            ms.Close();
            return FetImg;

        }
    }
}
