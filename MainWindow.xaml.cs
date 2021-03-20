//====================================================================================================
//The Free Edition of Instant C# limits conversion output to 100 lines per file.

//To purchase the Premium Edition, visit our website:
//https://www.tangiblesoftwaresolutions.com/order/order-instant-csharp.html
//====================================================================================================

//INSTANT C# NOTE: Formerly VB project-level imports:
using System.Threading.Tasks;
using System.Linq;
using System.Xml.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using Microsoft.Ink;
//Imports System.Windows.Ink

//C:\Program Files (x86)\Common Files\Microsoft Shared\Ink
//添加对 WPF 墨迹分析程序集、IAWinFX.dll、IACore.dll 和 IALoader.dll（这些内容可以在 \Program Files\Reference Assemblies\Microsoft\Tablet PC\v1.7 中找到）的引用。

namespace Ink
{
	internal struct PointInfo
	{
		public Point pt; //点的位置
		public Vector dir; //与上一个点之间的方向
		public float len; //上一个点到这个点的距离
		public Vector perpendicular; //与上一个点之间的方向的垂直
		public System.Drawing.Point ToDrawingPoint()
		{
			return new System.Drawing.Point(Convert.ToInt32(pt.X), Convert.ToInt32(pt.Y));
		}
	}

	public partial class MainWindow
	{
		private RenderTargetBitmap bmp; //渲染到的图片
		private DrawingVisual dv = new DrawingVisual(); //绘制到图片的visual
		private PathGeometry geo; //一笔 存的所有的path
		private List<PathGeometry> geos = new List<PathGeometry>(); //所有笔划
		private Point ptStart; //起始点
		private Point ptEnd; //终点
		private List<PointInfo> ptinfos = new List<PointInfo>(); //点的信息

		//求垂直与输入向量的向量，顺时针，应该还有更好的写法吧
		public Vector Perpendicular(Vector v)
		{
			Vector result = new Vector();

			if (v.Y == 0 && v.X == 1)
			{
				return new Vector(0, -1);
			}
			else if (v.Y == 0 && v.X == -1)
			{
				return new Vector(0, 1);
			}
			else
			{
				if (v.Y > 0)
				{
					result.X = Math.Sqrt(v.Y * v.Y / (v.X * v.X + v.Y * v.Y));
				}
				else
				{
					result.X = -Math.Sqrt(v.Y * v.Y / (v.X * v.X + v.Y * v.Y));
				}
			}
			result.Y = -v.X * result.X / v.Y;
			return result;
		} //顺时针

		private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
		{
			ptEnd = e.MouseDevice.GetPosition(canvas);
			PathGeometry g = new PathGeometry();
			PointInfo infoPrev = ptinfos.Last();
			Point ptPrev = infoPrev.pt;
			float len = 0;
			Point pt1 = new Point();
			Point pt2 = new Point();
			Vector dir0 = infoPrev.dir;
			pt1 = ptPrev + dir0 * lw * -0.5 / infoPrev.len;
			pt2 = ptPrev + dir0 * lw * 0.5 / infoPrev.len;
			PathFigure pf = new PathFigure();
			pf.Segments.Add(new LineSegment(ptPrev, true));
			pf.Segments.Add(new LineSegment(pt1, true));
			pf.Segments.Add(new LineSegment(ptEnd, true));
			pf.Segments.Add(new LineSegment(pt2, true));
			pf.StartPoint = ptPrev;
			pf.IsClosed = true;
			g.Figures.Add(pf);
			geo.AddGeometry(g);
			geos.Add(geo);
			GeometryDrawing drawing = new GeometryDrawing(Brushes.Transparent, new Pen(Brushes.Black, 1), g);
			DrawingContext dc = dv.RenderOpen();
			dc.DrawDrawing(drawing);
			dc.Close();
			bmp.Render(dv);

			PointInfo info = new PointInfo();
			info.pt = ptEnd;
			info.len = len;
			info.dir = new Vector();
			info.perpendicular = new Vector();

			tiwp.Clear();
			tiwp.Add(Regconize()); //添加识别到的文字到控件
			ptinfos.Clear();
		}

		private float lw = 5.0F;

		private void Canvas_MouseMove(object sender, MouseEventArgs e)
		{
			Point pt = e.GetPosition(canvas);
			Vector dir = new Vector();
			double len = 0.0;
			Vector vdir = new Vector();
			PathGeometry g = new PathGeometry();
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				DrawingContext dc = dv.RenderOpen();
				if (ptinfos.Count == 1)
				{
					Point pt1, pt2;
					len = Math.Sqrt((pt.X - ptStart.X) * (pt.X - ptStart.X) + (pt.Y - ptStart.Y) * (pt.Y - ptStart.Y));
					dir = ptStart - pt;
					dir.Normalize();
					vdir = Perpendicular(dir);
					// dir1 = New Vector(0, 0) - dir0
					pt1 = pt + vdir * lw * 0.5 / (double)len;
					pt2 = pt + vdir * lw * -0.5 / (double)len;
					PathFigure pf = new PathFigure();
					pf.Segments.Add(new LineSegment(ptStart, true));
					pf.Segments.Add(new LineSegment(pt1, true));
					pf.Segments.Add(new LineSegment(pt2, true));
					pf.StartPoint = ptStart;
					pf.IsClosed = true;
					g.Figures.Add(pf);
					geo.AddGeometry(g);
				}
				else if(ptinfos != null && ptinfos.Count > 0)
				{
					PointInfo infoPrev = ptinfos.Last();
					Point ptPrev = infoPrev.pt;
					Point pt1, pt2, pt3, pt4;
					len = Math.Sqrt((pt.X - ptPrev.X) * (pt.X - ptPrev.X) + (pt.Y - ptPrev.Y) * (pt.Y - ptPrev.Y));
					dir = ptPrev - pt;
					dir.Normalize();
					Vector dir0 = infoPrev.perpendicular;
					vdir = Perpendicular(dir);
					pt1 = ptPrev + dir0 * lw * 0.5 / (double)infoPrev.len;
					pt2 = ptPrev + dir0 * lw * -0.5 / (double)infoPrev.len;
					pt3 = pt + vdir * lw * 0.5 / (double)len;
					pt4 = pt + vdir * lw * -0.5 / (double)len;
					PathFigure pf = new PathFigure();
					pf.Segments.Add(new LineSegment(ptPrev, true));
					pf.Segments.Add(new LineSegment(pt1, true));
					pf.Segments.Add(new LineSegment(pt3, true));
					pf.Segments.Add(new LineSegment(pt4, true));
					pf.Segments.Add(new LineSegment(pt2, true));
					pf.StartPoint = ptPrev;
					pf.IsClosed = true;
					g.Figures.Add(pf);
					geo.AddGeometry(g);
				}

				GeometryDrawing drawing = new GeometryDrawing(Brushes.Transparent, new Pen(Brushes.Black, 1), g);
				dc.DrawDrawing(drawing);
				dc.Close();
				bmp.Render(dv);

				PointInfo info;
				info.pt = pt;
				info.dir = dir;
				info.len = (float)len;
				info.perpendicular = vdir;
				ptinfos.Add(info);
			}
		}
		private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
		{
			ptinfos.Clear();
			geo = new PathGeometry();
			ptStart = e.MouseDevice.GetPosition(canvas);
			PointInfo info;

			info.pt = ptStart;
			info.dir = new Vector();
			info.perpendicular = new Vector();
			info.len = 0;
			ptinfos.Add(info);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			this.InitializeComponent();
			bmp = new RenderTargetBitmap(512, 512, 96.0, 96.0, PixelFormats.Pbgra32);
			ImageBrush brush = new ImageBrush(bmp);
			brush.Stretch = Stretch.Fill;
			canvas.Background = brush;

			InitRecognizer();
		}
		private Recognizers myRecognizers; // REM 所有识别器
		private Microsoft.Ink.Ink ink = new Microsoft.Ink.Ink(); // REM 墨迹
		private Strokes strokes; // REM 所有的笔画
		private RecognizerContext recognizectx; // REM 识别器上下文
												// Private myInkCollector As InkCollector
												// REM 初始化墨迹识别器
		public void InitRecognizer()
		{
			myRecognizers = new Recognizers();
			strokes = ink.CreateStrokes();
			recognizectx = myRecognizers.GetDefaultRecognizer().CreateRecognizerContext();
			recognizectx.Strokes = strokes;
		}

		private string[] Regconize() // REM 识别 ptinfos 中的点
		{
			List<System.Drawing.Point> listpts = new List<System.Drawing.Point>();
			foreach (var i in ptinfos)
				listpts.Add(i.ToDrawingPoint());
			// REM 每一笔转换成stroke
			Stroke stroke = ink.CreateStroke(listpts.ToArray());
			// REM 添加到识别器的上下文
			recognizectx.Strokes.Add(stroke);
			RecognitionStatus recognizestatus = new RecognitionStatus();
			RecognitionResult recognizeresult = recognizectx.Recognize(out recognizestatus);
			// REM 识别器的所有选择
			RecognitionAlternates recognizealternates = recognizeresult.GetAlternatesFromSelection();
			// REM 列出识别器所识别出的内容
			List<string> result = new List<string>();
			for (var i = 0; i <= recognizealternates.Count - 1; i++)
			{
				string text = recognizealternates[i].ToString();
				// Console.WriteLine(text)
				result.Add(text);
			}
			return result.ToArray();
		}

		private void buttonClear_Click(object sender, RoutedEventArgs e)
		{
			geos.Clear(); // REM 清除所有的笔划
			ptinfos.Clear(); // REM 清除一笔中所有点
			CreateNewBitmap();

			strokes.Clear(); // REM 清除所有的画笔
			tiwp.Clear(); // REM 清除控件中的选择
		}
		private void buttonUndo_Click(object sender, RoutedEventArgs e)
		{
			ptinfos.Clear(); // REM 清除一笔中所有点
			CreateNewBitmap();

			if ((geos.Count > 0))
			{
				strokes.RemoveAt(strokes.Count - 1); // REM 清除上一笔

				geos.RemoveAt(geos.Count - 1); // REM 清除上一笔
				DrawingContext dc = dv.RenderOpen();
				foreach (var g in geos)
				{
					GeometryDrawing drawing = new GeometryDrawing(Brushes.Black, new Pen(Brushes.Black, 1), g);
					dc.DrawDrawing(drawing);
				}
				dc.Close();
				bmp.Render(dv);
			}
		}

		private void tiwp_ImageClick(object sender, EventArgs e)
		{
			Console.WriteLine(sender);
			geos.Clear(); // REM 清除所有的笔划
			ptinfos.Clear(); // REM 清除一笔中所有点
			CreateNewBitmap();

			strokes.Clear(); // REM 清除所有的笔划
			tiwp.Clear(); // REM 清除控件中的选择
		}

		public void CreateNewBitmap()
		{
			bmp = new RenderTargetBitmap(512, 512, 96.0, 96.0, PixelFormats.Pbgra32);
			ImageBrush brush = new ImageBrush(bmp);
			brush.Stretch = Stretch.Fill;
			canvas.Background = brush;
		}
	}
}
