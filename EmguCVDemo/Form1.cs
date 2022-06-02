using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
namespace EmguCVDemo
{
    public partial class Form1 : Form
    {
        

        public Form1()
        {

            InitializeComponent();
            #region 读取图片
            //Mat img = CvInvoke.Imread(@"C:\Users\xnx\Pictures\0.jpg");
            //CvInvoke.Imshow("img",img);
            //CvInvoke.WaitKey();
            #endregion
            #region 播放视频
            cap = new Emgu.CV.VideoCapture("rtsp://122.112.145.82:8554/mystream");

#pragma warning disable CS8622 // 参数类型中引用类型的为 Null 性与目标委托不匹配(可能是由于为 Null 性特性)。
            cap.ImageGrabbed += capture_ImageGrabbed;
#pragma warning restore CS8622 // 参数类型中引用类型的为 Null 性与目标委托不匹配(可能是由于为 Null 性特性)。
            cap.Start();
            #endregion
        }


        #region 读取视频
        Emgu.CV.VideoCapture cap;
        private delegate void SetPicVideo(Bitmap val);//跨线程修改图片框
        private readonly object balanceLock = new object();
        private object lockObj = new object();
        private Thread SetPicVideoThread;
        Bitmap bmpVideo = null;


        private void capture_ImageGrabbed(object sender, EventArgs e)
        {
            try
            {
                Mat frame = new Mat();
                // 改变后的图片
                Mat frame2 = new Mat();
                lock (lockObj)
                {
                    if (cap != null  )
                    {
                        if (!cap.Retrieve(frame))
                        {
                            frame.Dispose();
                            return;
                        }
                        if (frame.IsEmpty)
                            return;
                       
                        int width = pictureBox1.Width;
                        int height = pictureBox1.Height;
                        System.Drawing.Size s = new System.Drawing.Size(width, height);
                        CvInvoke.Resize(frame, frame2, s, 0, 0);
                        if (isRecording)
                        {
                            try
                            {
                                vw.Write(frame);
                            }
                            catch { 
                            
                            }
                          
                        }

                        bmpVideo = frame2.ToBitmap();
                        SetPicVideoThread = new Thread(new ThreadStart(setPicVideo));
                        SetPicVideoThread.IsBackground = true;
                        SetPicVideoThread.Start();
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        void SetPic(Bitmap val)
        {
            if (val != null)
            {
                this.pictureBox1.Image = val;
            }

        }



        private void setPicVideo()
        {
            try
            {
                if (pictureBox1.InvokeRequired)
                {
                    SetPicVideo d = new SetPicVideo(SetPic);

                    object[] arg = new object[] { bmpVideo };//要传入的参数值
                    this.Invoke(d, arg);
                }
                else
                {

                    SetPic(bmpVideo);
                }
            }
            catch { 
            
            }
           
        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cap.Stop();
        }
        #endregion
        #region 视频录制

        DateTime recordTime;
        VideoWriter vw;
        bool isRecording = false;
        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "开始录制")
            {
                recordTime = DateTime.Now;
                button1.ForeColor = Color.Red;
                string saveDir = Path.Combine(Application.StartupPath,"video");
                if (!Directory.Exists(saveDir)) { 
                    Directory.CreateDirectory(saveDir);
                } CvInvoke
                vw = new VideoWriter(Path.Combine(saveDir, DateTime.Now.ToString("yyyyMMddHHmmsss") + ".avi"),(int)VideoWriter.Fourcc('M', 'J', 'P', 'G'), 30, new Size(cap.Width, cap.Height), true);
                 
                vw.Set(VideoWriter.WriterProperty.Quality, 0.1);
                isRecording = true;
                button1.Text = "结束录制";
            }
            else
            {
                isRecording = false;
                
                vw.Dispose();
                button1.ForeColor = Color.Black;
                button1.Text = "开始录制";
                TimeSpan ts = DateTime.Now - recordTime;
                label1.Text = $"共录制：{ts.Seconds}秒";
                
            }
        }
        #endregion

    }
}