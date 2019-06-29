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
using System.Drawing.Drawing2D;
using System.Media;
using System.Xml;

namespace AbbMusicPlayer
{
    public partial class Form1 : Form
    {
        private Point mPoint;
        string[] files;//播放文件路径
        double max, now, bal;//播放进去条位置
        int playindex=-1;//播放曲目下标
        int timecount = 0;//计时
        string info="";//播放歌曲信息
        int infocount=0;//文字滚动位置
        int randomplay = 0;
        

        //xml
        private XmlDocument xml = new XmlDocument();
        private XmlNode root;
        private XmlElement pathelement;
        private XmlNode favouritenode;
        private int fav_last_id = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //加载xml赋值节点
            xml.Load("compent.xml");
            root = xml.DocumentElement;
            favouritenode = root.SelectSingleNode("favourites");
            pathelement = (XmlElement)root.SelectSingleNode("path");

            XmlNodeList topM = root.SelectNodes("//favourites/favourite");
            int count = topM.Count;
            if (count != 0)
            {
                //id赋值
                fav_last_id = int.Parse(topM.Item(count - 1).Attributes["id"].Value);
            }
            getMusicList();
            this.axWindowsMediaPlayer1.settings.volume = 100;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            this.Show();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            mPoint = new Point(e.X, e.Y);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = new Point(this.Location.X + e.X - mPoint.X, this.Location.Y + e.Y - mPoint.Y);
             }

        }

        private void label6_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count > 0) { 
                this.label4.Visible = true;
                this.label6.Visible = false;
                //选中则播放选中，未选中当前未有歌曲播放则播放第一首歌曲
                if (listBox1.SelectedIndex == -1 && playindex == -1){
                    label5_Click(null,null);
                }
                else if(listBox1.SelectedIndex!=playindex){
                    playindex = listBox1.SelectedIndex;
                    this.axWindowsMediaPlayer1.URL = files[playindex];
                }
                this.axWindowsMediaPlayer1.Ctlcontrols.play();
                timer1.Enabled = true;
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count > 0) { 
                this.label6.Visible = true;
                this.label4.Visible = false;
                this.axWindowsMediaPlayer1.Ctlcontrols.pause();
                timer1.Enabled = false;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.Items.Count > 0){
                timecount = 0;
                int index = listBox1.IndexFromPoint(e.X, e.Y);
                listBox1.SelectedIndex = index;

                if (listBox1.SelectedIndex != -1)
                {
                    playindex = index;
                    this.axWindowsMediaPlayer1.URL = files[index];
                    this.axWindowsMediaPlayer1.Ctlcontrols.play();
                    this.label4.Visible = true;
                    this.label6.Visible = false;
                }
                max = 0.0; now = 0.0; bal = 0.0; trackBar1.Value = 0;
                timer1.Enabled = true;  //开始检测进度
                info = this.axWindowsMediaPlayer1.currentMedia.getItemInfo("Title") + " " + this.axWindowsMediaPlayer1.currentMedia.getItemInfo("Author");
                infocount = 0;
                testFavourite();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (info.Length > 16)
            {
                label8.Text = info.Substring(infocount, 15);
            }
            else {
                label8.Text = info;
            }
            if (infocount + 15 < info.Length)
            {
                infocount++;
            }
            else
            {
                infocount = 0;
            }

            timecount++;
            if (trackBar1.Value >= 99)//==100在1秒间隔内有时检测不到
            {
                now = 0;  now = 0.0; bal = 0.0; trackBar1.Value = 0;
                label5_Click(null, null);
            }
            else
            {
                now = this.axWindowsMediaPlayer1.Ctlcontrols.currentPosition;//获取文件的当前播放位置
                max = this.axWindowsMediaPlayer1.currentMedia.duration;//获取文件长度
                TimeSpan temp = new TimeSpan(0, 0, timecount);
                label7.Text = string.Format("{0:00}:{1:00}", temp.Minutes, temp.Seconds);
                if (max != 0) { 
                    bal = now / max;//计算百分比 
                }
            }
            trackBar1.Value = (int)(bal * 100);

        }
        private void setNextPlayIndex()
        {//判断是否随机设置下一首播放内容
            if (randomplay == 0)
            {
                if (playindex < listBox1.Items.Count - 1)
                {
                    playindex++;
                    listBox1.SelectedIndex++;
                }
                else
                {
                    playindex = 0;
                    listBox1.SelectedIndex = 0;
                }
            }
            else if (randomplay == 1)
            {
                int ct=listBox1.Items.Count;
                Random reum = new Random();
                int randomct = reum.Next(ct);
                playindex = randomct;
                listBox1.SelectedIndex = randomct;
            }
        }
        private void label5_Click(object sender, EventArgs e)
        {//下一曲按钮
            if (listBox1.Items.Count > 0) { 
                timecount = 0;
                setNextPlayIndex();
                this.axWindowsMediaPlayer1.URL = files[playindex];
                this.axWindowsMediaPlayer1.Ctlcontrols.play();
                info = this.axWindowsMediaPlayer1.currentMedia.getItemInfo("Title") + " " + this.axWindowsMediaPlayer1.currentMedia.getItemInfo("Author");
                infocount = 0;
                XmlNodeList nodelist = root.SelectNodes("favourites/favourite");
                testFavourite();
                timer1.Enabled = true;
                this.label4.Visible = true;
                this.label6.Visible = false;
            }
        }
        
        private void label3_Click(object sender, EventArgs e)
        {//上一曲按钮
            if (listBox1.Items.Count > 0) { 
                timecount = 0;
                if (randomplay == 0)
                {
                    if (playindex >= 1){
                        playindex--;
                        listBox1.SelectedIndex--;
                    }
                    else {
                        playindex = listBox1.Items.Count - 1;
                        listBox1.SelectedIndex = listBox1.Items.Count - 1;
                    }
                }else if(randomplay==1){
                    int ct = listBox1.Items.Count;
                    Random reum = new Random();
                    int randomct = reum.Next(ct);
                    playindex = randomct;
                    listBox1.SelectedIndex = randomct;
                }
                this.axWindowsMediaPlayer1.URL = files[playindex];
                this.axWindowsMediaPlayer1.Ctlcontrols.play();
                info = this.axWindowsMediaPlayer1.currentMedia.getItemInfo("Title") + " " + this.axWindowsMediaPlayer1.currentMedia.getItemInfo("Author");
                infocount = 0;
                testFavourite();
                timer1.Enabled = true;
                this.label4.Visible = true;
                this.label6.Visible = false;
            }
        }
        private void testFavourite() {
            XmlNodeList nodelist = root.SelectNodes("favourites/favourite");
            for (int i = 0; i < nodelist.Count; i++)
            {
                if ((nodelist.Item(i) != null) && (nodelist.Item(i).FirstChild.InnerText == info))
                {
                    label10.Visible = true;
                    label9.Visible = false;
                    break;
                }
                else
                {
                    label10.Visible = false;
                    label9.Visible = true;
                }
            }
        }
        private void axWindowsMediaPlayer1_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            SetWindowRegion();
        }
        public void SetWindowRegion() { //绘制窗体圆角
            System.Drawing.Drawing2D.GraphicsPath FormPath; 
            FormPath = new System.Drawing.Drawing2D.GraphicsPath(); 
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height); 
            FormPath = GetRoundedRectPath(rect, 10); 
            this.Region = new Region(FormPath); 
        }           
        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)        {       
            int diameter = radius;       
            Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));  
            GraphicsPath path = new GraphicsPath();                       
            path.AddArc(arcRect, 180, 90);//左上角         
            arcRect.X = rect.Right - diameter;//右上角 
            path.AddArc(arcRect, 270, 90); 
            arcRect.Y = rect.Bottom - diameter;// 右下角  
            path.AddArc(arcRect, 0, 90);     
            arcRect.X = rect.Left;// 左下角  
            path.AddArc(arcRect, 90, 90);   
            path.CloseFigure();     
            return path;  
        }

        private void trackBar1_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.axWindowsMediaPlayer1.URL != "")
            {
                timer1.Enabled = false;   //停止检测播放进度
                this.axWindowsMediaPlayer1.Ctlcontrols.pause();  //暂停播放文件
            }
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            if (this.axWindowsMediaPlayer1.URL != ""){
                double newValue = trackBar1.Value * 0.1 * 0.1 * max;//还原播放进度
                this.axWindowsMediaPlayer1.Ctlcontrols.currentPosition = newValue;//重置播放进度
                this.axWindowsMediaPlayer1.Ctlcontrols.play();//按进度
                timecount = (int)newValue;
                timer1.Enabled = true;
            }
            else{
                trackBar1.Value = 0;
            }
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {//删除喜爱
            if (this.axWindowsMediaPlayer1.URL != ""){
                label10.Visible = false;
                label9.Visible = true;
                //删除xml记录
                XmlNodeList nodelist = root.SelectNodes("favourites/favourite");
                for (int i = 0; i < nodelist.Count; i++)
                {
                    if ((nodelist.Item(i) != null) && (nodelist.Item(i).FirstChild.InnerText == info))
                    {
                        favouritenode.RemoveChild(nodelist.Item(i));
                    }
                }
                xml.Save(@"compent.xml");
            }
        }

        private void label9_Click(object sender, EventArgs e)
        {//喜爱按钮
            string str=this.axWindowsMediaPlayer1.URL;
            if (str != "") {
                str = str.Substring(str.LastIndexOf('\\') + 1, str.LastIndexOf('.') - str.LastIndexOf('\\') - 1);
                if(this.listBox1.Items.Contains(str)){
                    
                    label10.Visible = true;
                    label9.Visible = false;
                    string musicname = info;
                    string fileuri=files[playindex];
                    //数据存入xml
                    XmlNode favourites = root.SelectSingleNode("//favourites");
                    XmlElement elem = xml.CreateElement("favourite");
                    elem.SetAttribute("id", (fav_last_id + 1).ToString());
                    fav_last_id++;
                    elem.InnerXml = "<name>" + musicname + "</name>" + "<uri>" + @fileuri + "</uri>";
                    favourites.AppendChild(elem);
                    xml.Save(@"compent.xml");
                }
            }
        }

        private void 导入本地音乐ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "请选择本地音乐所在文件夹";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string fileuri = fbd.SelectedPath;
                pathelement.SetAttribute("uri", fileuri);
                xml.Save(@"compent.xml");
                getMusicList();
            }
        }

        private void 关于AbbMusicPlayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("             :::Infomation:::" + "\n" + "Version:1.0.0" + "\n" + "Email：185239060@qq.com" + "\n" + "My Web：www.aobaba.cn");
        }

        private void label11_Click(object sender, EventArgs e)
        {
            getMusicList();
        }

        private void label12_Click(object sender, EventArgs e)
        {
            getFavouriteMusicList();
        }
        //获取播放列表
        private void getMusicList() {
            this.listBox1.Items.Clear();
            string path="";
            if (pathelement.GetAttribute("uri") != "")
            {
                path = @pathelement.GetAttribute("uri");
                files = Directory.GetFiles(path, "*.mp3");
                for (int i = 0; i < files.Length; i++)
                {
                    this.listBox1.Items.Add(files[i].Substring(files[i].LastIndexOf('\\') + 1, files[i].LastIndexOf('.') - files[i].LastIndexOf('\\') - 1));
                }
            }
            getSelectedMusic();
        }
        //获取喜爱列表
        private void getFavouriteMusicList()
        {
            this.listBox1.Items.Clear();
            XmlNodeList nodelist = root.SelectNodes("favourites/favourite");
            for (int i = 0; i < nodelist.Count; i++)
            {
                files[i] = nodelist.Item(i).LastChild.InnerText;
                this.listBox1.Items.Add(nodelist.Item(i).FirstChild.InnerText);
            }
            getSelectedMusic();

        }
        //选中播放音乐
        private void getSelectedMusic() {
            string str=this.axWindowsMediaPlayer1.URL;
            if (str != "") { 
                str = str.Substring(str.LastIndexOf('\\') + 1, str.LastIndexOf('.') - str.LastIndexOf('\\') - 1);
                listBox1.SelectedIndex =listBox1.FindString(str);
                playindex = listBox1.FindString(str);
            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void 设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {//循环按钮点击变为随机
            label15.Visible = false;
            label14.Visible = true;
            randomplay = 1;
        }

        private void label14_Click(object sender, EventArgs e)
        {//随机按钮点击变为单曲循环
            label16.Visible = true;
            label14.Visible = false;
            randomplay = 2;
        }

        private void label16_Click(object sender, EventArgs e)
        {//单曲循环点击变为循环
            label16.Visible = false;
            label15.Visible = true;
            randomplay = 0;
        }

        private void label17_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {

            }
            
            
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
