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
using System.Globalization;

namespace Ink
{
	public partial class TextImageWrapPanel
	{
		private List<string> list = new List<string>();
		public void Add(string text)
		{
			list.Add(text);
			MakeImage(text);
		}

		public void Add(string[] texts)
		{
			sv?.ScrollToHome();
			foreach (var t in texts)
			{
				list.Add(t);
				MakeImage(t);
			}
		}

		public void Clear()
		{
			list.Clear();
			wp?.Children.Clear();
		}

		private void MakeImage(string text)
		{
			Image img = new Image();
			img.Width = sv.ActualWidth / 2;
			img.Height = sv.ActualWidth / 2;
			img.Tag = text;
			img.Visibility = System.Windows.Visibility.Visible;

			img.MouseDown += Img_MouseDown;
			RenderTargetBitmap bmp = new RenderTargetBitmap((int)img.Width, (int)img.Height, 96, 96, PixelFormats.Pbgra32);
			DrawingVisual dv = new DrawingVisual();
			DrawingContext dc = dv.RenderOpen();
			System.Drawing.Size size = System.Windows.Forms.TextRenderer.MeasureText(text, new System.Drawing.Font("微软雅黑", 36));

			dc.DrawText(new FormattedText(text, CultureInfo.GetCultureInfo("zh-cn"), System.Windows.FlowDirection.LeftToRight, new Typeface("微软雅黑"), 36, Brushes.Black), new Point((img.Width - size.Width) / 2.0, (img.Height - size.Height) / 2.0));
			dc.Close();
			bmp.Render(dv);
			img.Source = bmp;
			wp.Children.Add(img);
		}

		private void Img_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Image img = (Image)sender;
			if (ImageClick != null)
				ImageClick(img.Tag, e);
		}

		public delegate void ImageClickEventHandler(object sender, EventArgs e);
		public event ImageClickEventHandler ImageClick;

	}

}