using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.IO;
using System.Diagnostics;


namespace ProjectAppCameraKinect
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private KinectSensor kinect;
        int contImagem;

        public MainWindow()
        {
            InitializeComponent();
            InicializarSensor(10);
            inicializarFluxoDeCores();
            
        }

        private KinectSensor InicializarSensor(int anguloElevacaoInicial)
        {
            this.kinect = KinectSensor.KinectSensors.First(sensor => sensor.Status == KinectStatus.Connected);
            this.kinect.Start();
            this.kinect.ElevationAngle = anguloElevacaoInicial;
            return this.kinect;
        }

        private void inicializarFluxoDeCores()
        {
            this.kinect.ColorStream.Enable(); //formato padrão,caso não passe nenhum parâmetro: ColorImageFormat.RgbResolution640x480Fps30)
        }

        private BitmapSource criarImagemRGB(ColorImageFrame quadro)
        {
            using (quadro)
            {
                byte[] bytesImagem = new byte[quadro.PixelDataLength];
                quadro.CopyPixelDataTo(bytesImagem);
                return BitmapSource.Create(quadro.Width, quadro.Height,
                96, 96, PixelFormats.Bgr32, null, bytesImagem,
                quadro.Width * quadro.BytesPerPixel);
            }
        }


        
        private void gravarImagem(BitmapSource imagem)
        {

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(imagem));
            using (var fs = new FileStream("./img/" + (contImagem++) + ".jpeg", FileMode.Create, FileAccess.Write))
            {
                encoder.Save(fs);
            }

        }

        
        private void gravarVideo()
        {
            
            Process.Start("ffmpeg.exe", "-framerate 10 -i ./img/%d.jpeg -c:v libx264 -r 30 -pix_fmt yuv420p ./videos/kinect_video.mp4");
        }

        private void Button_BaterFoto(object sender, RoutedEventArgs e)
        {
            BitmapSource imagem = criarImagemRGB(kinect.ColorStream.OpenNextFrame(0));  //imageCamera é a imagem criada no arquivo MainWindows.xaml
            imagemCamera.Source = imagem;
            gravarImagem(imagem);
            
        }

    
        private void Button_IniciarVideo(object sender, RoutedEventArgs e)
        {
            kinect.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinect_ColorFrameReadyVideo); // associação do evento 
            
          
        }

        private void Button_EncerrarVideo(object sender, RoutedEventArgs e)
        {
            if (imagemCamera.Source == null)
            {
                MessageBox.Show("Inicie o vídeo primeiro!");
            }
            else
            {
                Process.Start("ffmpeg.exe", "-framerate 10 -i ./img/%d.jpeg -c:v libx264 -r 30 -pix_fmt yuv420p ./video/kinect_video.mp4");
     
            }
        }

        private void Button_Sair(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void kinect_ColorFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame quadro = e.OpenColorImageFrame())
            {
                if (quadro == null)
                    return;
                imagemCamera.Source = criarImagemRGB(quadro);
            }
        }

        private void kinect_ColorFrameReadyVideo(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame quadro = e.OpenColorImageFrame())
            {
                if (quadro == null)
                    return;
                BitmapSource imagem = criarImagemRGB(quadro);
                imagemCamera.Source = imagem;
                gravarImagem(imagem);
                imagem = null;
            }
        }
       

        
    }
}
