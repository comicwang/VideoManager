 using System.Web;
  using System.Configuration;
using System.Diagnostics;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace VedioWeb
{
    public class VideoConverter
    {
        public static string ffmpegtool = System.Configuration.ConfigurationManager.AppSettings["ffmpeg"];

        private static VideoInfo videoMeta = null;

        public static void Clear()
        {
            videoMeta = null;
        }

        /// <summary>
        /// 执行Ffmpeg命令
        /// </summary>
        /// <param name="strArg"></param>
        /// <param name="output"></param>
        /// <param name="outerro"></param>
        private static void FfmpegCommand(string strArg,out string output,out string outerro)
        {
            //参数说明
            /*
                * -i filename(input) 源文件目录
                * -y 输出新文件，是否强制覆盖已有文件
                * -c 指定编码器 
                * -fs limit_size(outinput) 设置文件大小的限制，以字节表示的。没有进一步的字节块被写入后，超过极限。输出文件的大小略大于所请求的文件大小。
                * -s 视频比例  4:3 320x240/640x480/800x600  16:9  1280x720 ，默认值 'wxh'，和原视频大小相同
                * -vframes number(output) 将视频帧的数量设置为输出。别名：-frames:v
                * -dframes number (output) 将数据帧的数量设置为输出.别名：-frames:d
                * -frames[:stream_specifier] framecount (output,per-stream) 停止写入流之后帧数帧。
                * -bsf[:stream_specifier] bitstream_filters (output,per-stream)  指定输出文件流格式，
            例如输出h264编码的MP4文件：ffmpeg -i h264.mp4 -c:v copy -bsf:v h264_mp4toannexb -an out.h264
                * -r 29.97 桢速率（可以改，确认非标准桢率会导致音画不同步，所以只能设定为15或者29.97） 
                * 
                */
            Process p = new Process();//建立外部调用线程
            p.StartInfo.FileName = ffmpegtool;//要调用外部程序的绝对路径
                                              //参数(这里就是FFMPEG的参数了)
                                              //p.StartInfo.Arguments = @"-i "+sourceFile+ " -ab 56  -b a -ar 44100 -b 500 -r 29.97 -s 1280x720 -y " + playFile+"";

            // p.StartInfo.Arguments = "-y -i \""+sourceFile+"\" -b v   -s 800x600 -r 29.97 -b 1500 -acodec aac -ac 2 -ar 24000 -ab 128 -vol 200 -f psp  \""+playFile+"\" ";


            //string strArg = "-i " + sourceFile + " -y -s 640x480 " + playFile + " ";
           // string strArg = string.Empty;//"-i " + sourceFile + " -y -s 1280x720 " + playFile + " ";

           // string destFile = Path.Combine(Path.GetDirectoryName(sourceFile), Path.GetFileNameWithoutExtension(sourceFile) + ".jpg");

           // strArg = string.Format(@"-i {0} -y -f image2 -t 0.001 -s 300x300 {1}", sourceFile, destFile);

            //获取图片
            //截取图片jpg
            //string strArg = "-i " + sourceFile + " -y -f image2 -t 1 " + imgFile;
            //string strArg = "-i " + sourceFile + " -y -s 1280x720 -f image2 -t 1 " + imgFile;

            //视频截取
            //string strArg = "  -i " + sourceFile + " -y   -ss 0:20  -frames 100  " + playFile;

            //转化gif动画
            //string strArg = "-i " + sourceFile + " -y -s 1280x720 -f gif -vframes 30 " + imgFile;
            //string strArg = "  -i " + sourceFile + " -y  -f gif -vframes 50 " + imgFile;
            // string strArg = "  -i " + sourceFile + " -y  -f gif -ss 0:20  -dframes 10 -frames 50 " + imgFile;

            //显示基本信息
            //string strArg = "-i " + sourceFile + " -n OUTPUT";

            //播放视频 
            //string strArg = "-stats -i " + sourceFile + " ";

            p.StartInfo.Arguments = strArg;

            p.StartInfo.UseShellExecute = false;//不使用操作系统外壳程序启动线程(一定为FALSE,详细的请看MSDN)
            p.StartInfo.RedirectStandardError = true;//把外部程序错误输出写到StandardError流中(这个一定要注意,FFMPEG的所有输出信息,都为错误输出流,用StandardOutput是捕获不到任何消息的...这是我耗费了2个多月得出来的经验...mencoder就是用standardOutput来捕获的)
            string outerroData = string.Empty;
            string outputData = string.Empty;
            p.StartInfo.CreateNoWindow = false;//不创建进程窗口
            p.OutputDataReceived += (ss, ee) =>
            {
                outputData += ee.Data;
            };

            p.ErrorDataReceived += (ss, ee) =>
            {
                outerroData += ee.Data;
            };

            p.Start();//启动线程
            p.BeginErrorReadLine();//开始异步读取
            p.WaitForExit();//阻塞等待进程结束
            p.Close();//关闭进程
            p.Dispose();//释放资源

            output = outputData;
            outerro = outerroData;
        }    

        /// <summary>
        /// 获取视频元数据信息
        /// </summary>
        /// <param name="sourceFile">视频文件地址</param>
        public static VideoInfo GetVideoInfo(string sourceFile)
        {
            try
            {
                //判断文件是否存在
                if (!File.Exists(sourceFile))
                {
                    return null;
                }

                string output = string.Empty;
                string outerro = string.Empty;
                FfmpegCommand(@"-i " + sourceFile, out output, out outerro);

                if (string.IsNullOrEmpty(outerro))
                {
                    return null;
                }

                //通过正则表达式获取信息里面的宽度信息
                Regex regex = new Regex("(\\d{2,4})x(\\d{2,4})", RegexOptions.Compiled);
                Match m = regex.Match(outerro);
                if (m.Success)
                {
                    videoMeta = new VideoInfo();
                    videoMeta.width = int.Parse(m.Groups[1].Value);
                    videoMeta.height = int.Parse(m.Groups[2].Value);
                    return videoMeta;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取视频文件的截图
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <returns></returns>
        public static string GetImage(string sourceFile)
        {
            //获取视频的元数据信息
            VideoInfo info = videoMeta;
            if (info == null)
                info = GetVideoInfo(sourceFile);

            string destFile = Path.Combine(Path.GetDirectoryName(sourceFile), Path.GetFileNameWithoutExtension(sourceFile) + ".jpg");

            string strArg = string.Format(@"-i {0} -y -f image2 -t 0.001 -s {2}x{3} {1}", sourceFile, destFile, 300, (int)(300 * info.height / info.width));

            string output = string.Empty;
            string outerro = string.Empty;
            FfmpegCommand(strArg, out output, out outerro);
            if (string.IsNullOrEmpty(outerro))
            {
                return string.Empty;
            }
            return destFile;
        }

        public static bool Tans2Mp4(string sourceFile)
        {
            //获取视频的元数据信息
            VideoInfo info = videoMeta;
            if (info == null)
                info = GetVideoInfo(sourceFile);

            string destFile = Path.Combine(Path.GetDirectoryName(sourceFile), Path.GetFileNameWithoutExtension(sourceFile) + ".mp4");
            string strArg = string.Format("-i {0} -y {3} ", sourceFile, info.width, info.height, destFile);
            string output = string.Empty;
            string outerro = string.Empty;
            FfmpegCommand(strArg, out output, out outerro);
            if (string.IsNullOrEmpty(outerro))
            {
                return false;
            }
            return true;
        }
    }

    public class VideoInfo
    {
        public int width { get; set; }

        public int height { get; set; }

        public string size { get; set; }
    }
}
 