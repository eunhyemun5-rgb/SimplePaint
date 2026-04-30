
namespace SimplePaint
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.Windows.Forms;
    public partial class Form1 : Form
    {
        enum ToolType { Line, Rectangle, Circle }  // 사용할 도형 타입
        private Bitmap canvasBitmap;               // 실제 그림이 저장되는 비트맵
        private Graphics canvasGraphics;           // 비트맵 위에 그리기 위한 객체
        private bool isDrawing = false;            // 현재 드래그 중인지 여부
        private Point startPoint;                  // 드래그 시작점
        private Point endPoint;                    // 드래그 끝점
        private ToolType currentTool = ToolType.Line;  // 현재 선택된 도형
        private Color currentColor = Color.Black;      // 현재 색상
        private int currentLineWidth = 2;              // 현재 선 두께
        public Form1()
        {
            InitializeComponent();
            // [과제 4] 스크롤바 기능을 위해 PictureBox의 속성 설정
            // picCanvas가 들어있는 Panel 컨테이너의 AutoScroll을 True로 설정해야 합니다.
            picCanvas.SizeMode = PictureBoxSizeMode.AutoSize;

            // 초기 캔버스 설정
            InitializeCanvas(picCanvas.Width, picCanvas.Height);
            // 마우스 이벤트 연결
            picCanvas.MouseDown += PicCanvas_MouseDown;
            picCanvas.MouseMove += PicCanvas_MouseMove;
            picCanvas.MouseUp += PicCanvas_MouseUp;
            picCanvas.Paint += PicCanvas_Paint;
            // 도형 선택 버튼 이벤트 연결
            btnLine.Click += (s, e) => currentTool = ToolType.Line;
            btnRectangle.Click += (s, e) => currentTool = ToolType.Rectangle;
            btnCircle.Click += (s, e) => currentTool = ToolType.Circle;
            // 색상 및 선 두께 설정
            cmbColor.SelectedIndexChanged += cmbColor_SelectedIndexChanged;
            cmbColor.SelectedIndex = 0;
            trbLineWidth.ValueChanged += (s, e) => currentLineWidth = trbLineWidth.Value;
            // [과제 3 & 4] 버튼 이벤트 연결
            if (btnSaveFile != null) btnSaveFile.Click += btnSave_Click;
            if (btnOpenFile != null) btnOpenFile.Click += btnOpen_Click; // 열기 버튼 연결
        }
        // 캔버스 초기화 및 메모리 할당 공통 함수
        private void InitializeCanvas(int width, int height)
        {
            if (canvasBitmap != null) canvasBitmap.Dispose();
            if (canvasGraphics != null) canvasGraphics.Dispose();
            canvasBitmap = new Bitmap(width, height);
            canvasGraphics = Graphics.FromImage(canvasBitmap);
            canvasGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            canvasGraphics.Clear(Color.White);
            picCanvas.Image = canvasBitmap;
        }
        // --- [과제 4] 외부 이미지 파일 열기 기능 구현 ---
        private void btnOpen_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp";
                openFileDialog.Title = "외부 이미지 불러오기";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 1. 외부 이미지 읽어오기
                    using (Image loadedImage = Image.FromFile(openFileDialog.FileName))
                    {
                        // 2. 이미지 크기에 맞춰 캔버스 크기 조정
                        // (PictureBox.SizeMode이 AutoSize이므로 이미지가 크면 스크롤바가 생깁니다)
                        InitializeCanvas(loadedImage.Width, loadedImage.Height);
                        // 3. 읽어온 이미지를 캔버스 비트맵 위에 그리기 (캔버스로 사용)
                        canvasGraphics.DrawImage(loadedImage, 0, 0, loadedImage.Width, loadedImage.Height);
                    }
                    picCanvas.Invalidate();
                    MessageBox.Show("이미지를 캔버스로 불러왔습니다. 그 위에 그림을 그리세요!");
                }
            }
        }
        // --- 마우스 드래그 및 그리기 로직 ---
        private void PicCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            isDrawing = true;
            startPoint = e.Location;
        }
        private void PicCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDrawing) return;
            endPoint = e.Location;
            picCanvas.Invalidate();
        }
        private void PicCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isDrawing) return;
            isDrawing = false;
            endPoint = e.Location;
            using (Pen pen = new Pen(currentColor, currentLineWidth))
            {
                DrawShape(canvasGraphics, pen, startPoint, endPoint);
            }
            picCanvas.Invalidate();
        }
        private void PicCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (!isDrawing) return;
            using (Pen previewPen = new Pen(currentColor, currentLineWidth))
            {
                previewPen.DashStyle = DashStyle.Dash;
                DrawShape(e.Graphics, previewPen, startPoint, endPoint);
            }
        }
        private void DrawShape(Graphics g, Pen pen, Point p1, Point p2)
        {
            Rectangle rect = GetRectangle(p1, p2);
            switch (currentTool)
            {
                case ToolType.Line: g.DrawLine(pen, p1, p2); break;
                case ToolType.Rectangle: g.DrawRectangle(pen, rect); break;
                case ToolType.Circle: g.DrawEllipse(pen, rect); break;
            }
        }
        // --- 파일 저장 기능 ---
        private void btnSave_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "PNG Image|*.png|JPeg Image|*.jpg|Bitmap Image|*.bmp";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ImageFormat format = ImageFormat.Png;
                    string ext = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();
                    if (ext == ".jpg") format = ImageFormat.Jpeg;
                    else if (ext == ".bmp") format = ImageFormat.Bmp;
                    canvasBitmap.Save(saveFileDialog.FileName, format);
                    MessageBox.Show("그림이 파일로 저장되었습니다.");
                }
            }
        }
        // --- 유틸리티 함수 ---
        private void cmbColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbColor.SelectedIndex)
            {
                case 0: currentColor = Color.Black; break;
                case 1: currentColor = Color.Red; break;
                case 2: currentColor = Color.Blue; break;
                case 3: currentColor = Color.Green; break;
            }
        }
        private Rectangle GetRectangle(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y),
                Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
        }
    }
}