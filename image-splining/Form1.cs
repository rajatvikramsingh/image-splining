using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.ML;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.DebuggerVisualizers;


namespace image_splining
{
    public partial class Form1 : Form
    {
        Image<Bgr, Byte> img1, img2;
        Image<Bgr, byte>[] img1_gp, img2_gp, g;
        Image<Bgr, byte>[] img1_lp, img2_lp, p, lap;
        int dim;
        private double[] w1 = new double[5] { 0.05, 0.25, 0.4, 0.25, 0.05 };
        double[,] w2 = new double[5, 5];
        double[,] wt = new double[5, 5];
        int count = 0;
        


        public Form1()
        {
            InitializeComponent();
        }

        private Image<Bgr, byte>[] gaussianPyr(Image<Bgr, byte> img)
        {
            g = new Image<Bgr, byte>[dim];
            g[0] = img;
            for (int i = 1; i < dim; i++)
            {
                g[i] = new Image<Bgr, byte>(g[i - 1].Height / 2, g[i - 1].Width / 2);
                //g[i] = reduce(g[i - 1]);
                CvInvoke.cvPyrDown(g[i - 1], g[i], FILTER_TYPE.CV_GAUSSIAN_5x5);
            }
            return g;
        }



        private double[,] generate_weight()
        {
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                {
                    w2[i, j] = w1[i] * w1[j];
                }
            return w2;
        }


        /* private Image<Bgr,Byte> reduce(Image<Bgr,Byte> im)
         {
             wt = generate_weight();
             Image<Bgr, byte> imgnew = new Image<Bgr, byte>(im.Height/2, im.Width/2);
             Image<Bgr, byte> imgnew2 = new Image<Bgr, byte>(im.Height / 2, im.Width / 2);
             double[,] arr_R = new double[imgnew.Height, imgnew.Width];
             double[,] arr_G = new double[imgnew.Height, imgnew.Width];
             double[,] arr_B = new double[imgnew.Height, imgnew.Width];
             double intensity_R = 0;
             double intensity_B = 0;
             double intensity_G = 0;
             int i, j = 0;

             for (i = 0; i < imgnew.Height; i++)
                 for (j = 0; j < imgnew.Width; j++)
                 {
                     intensity_R = 0;
                     intensity_B = 0;
                     intensity_G = 0;
                     for (int k = 0; k < 5; k++)
                         for (int l = 0; l < 5; l++)
                         {
                             if ((2*i + k  >= 0) && (2*j + l  >= 0) && (2*i + k  < im.Height) && (2*j + l  < im.Width))
                             {
                                 intensity_R = intensity_R + im[2*i + k , 2*j + l].Red * wt[k,l];
                                 intensity_G = intensity_G + im[2 * i + k, 2 * j + l].Green * wt[k, l];
                                 intensity_B = intensity_B + im[2 * i + k, 2 * j + l].Blue * wt[k, l];
                             }
                         }
                     arr_R[i, j] = intensity_R;
                     arr_B[i, j] = intensity_B;
                     arr_G[i, j] = intensity_G;
                     imgnew[i, j] = new Bgr(intensity_B, intensity_G, intensity_R);
                 }
             //CvInvoke.cvNormalize(imgnew, imgnew2, 0, 255, NORM_TYPE.CV_MINMAX, IntPtr.Zero);
             imgnew = scaleImage(arr_R, arr_G, arr_B, imgnew.Height, imgnew.Width);
             return imgnew;
            
         }


         private Image<Bgr, Byte> expand(Image<Bgr, Byte> im)
         {
             Image<Bgr, byte> imgnew2 = new Image<Bgr, byte>(im.Height*2, im.Width*2);
             Image<Bgr, byte> imgnew = new Image<Bgr, byte>(im.Height * 2, im.Width * 2);
             double[,] arr_R = new double[imgnew.Height, imgnew.Width];
             double[,] arr_G = new double[imgnew.Height, imgnew.Width];
             double[,] arr_B = new double[imgnew.Height, imgnew.Width];

             double res_R = 0;
             double res_G = 0;
             double res_B = 0;

             wt = generate_weight();
             for (int i = 0; i < imgnew.Height; i++)
                 for (int j = 0; j < imgnew.Width; j++)
                 {
                     res_R = 0;
                     res_G = 0;
                     res_B = 0;
                     for (int m = -2; m < 3; m++)
                         for (int n = -2; n < 3; n++)
                         {
                             if ((((i - m) / 2) >= 0) && (((j - n) / 2) >= 0) && (((i - m) / 2) < im.Height) && (((j - n) / 2) < im.Width))
                             {
                                 res_R = res_R + im[(int)((i - m) / 2), (int)((j - n) / 2)].Red *wt[m + 2, n + 2];
                                 res_G = res_G + im[(int)((i - m) / 2), (int)((j - n) / 2)].Green *wt[m + 2, n + 2];
                                 res_B = res_B + im[(int)((i - m) / 2), (int)((j - n) / 2)].Blue *wt[m + 2, n + 2];
                             }
                         }
                     double intensity_R = 4 * res_R;
                     double intensity_B = 4 * res_G;
                     double intensity_G = 4 * res_B;
                     arr_B[i, j] = intensity_B;
                     arr_G[i, j] = intensity_G;
                     arr_R[i, j] = intensity_R;
                     imgnew[i, j] = new Bgr(intensity_B, intensity_G, intensity_R);
                 }
             imgnew = scaleImage(arr_R,arr_G,arr_B, imgnew.Height, imgnew.Width);
             //CvInvoke.cvNormalize(imgnew, imgnew2, 0, 255, NORM_TYPE.CV_MINMAX, IntPtr.Zero);
             return imgnew;
         }

        

         private Image<Bgr, byte> scaleImage(double[,] arr_R,double[,] arr_G,double[,] arr_B, int height, int width)
         {
             double min_R = arr_R[0, 0];
             double min_G = arr_G[0, 0];
             double min_B = arr_B[0, 0];
            
             double[,] imgnew_R = new double[height, width];
             double[,] imgnew_G = new double[height, width];
             double[,] imgnew_B = new double[height, width];

             Image<Bgr, byte> image = new Image<Bgr, byte>(height, width);
             for (int i = 0; i < height; i++)
                 for (int j = 0; j < width; j++)
                 {
                     if (arr_R[i, j] < min_R)
                         min_R = arr_R[i, j];
                     if (arr_G[i, j] < min_G)
                         min_G = arr_G[i, j];
                     if (arr_B[i, j] < min_B)
                         min_B = arr_B[i, j];
                 }

             for (int i = 0; i < height; i++)
                 for (int j = 0; j < width; j++)
                 {
                     imgnew_R[i, j] = arr_R[i, j] - min_R;
                     imgnew_G[i, j] = arr_G[i, j] - min_G;
                     imgnew_B[i, j] = arr_B[i, j] - min_B;
                 }
            
             double max_R = imgnew_R[0, 0];
             double max_G = imgnew_G[0, 0];
             double max_B = imgnew_B[0, 0];

             for (int i = 0; i < height; i++)
                 for (int j = 0; j < width; j++)
                 {
                     if (imgnew_R[i, j] > max_R)
                         max_R = imgnew_R[i, j];
                     if (imgnew_G[i, j] > max_G)
                         max_G = imgnew_G[i, j];
                     if (imgnew_B[i, j] > max_B)
                         max_B = imgnew_B[i, j];
                 }
             for (int i = 0; i < height; i++)
                 for (int j = 0; j < width; j++)
                     image[i, j] = new Bgr(((imgnew_B[i, j] / max_B) * 255),((imgnew_G[i, j] / max_G) * 255),(imgnew_R[i, j] / max_R) * 255);
             return image;
         }
         */

        private Image<Bgr, byte>[] laplacianPyr(Image<Bgr, byte> img, Image<Bgr, byte>[] g)
        {
            p = new Image<Bgr, byte>[dim - 1];

            for (int i = 0; i < dim - 1; i++)
            {
                Image<Bgr, byte> temp = new Image<Bgr, byte>(g[i].Height, g[i].Width);
                //p[i] = g[i].Sub(expand(g[i + 1]));
                CvInvoke.cvPyrUp(g[i + 1], temp, FILTER_TYPE.CV_GAUSSIAN_5x5);
                p[i] = g[i].Sub(temp);
            }
            //p[dim-1] = img;
            return p;
        }


        private void image1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog OpenFileDialogue1 = new OpenFileDialog();
            OpenFileDialogue1.ShowDialog();
            if (OpenFileDialogue1.FileName.Length != 0)
            {
                img1 = new Image<Bgr, Byte>(OpenFileDialogue1.FileName);
                imageBox1.Image = img1;
                dim = (int)Math.Log((double)img1.Height, (double)2);
            }
            else
            {
                MessageBox.Show("Please enter a Filename!!!");
            }
        }

        private void image2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog OpenFileDialogue1 = new OpenFileDialog();
            OpenFileDialogue1.ShowDialog();
            if (OpenFileDialogue1.FileName.Length != 0)
            {
                img2 = new Image<Bgr, Byte>(OpenFileDialogue1.FileName);
                imageBox2.Image = img2;
            }
            else
            {
                MessageBox.Show("Please enter a Filename!!!");
            }
            label1.Visible = true;
            label2.Visible = true;
            label3.Visible = true;
            label4.Visible = true;
            numericUpDown1.Visible = true;
            numericUpDown2.Visible = true;
            numericUpDown3.Visible = true;
            numericUpDown4.Visible = true;
        }

        private void splineToolStripMenuItem_Click(object sender, EventArgs e)
        {

            img1_gp = gaussianPyr(img1);
            img2_gp = gaussianPyr(img2);
            img1_lp = laplacianPyr(img1, img1_gp);
            img2_lp = laplacianPyr(img2, img2_gp);
            lap = new Image<Bgr, byte>[dim];
            Image<Bgr, byte> iml, imr, imf;

            for (int k = 0; k < dim - 1; k++)
            {
                iml = img1_lp[k];
                imr = img2_lp[k];
                imf = new Image<Bgr, byte>(iml.Height, iml.Width);
                for (int i = 0; i < imf.Height; i++)
                    for (int j = 0; j < imf.Width; j++)
                    {
                        //if (i < Math.Pow(2, Math.Log - 1))
                        if (j < imf.Height / 2)
                            imf[i, j] = iml[i, j];

                        else if (j == imf.Height / 2)
                            imf[i, j] = new Bgr((iml[i, j].Blue + imr[i, j].Blue) / 2, (iml[i, j].Green + imr[i, j].Green) / 2, (iml[i, j].Red + imr[i, j].Red) / 2);

                        else
                            imf[i, j] = imr[i, j];
                    }
                lap[k] = imf;
            }

            Image<Bgr, byte>[] fimg = lap;
            for (int k = dim - 2; k > 0; k--)
            {
                Image<Bgr, byte> temp = new Image<Bgr, byte>(fimg[k - 1].Height, fimg[k - 1].Width);
                //fimg[k - 1] = addImage(fimg[k - 1], expand(fimg[k]));
                CvInvoke.cvPyrUp(fimg[k], temp, FILTER_TYPE.CV_GAUSSIAN_5x5);
                fimg[k - 1] = fimg[k - 1] + temp;
            }

            fimg[0]._EqualizeHist();
            imageBox3.Image = fimg[0];
        }

        private void nonOverlappingSplineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image<Bgr, byte> Rimg = new Image<Bgr, byte>(img1.Width, img1.Height);
            for (int i = (int)numericUpDown2.Value; i < numericUpDown4.Value; i++)
            {
                for (int j = (int)numericUpDown1.Value; j < numericUpDown3.Value; j++)
                {
                    Rimg[i, j] = new Bgr(1, 1, 1);
                }
            }
            
            Image<Bgr, byte>[] gr = gaussianPyr(Rimg);
            img1_gp = gaussianPyr(img1);
            img2_gp = gaussianPyr(img2);
            img1_lp = laplacianPyr(img1, img1_gp);
            img2_lp = laplacianPyr(img2, img2_gp);
            lap = new Image<Bgr, byte>[dim];
            Image<Bgr, byte> iml, imr, imf, gri;

            for (int k = 0; k < dim - 1; k++)
            {
                iml = img1_lp[k];
                imr = img2_lp[k];
                gri = gr[k];
                imf = new Image<Bgr, byte>(iml.Height, iml.Width);
                double R, B, G;

                for (int i = 0; i < imf.Height; i++)
                {
                    for (int j = 0; j < imf.Width; j++)
                    {
                        R = iml[i, j].Red * gri[i, j].Red + imr[i, j].Red * (1 - gri[i, j].Red);
                        B = iml[i, j].Blue * gri[i, j].Blue + imr[i, j].Blue * (1 - gri[i, j].Blue);
                        G = iml[i, j].Green * gri[i, j].Green + imr[i, j].Green * (1 - gri[i, j].Green);
                        imf[i, j] = new Bgr(B, G, R);
                    }
                }
                    lap[k] = imf;
                
            }

            Image<Bgr, byte>[] fimg = lap;
            for (int k = dim - 2; k > 0; k--)
            {
                Image<Bgr, byte> temp = new Image<Bgr, byte>(fimg[k - 1].Height, fimg[k - 1].Width);
                //fimg[k - 1] = addImage(fimg[k - 1], expand(fimg[k]));
                CvInvoke.cvPyrUp(fimg[k], temp, FILTER_TYPE.CV_GAUSSIAN_5x5);
                fimg[k - 1] = fimg[k - 1] + temp;
            }

            fimg[0]._EqualizeHist();
            imageBox3.Image = fimg[0];



        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Visible = false;
            label2.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            numericUpDown1.Visible = false;
            numericUpDown2.Visible = false;
            numericUpDown3.Visible = false;
            numericUpDown4.Visible = false;
        }

        private void imagebox1_Click1(object sender, EventArgs e)
        {
            Point LocalMousePosition1;
            if (count%2 ==0)
            {
                LocalMousePosition1 = imageBox1.PointToClient(Cursor.Position);
                numericUpDown1.Value = LocalMousePosition1.X * (img1.Height/imageBox1.Height);
                numericUpDown2.Value = LocalMousePosition1.Y * (img1.Height / imageBox1.Height);
            }
            else
            {
                LocalMousePosition1 = imageBox1.PointToClient(Cursor.Position);
                numericUpDown3.Value = LocalMousePosition1.X * (img1.Height / imageBox1.Height);
                numericUpDown4.Value = LocalMousePosition1.Y * (img1.Height / imageBox1.Height);
            }
            count++;

        }
        
        
            

        /*private Image<Bgr, byte> addImage(Image<Bgr, byte> img1, Image<Bgr, byte> img2)
        {
            double[,] img_R = new double[img1.Height, img1.Width];
            double[,] img_B = new double[img1.Height, img1.Width];
            double[,] img_G = new double[img1.Height, img1.Width]; 
            Image<Bgr, byte> fimg = new Image<Bgr, byte>(img1.Height, img1.Width);
            Image<Bgr, byte> imgnew2 = new Image<Bgr, byte>(img1.Height, img1.Width);
            for (int i = 0; i < img1.Height; i++)
                for (int j = 0; j < img1.Width; j++)
                {
                    img_B[i, j] = img1[i, j].Blue + img2[i, j].Blue;
                    img_R[i, j] = img1[i, j].Red + img2[i, j].Red;
                    img_G[i, j] = img1[i, j].Green + img2[i, j].Green;
                    fimg[i, j] = new Bgr(img1[i, j].Blue + img2[i, j].Blue, img1[i, j].Green + img2[i, j].Green, img1[i, j].Red + img2[i, j].Red);
                }
            
            fimg = scaleImage(img_R,img_G,img_B, img1.Height, img1.Width); //Comment this line to see without scaling
            //CvInvoke.cvNormalize(fimg, imgnew2, 0, 255, NORM_TYPE.CV_MINMAX, IntPtr.Zero);
            return fimg;
        }
        */


    }
}
