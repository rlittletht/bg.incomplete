using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using Microsoft.Win32;
//using AxSHDocVw;
//using MSHTML;

using UWin32;

namespace UWin32
{
	public class user32
	{
		[DllImport("user32.dll", EntryPoint="TrackMouseEvent", SetLastError=true, CharSet=CharSet.Auto)]
		public static extern bool TrackMouseEvent(
		[In, Out, MarshalAs(UnmanagedType.Struct)] ref TRACKMOUSEEVENT lpEventTrack);

		[StructLayout(LayoutKind.Sequential)]
		public struct TRACKMOUSEEVENT
		{
			[MarshalAs(UnmanagedType.U4)]
			public int cbSize;
			[MarshalAs(UnmanagedType.U4)]
				public int dwFlags;
			public IntPtr hwndTrack;
			[MarshalAs(UnmanagedType.U4)]
			public int dwHoverTime;
		}
	}

}

namespace bg
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class BgGraph : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox m_picbUpper;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.HScrollBar m_sbhUpper;
		private object m_oData;
		private System.Windows.Forms.Button m_pbPrint;
		private Hover m_ch = null;
		private GrapherParams m_gp;
		private System.Windows.Forms.VScrollBar m_sbvUpper;
		private System.Windows.Forms.PictureBox m_picbLower;
		private System.Windows.Forms.HScrollBar m_sbhLower;
		private System.Windows.Forms.VScrollBar m_sbvLower;

		/* B G  G R A P H */
		/*----------------------------------------------------------------------------
			%%Function: BgGraph
			%%Qualified: bg.BgGraph.BgGraph
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public BgGraph()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//   

			m_gp.dBgLow = 30.0;
			m_gp.dBgHigh = 220.0;
			m_gp.nHalfDays = 14;
			m_gp.nIntervals = 19;
			m_gp.fShowMeals = false;
			m_bvUpper = BoxView.Graph;
			m_bvLower = BoxView.Log;

			m_sbvUpper.Tag = m_picbUpper;
			m_sbvLower.Tag = m_picbLower;

			m_sbhUpper.Tag = m_picbUpper;
			m_sbhLower.Tag = m_picbLower;

			SetupViews(this.ClientSize.Height);
		}

		/* S E T U P  V I E W S */
		/*----------------------------------------------------------------------------
			%%Function: SetupViews
			%%Qualified: bg.BgGraph.SetupViews
			%%Contact: rlittle
			
		----------------------------------------------------------------------------*/
		void SetupViews(int nHeight)
		{
			int nMarginTop = 32;
			int nMarginBottom = 13;
			int nMarginBetween = 0;
			int nPctUpper = 68;
			int nPctLower = 32;
			int nHeightAvail = (nHeight - nMarginTop - nMarginBottom - m_sbhUpper.Height - m_sbhLower.Height);

			if (m_bvUpper == BoxView.None)
				{
				nPctUpper = 0;
				nPctLower = 100;
				m_picbUpper.Visible = false;
				m_sbhUpper.Visible = false;
				m_sbvUpper.Visible = false;
				}
			if (m_bvLower == BoxView.None)
				{
				nPctLower = 0;
				nPctUpper = 100;
				m_picbLower.Visible = false;
				m_sbhLower.Visible = false;
				m_sbvLower.Visible = false;
				}

			m_picbUpper.Location = new Point(m_picbUpper.Location.X, nMarginTop);
			m_picbUpper.Height = (nHeightAvail * nPctUpper) / 100;

			m_sbhUpper.Location = new Point(m_sbhUpper.Location.X, m_picbUpper.Location.Y + m_picbUpper.Height);
			m_sbvUpper.Location = new Point(m_sbvUpper.Location.X, m_picbUpper.Location.Y);
			m_sbvUpper.Height = m_picbUpper.Height;

			m_picbLower.Location = new Point(m_picbLower.Location.X, m_sbhUpper.Location.Y + m_sbhUpper.Height + nMarginBetween);
			m_picbLower.Height = (nHeightAvail * nPctLower) / 100;

			m_sbhLower.Location = new Point(m_sbhLower.Location.X, m_picbLower.Location.Y + m_picbLower.Height);
			m_sbvLower.Location = new Point(m_sbvLower.Location.X, m_picbLower.Location.Y);
			m_sbvLower.Height = m_picbLower.Height;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.m_picbUpper = new System.Windows.Forms.PictureBox();
			this.m_sbhUpper = new System.Windows.Forms.HScrollBar();
			this.m_pbPrint = new System.Windows.Forms.Button();
			this.m_sbvUpper = new System.Windows.Forms.VScrollBar();
			this.m_picbLower = new System.Windows.Forms.PictureBox();
			this.m_sbhLower = new System.Windows.Forms.HScrollBar();
			this.m_sbvLower = new System.Windows.Forms.VScrollBar();
			this.SuspendLayout();
			// 
			// m_picbUpper
			// 
			this.m_picbUpper.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_picbUpper.BackColor = System.Drawing.SystemColors.Window;
			this.m_picbUpper.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_picbUpper.Location = new System.Drawing.Point(16, 32);
			this.m_picbUpper.Name = "m_picbUpper";
			this.m_picbUpper.Size = new System.Drawing.Size(648, 176);
			this.m_picbUpper.TabIndex = 0;
			this.m_picbUpper.TabStop = false;
			this.m_picbUpper.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintGraph);
			this.m_picbUpper.MouseHover += new System.EventHandler(this.HoverGraph);
			this.m_picbUpper.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HandleMouse);
			// 
			// m_sbhUpper
			// 
			this.m_sbhUpper.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_sbhUpper.Location = new System.Drawing.Point(16, 392);
			this.m_sbhUpper.Name = "m_sbhUpper";
			this.m_sbhUpper.Size = new System.Drawing.Size(648, 17);
			this.m_sbhUpper.SmallChange = 10;
			this.m_sbhUpper.TabIndex = 1;
			this.m_sbhUpper.Visible = false;
			this.m_sbhUpper.ValueChanged += new System.EventHandler(this.ScrollPaint);
			// 
			// m_pbPrint
			// 
			this.m_pbPrint.Location = new System.Drawing.Point(600, 8);
			this.m_pbPrint.Name = "m_pbPrint";
			this.m_pbPrint.TabIndex = 2;
			this.m_pbPrint.Text = "Print";
			this.m_pbPrint.Click += new System.EventHandler(this.PrintGraph);
			this.m_pbPrint.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			// 
			// m_sbvUpper
			// 
			this.m_sbvUpper.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_sbvUpper.Location = new System.Drawing.Point(664, 32);
			this.m_sbvUpper.Name = "m_sbvUpper";
			this.m_sbvUpper.Size = new System.Drawing.Size(16, 360);
			this.m_sbvUpper.TabIndex = 3;
			this.m_sbvUpper.ValueChanged += new System.EventHandler(this.ScrollVertPaint);
			// 
			// m_picbLower
			// 
			this.m_picbLower.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_picbLower.BackColor = System.Drawing.SystemColors.Window;
			this.m_picbLower.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.m_picbLower.Location = new System.Drawing.Point(16, 416);
			this.m_picbLower.Name = "m_picbLower";
			this.m_picbLower.Size = new System.Drawing.Size(648, 144);
			this.m_picbLower.TabIndex = 4;
			this.m_picbLower.TabStop = false;
			this.m_picbLower.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintGraph);
			// 
			// m_sbhLower
			// 
			this.m_sbhLower.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_sbhLower.Location = new System.Drawing.Point(16, 560);
			this.m_sbhLower.Name = "m_sbhLower";
			this.m_sbhLower.Size = new System.Drawing.Size(648, 17);
			this.m_sbhLower.SmallChange = 10;
			this.m_sbhLower.TabIndex = 5;
			this.m_sbhLower.Visible = false;
			this.m_sbhLower.ValueChanged += new System.EventHandler(this.ScrollPaint);
			// 
			// m_sbvLower
			// 
			this.m_sbvLower.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.m_sbvLower.Location = new System.Drawing.Point(664, 416);
			this.m_sbvLower.Name = "m_sbvLower";
			this.m_sbvLower.Size = new System.Drawing.Size(16, 144);
			this.m_sbvLower.TabIndex = 6;
			this.m_sbvLower.ValueChanged += new System.EventHandler(this.ScrollVertPaint);
			// 
			// BgGraph
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(696, 590);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.m_sbvLower,
																		  this.m_sbhLower,
																		  this.m_picbLower,
																		  this.m_sbvUpper,
																		  this.m_pbPrint,
																		  this.m_sbhUpper,
																		  this.m_picbUpper});
			this.Name = "BgGraph";
			this.Text = "Form1";
			this.SizeChanged += new System.EventHandler(this.HandleSizeChange);
			this.ResumeLayout(false);

		}
		#endregion

		/* S E T  P B  D A T A  P O I N T S */
		/*----------------------------------------------------------------------------
			%%Function: SetPbDataPoints
			%%Qualified: bg.BgGraph.SetPbDataPoints
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		void SetPbDataPoints(PictureBox pb, VScrollBar sbv, HScrollBar sbh)
		{
			if (pb.Tag != null)
				((GraphicBox)pb.Tag).SetDataPoints(m_oData, sbv, sbh);
		}


		/* S E T  P B  D A T A  P O I N T S */
		/*----------------------------------------------------------------------------
			%%Function: SetPbDataPoints
			%%Qualified: bg.BgGraph.SetPbDataPoints
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public void SetDataPoints(object oData)
		{
			m_oData = oData;
			SetPbDataPoints(m_picbUpper, m_sbvUpper, m_sbhUpper);
			SetPbDataPoints(m_picbLower, m_sbvLower, m_sbhLower);
		}

		/* S E T  P B  B O U N D S */
		/*----------------------------------------------------------------------------
			%%Function: SetPbBounds
			%%Qualified: bg.BgGraph.SetPbBounds
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		void SetPbBounds(PictureBox pb)
		{
			if (BvFromPb(pb) != BoxView.None)
				((GraphicBox)pb.Tag).SetProps(m_gp);
		}

		/* S E T  B O U N D S */
		/*----------------------------------------------------------------------------
			%%Function: SetBounds
			%%Qualified: bg.BgGraph.SetBounds
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public void SetBounds(double dLow, double dHigh, int nDays, int nBgIntervals, bool fShowMeals, bool fLandscape)
		{
			m_gp.dBgLow = dLow;
			m_gp.dBgHigh = dHigh;
			m_gp.nHalfDays = nDays * 2;
			m_gp.nIntervals = nBgIntervals;
			m_gp.fShowMeals = fShowMeals;
			m_gp.fLandscape = fLandscape;
			m_gp.fGraphAvg = true; // false;
			SetPbBounds(m_picbUpper);
			SetPbBounds(m_picbLower);
			AutosizeGraph(m_bvUpper, m_bvLower, (GraphicBox)m_picbUpper.Tag, (GraphicBox)m_picbLower.Tag, ref m_gp);
		}

		public enum BoxView
		{
			None,
			Graph,
			Log,
			Meal
		};

		public void AutosizeGraph(BoxView bvUpper, BoxView bvLower, GraphicBox gbUpper, GraphicBox gbLower, ref GrapherParams gp)
		{
			int nDays;

			if (bvUpper == BoxView.Log)
				{
				// get the number of lines expected in this item
				nDays = gbUpper.GetDaysPerPage();
				if (bvLower == BoxView.Graph)
					{
					gbLower.SetDaysPerPage(nDays);
					gp.nHalfDays = nDays * 2;
					}
				}
			else if (bvLower == BoxView.Log)
				{
				// get the number of lines expected in this item
				nDays = gbLower.GetDaysPerPage();
				if (bvUpper == BoxView.Graph)
					{
					gbUpper.SetDaysPerPage(nDays);
					gp.nHalfDays = nDays * 2;
					}
				}
		}


		/* S E T  G R A P H I C  V I E W S */
		/*----------------------------------------------------------------------------
			%%Function: SetGraphicViews
			%%Qualified: bg.BgGraph.SetGraphicViews
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public void SetGraphicViews(BoxView bvUpper, BoxView bvLower)
		{
			if (bvUpper == BoxView.None && bvLower == BoxView.None)
				throw(new Exception("Illegal BoxView parameters"));

			m_bvUpper = bvUpper;
			m_bvLower = bvLower;
			SetupViews(this.ClientSize.Height);

			Graphics gr = this.CreateGraphics();
			RectangleF rectfUpper = new RectangleF(Reporter.DxpFromDxa(gr, 100), 
												   Reporter.DypFromDya(gr, 100), 
												   m_picbUpper.Width - Reporter.DxpFromDxa(gr, 200), 
												   m_picbUpper.Height - Reporter.DypFromDya(gr, 200));

			RectangleF rectfLower = new RectangleF(Reporter.DxpFromDxa(gr, 100), 
												   Reporter.DypFromDya(gr, 100),
												   m_picbLower.Width - Reporter.DxpFromDxa(gr, 200), 
												   m_picbLower.Height - Reporter.DypFromDya(gr, 200));

			switch (bvUpper)
				{
				case BoxView.Log:
					m_picbUpper.Tag = new Reporter(rectfUpper, gr);
					break;
				case BoxView.Graph:
					m_picbUpper.Tag = new Grapher(rectfUpper, gr);
					break;
				case BoxView.Meal:
					m_picbUpper.Tag = new MealCharter(rectfUpper, gr);
					break;
				}

			switch (bvLower)
				{
				case BoxView.Log:
					m_picbLower.Tag = new Reporter(rectfLower, gr);
					break;
				case BoxView.Graph:
					m_picbLower.Tag = new Grapher(rectfLower, gr);
					break;
				case BoxView.Meal:
					m_picbLower.Tag = new MealCharter(rectfLower, gr);
					break;
				}
		}

		BoxView m_bvUpper;
		BoxView m_bvLower;

		/* B V  F R O M  S T R I N G */
		/*----------------------------------------------------------------------------
			%%Function: BvFromString
			%%Qualified: bg.BgGraph.BvFromString
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		static public BoxView BvFromString(string s)
		{
			if (String.Compare(s, "None", true) == 0)
				return BoxView.None;
			else if (String.Compare(s, "Graph", true) == 0)
				return BoxView.Graph;
			else if (String.Compare(s, "Log", true) == 0)
				return BoxView.Log;
			else if (String.Compare(s, "Meal", true) == 0)
				return BoxView.Meal;

			return BoxView.None;
		}


		/* P A I N T  G R A P H */
		/*----------------------------------------------------------------------------
			%%Function: PaintGraph
			%%Qualified: bg.BgGraph.PaintGraph
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		private void PaintGraph(object sender, System.Windows.Forms.PaintEventArgs e) 
		{
			PictureBox pb = (PictureBox)sender;

			e.Graphics.Clear(pb.BackColor);

			if (pb.Tag != null)
				((GraphicBox)pb.Tag).Paint(e.Graphics);
		}


		bool m_fInPaint = false;
		/* S C R O L L  P A I N T */
		/*----------------------------------------------------------------------------
			%%Function: ScrollPaint
			%%Qualified: bg.BgGraph.ScrollPaint
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		private void ScrollPaint(object sender, System.EventArgs e) 
		{
			if (m_fInPaint)
				return;

			m_fInPaint = true;
			HScrollBar sbh = (HScrollBar)sender;

			PictureBox pb = (PictureBox)sbh.Tag;

			if (BvFromPb(pb) == BoxView.Graph)
				{
				// its a report
				Grapher grph = (Grapher)pb.Tag;
				int iFirstQuarter = -1;

				grph.SetFirstFromScroll(iFirstQuarter = sbh.Value);

				DateTime dttm = grph.GetFirstDateTime();

				SetViewDateTimeScroll(m_picbUpper, m_sbhUpper, m_sbvUpper, dttm, iFirstQuarter);
				SetViewDateTimeScroll(m_picbLower, m_sbhLower, m_sbvLower, dttm, iFirstQuarter);

				pb.Invalidate();
				}
			m_fInPaint = false;
		}

		/* P I C T U R E  B O X  S I Z E  C H A N G E */
		/*----------------------------------------------------------------------------
			%%Function: PictureBoxSizeChange
			%%Qualified: bg.BgGraph.PictureBoxSizeChange
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		void PictureBoxSizeChange(PictureBox pb, HScrollBar sbh, VScrollBar sbv)
		{
			Graphics gr = this.CreateGraphics();
			RectangleF rcf = new RectangleF(Reporter.DxpFromDxa(gr, 100), 
											Reporter.DypFromDya(gr, 100), 
											pb.Width - Reporter.DxpFromDxa(gr, 200), 
											pb.Height - Reporter.DypFromDya(gr, 200));

			if (pb.Tag != null)
				{
				int iFirst = ((GraphicBox)pb.Tag).GetFirstForScroll();
				GraphicBox gb = null;
	
				if (BvFromPb(pb) == BoxView.Graph)
					pb.Tag = gb = (GraphicBox) new Grapher(rcf, gr);
				else if (BvFromPb(pb) == BoxView.Log)
					pb.Tag = gb = (GraphicBox) new Reporter(rcf, gr);
	
				if (gb != null)
					{
					gb.SetProps(m_gp);
					gb.SetDataPoints(m_oData, sbv, sbh);
					gb.Calc();
					gb.SetFirstFromScroll(iFirst);
					}
	
				pb.Invalidate();
				}
		}

		/* H A N D L E  S I Z E  C H A N G E */
		/*----------------------------------------------------------------------------
			%%Function: HandleSizeChange
			%%Qualified: bg.BgGraph.HandleSizeChange
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		private void HandleSizeChange(object sender, System.EventArgs e) 
		{
			SetupViews(this.ClientSize.Height);
			AutosizeGraph(m_bvUpper, m_bvLower, (GraphicBox)m_picbUpper.Tag, (GraphicBox)m_picbLower.Tag, ref m_gp);
			PictureBoxSizeChange(m_picbUpper, m_sbhUpper, m_sbvUpper);
			PictureBoxSizeChange(m_picbLower, m_sbhLower, m_sbvLower);
		}

		/* C A L C  P I C T U R E  B O X */
		/*----------------------------------------------------------------------------
			%%Function: CalcPictureBox
			%%Qualified: bg.BgGraph.CalcPictureBox
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		void CalcPictureBox(PictureBox pb, HScrollBar sbh)
		{
			if (pb.Tag != null)
				((GraphicBox)pb.Tag).Calc();
		}

		/* C A L C  G R A P H */
		/*----------------------------------------------------------------------------
			%%Function: CalcGraph
			%%Qualified: bg.BgGraph.CalcGraph
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public void CalcGraph()
		{
			CalcPictureBox(m_picbUpper, m_sbhUpper);
			CalcPictureBox(m_picbLower, m_sbhLower);
		}

		/* H O V E R  G R A P H */
		/*----------------------------------------------------------------------------
			%%Function: HoverGraph
			%%Qualified: bg.BgGraph.HoverGraph
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		private void HoverGraph(object sender, System.EventArgs e)
		{
			PictureBox pb = (PictureBox)sender;

			if (BvFromPb(pb) != BoxView.Graph)
				return;

			Grapher grph = (Grapher)pb.Tag;

			Point ptRaw = Cursor.Position;
			Point pt =  pb.PointToClient(ptRaw);

			PTFI ptfiHit = new PTFI();
			bool fHit = false;
			RectangleF rectfHit;
			object oHit;

			fHit = grph.FHitTest(pt, out oHit, out rectfHit);
			ptfiHit = (PTFI)oHit;
			if (fHit)
				{
				if (m_ch == null)
					m_ch = new Hover();
				m_ch.ShowTip(ptRaw, ptfiHit.bge);
				this.Focus();
				m_fTipShowing = true;
				m_rectfTipHitRegion = rectfHit;
				}

			this.Focus();

			// now lets register for this again
			user32.TRACKMOUSEEVENT tme = new user32.TRACKMOUSEEVENT();

			tme.cbSize = Marshal.SizeOf(tme);
			tme.dwFlags = 1;
			tme.dwHoverTime = -1;
			tme.hwndTrack = pb.Handle;
			user32.TrackMouseEvent(ref tme);
		}

		/* H A N D L E  M O U S E */
		/*----------------------------------------------------------------------------
			%%Function: HandleMouse
			%%Qualified: bg.BgGraph.HandleMouse
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		private void HandleMouse(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (m_fTipShowing == false)
				return;

			if (m_rectfTipHitRegion.Contains(new PointF((float)e.X, (float)e.Y)))
				return;

			m_fTipShowing = false;
			m_ch.Hide();
		}

		private bool m_fTipShowing;
		private RectangleF m_rectfTipHitRegion;

		/* G R A P H  P R I N T  R E G I O N */
		/*----------------------------------------------------------------------------
			%%Function: GraphPrintRegion
			%%Qualified: bg.BgGraph.GraphPrintRegion
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		GraphicBox GraphPrintRegion(RectangleF rcf, RectangleF rcfBanner, PrintPageEventArgs ev, bool fColor)
		{
			Grapher grph = new Grapher(rcf, ev.Graphics);

			grph.SetProps(gpPrint);
			grph.DrawBanner(ev.Graphics, rcfBanner);
//			grph.SetFirstQuarter(grphRef.GetFirstQuarter());
			grph.SetDataPoints(m_oData, null, null);
			grph.SetColor(fColor);
			return (GraphicBox)grph;
		}

		/* L O G  P R I N T  R E G I O N */
		/*----------------------------------------------------------------------------
			%%Function: LogPrintRegion
			%%Qualified: bg.BgGraph.LogPrintRegion
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		GraphicBox LogPrintRegion(RectangleF rcf, PrintPageEventArgs ev, bool fColor)
		{
			Reporter rpt = new Reporter(rcf, ev.Graphics);

			rpt.SetProps(gpPrint);
			rpt.SetDataPoints(m_oData, null, null);
//			rpt.SetFirstLine(rptRef.GetFirstLine());
			rpt.SetColor(fColor);
			return (GraphicBox)rpt;
		}

		void PaintPrintRegion(GraphicBox gb, BoxView bv, Graphics gr, DateTime dttmFirst)
		{
			if (gb == null)
				return;

			gb.Calc();

			if (bv == BoxView.Log)
				gb.SetFirstDateTime(dttmFirst);
			else if (bv == BoxView.Graph)
				gb.SetFirstDateTime(dttmFirst.AddDays(-1.0));

			gb.Paint(gr);
		}

		/* P R I N T  P A G E  H A N D L E R */
		/*----------------------------------------------------------------------------
			%%Function: PrintPageHandler
			%%Qualified: bg.BgGraph.PrintPageHandler
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		private void PrintPageHandler(object sender, PrintPageEventArgs ev)
		{
			Rectangle rectMargins = ev.MarginBounds;
			Rectangle rectPage = ev.PageBounds;
			PrintDocument ppd = (PrintDocument)sender;

			rectMargins = new Rectangle((int)Reporter.DxpFromDxaPrint(ev.Graphics, (float)(ev.MarginBounds.X * 14.40)),
										(int)Reporter.DypFromDyaPrint(ev.Graphics, (float)(ev.MarginBounds.Y * 14.40)),
										(int)Reporter.DxpFromDxaPrint(ev.Graphics, (float)(ev.MarginBounds.Width * 14.40)),
										(int)Reporter.DypFromDyaPrint(ev.Graphics, (float)(ev.MarginBounds.Height * 14.40)));


			// adjust the bottom margin...
			rectMargins.Height -= (int)Reporter.DypFromDyaPrint(ev.Graphics, 70);

			// page size is 8.5 x 11.  we are shooting for 8" x 10.5"

//			ev.Graphics.DrawRectangle(new Pen(new SolidBrush(Color.Blue), 1.0F), rectMargins);
			int nPctUpper = 70;
			int nMarginTop = (int)Reporter.DypFromDyaPrint(ev.Graphics, 25);
			int nMarginBetween = nMarginTop;
			bool fColor = ppd.PrinterSettings.SupportsColor;

			int nHeightTotal = (int)Reporter.DypFromDyaPrint(ev.Graphics, 1440.0F * 10.5F);
			int nWidthTotal = (int)Reporter.DxpFromDxaPrint(ev.Graphics, 1440.0F * 8.0F);
			if (ev.PageSettings.Landscape)
				{
				nWidthTotal = (int)Reporter.DypFromDyaPrint(ev.Graphics, 1440.0F * 10.5F);
				nHeightTotal= (int)Reporter.DxpFromDxaPrint(ev.Graphics, 1440.0F * 8.0F);
				}

			int nHeightAvail = nHeightTotal - nMarginBetween - nMarginTop;
			int nWidth = nWidthTotal - (int)Reporter.DxpFromDxaPrint(ev.Graphics, 10);

			if (m_bvUpper == BoxView.None)
				nPctUpper = 0;
			else if (m_bvLower == BoxView.None)
				nPctUpper = 100;

			// we have to apportion the regions...
			RectangleF rcfUpperBanner = new RectangleF(0, 0, nWidth, nMarginTop);
			RectangleF rcfUpper = new RectangleF(0, rcfUpperBanner.Bottom, nWidth, ((nHeightAvail) * nPctUpper) / 100);
			RectangleF rcfLowerBanner = new RectangleF(0, rcfUpper.Bottom, nWidth, nMarginBetween);
			RectangleF rcfLower = new RectangleF(0, rcfUpper.Bottom + nMarginBetween, nWidth, nHeightAvail - (rcfUpper.Bottom + nMarginBetween * 2));

			// whenever we go to print, if there's a log, then the first date in the log becomes the first date for the graph too

			// paint the upper region
			GraphicBox gbUpper = null;
			GraphicBox gbLower = null;

			switch (m_bvUpper)
				{
				case BoxView.Graph:
					{
					Grapher grph = (Grapher)m_picbUpper.Tag;
					gbUpper = GraphPrintRegion(rcfUpper, rcfUpperBanner, ev, fColor);
					break;
					}
				case BoxView.Log:
					{
					Reporter rpt = (Reporter)m_picbUpper.Tag;
					gbUpper = LogPrintRegion(rcfUpper, ev, fColor);
					break;
					}
				}
			
			switch (m_bvLower)
				{
				case BoxView.Graph:
					{
					Grapher grph = (Grapher)m_picbLower.Tag;
					gbLower = GraphPrintRegion(rcfLower, rcfLowerBanner, ev, fColor);
					break;
					}
				case BoxView.Log:
					{
					Reporter rpt = (Reporter)m_picbLower.Tag;
					gbLower = LogPrintRegion(rcfLower, ev, fColor);
					break;
					}
				}

			AutosizeGraph(m_bvUpper, m_bvLower, gbUpper, gbLower, ref gpPrint);
			PaintPrintRegion(gbUpper, m_bvUpper, ev.Graphics, dttmCurPage);
			PaintPrintRegion(gbLower, m_bvLower, ev.Graphics, dttmCurPage);

			// setup dttmCurPage for the next page
			// again, the log wins
			bool fMore = false;

			if (gbUpper != null)
				{
				fMore = gbUpper.FGetLastDateTimeOnPage(out dttmCurPage);

				// if we are getting the "last date" from a graph region, then
				// don't overlap the last day of page N with the page N+1.
				if (fMore && m_bvUpper == BoxView.Graph)
					dttmCurPage = dttmCurPage.AddDays(1.0f);
				}

			if (m_bvLower == BoxView.Log)
				fMore = gbLower.FGetLastDateTimeOnPage(out dttmCurPage);

			ev.HasMorePages = fMore;
		}

		PrinterSettings m_prtSettings = null;
		PageSettings m_pgSettings = null;

		DateTime dttmCurPage;
		GrapherParams gpPrint;

		/* P R I N T  G R A P H */
		/*----------------------------------------------------------------------------
			%%Function: PrintGraph
			%%Qualified: bg.BgGraph.PrintGraph
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		private void PrintGraph(object sender, System.EventArgs e) 
		{
			PrintDocument ppd = new PrintDocument();
			PrintDialog dlgPrint = new PrintDialog();
			if (m_pgSettings != null)
				{
				ppd.DefaultPageSettings = m_pgSettings;
				}
			else
				{
				ppd.DefaultPageSettings.Landscape = m_gp.fLandscape;
				ppd.DefaultPageSettings.Margins = new Margins(25, 25, 25, 25);
				}

			if (m_prtSettings != null)
				{
				ppd.PrinterSettings = m_prtSettings;
				}
			

			ppd.PrintPage += new PrintPageEventHandler(PrintPageHandler);
			
			dlgPrint.Document = ppd;

			dlgPrint.ShowDialog();
			m_prtSettings = ppd.PrinterSettings;
			m_pgSettings = ppd.DefaultPageSettings;

			dttmCurPage = ((GraphicBox)m_picbUpper.Tag).GetFirstDateTime();
			gpPrint = m_gp;
#if DEBUG
			DateTime dttmOld = dttmCurPage;
			((GraphicBox)m_picbUpper.Tag).SetFirstDateTime(dttmCurPage);
			dttmCurPage = ((GraphicBox)m_picbUpper.Tag).GetFirstDateTime();
			if (DateTime.Compare(dttmOld, dttmCurPage) != 0)
				throw new Exception("SetFirstDateTime identity failed!");
#endif
			if (m_bvLower == BoxView.Log)
				dttmCurPage = ((GraphicBox)m_picbLower.Tag).GetFirstDateTime();

			ppd.Print();

		}

//		int m_iFirstLine;

		/* B V  F R O M  P B */
		/*----------------------------------------------------------------------------
			%%Function: BvFromPb
			%%Qualified: bg.BgGraph.BvFromPb
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		BoxView BvFromPb(PictureBox pb)
		{
			if (pb.Tag == null)
				return BoxView.None;

			if (String.Compare(pb.Tag.GetType().ToString(), "bg.Grapher", true) == 0)
				return BoxView.Graph;
			else if (String.Compare(pb.Tag.GetType().ToString(), "bg.Reporter", true) == 0)
				return BoxView.Log;
			else if (String.Compare(pb.Tag.GetType().ToString(), "bg.MealCharter", true) == 0)
				return BoxView.Meal;
			else
				return BoxView.None;
		}

		/* S E T  V I E W  D A T E  T I M E  S C R O L L */
		/*----------------------------------------------------------------------------
			%%Function: SetViewDateTimeScroll
			%%Qualified: bg.BgGraph.SetViewDateTimeScroll
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		void SetViewDateTimeScroll(PictureBox pb, HScrollBar sbh, VScrollBar sbv, DateTime dttm, int iFirstQuarter)
		{
			if (BvFromPb(pb) == BoxView.Log)
				{
				Reporter rpt = (Reporter)pb.Tag;

				rpt.SetFirstDateTime(dttm);
				sbv.Value = rpt.GetFirstLine();
				}
			else if (BvFromPb(pb) == BoxView.Graph)
				{
				Grapher grph = (Grapher)pb.Tag;

				if (iFirstQuarter >= 0)
					{
					grph.SetFirstFromScroll(iFirstQuarter);
					sbh.Value = iFirstQuarter;
					}
				else
					{
					grph.SetFirstDateTime(dttm.AddDays(-1.0));
					if (grph.GetFirstForScroll() > sbh.Maximum)	 // if we have exceeded the scrolling regions, then we want to act as if we've scrolled to the end
						grph.SetFirstFromScroll(sbh.Value);
					if (grph.GetFirstForScroll() < 0)
						grph.SetFirstFromScroll(0);
					sbh.Value = grph.GetFirstForScroll();
					}
				}
			pb.Invalidate();
		}

		/* S C R O L L  V E R T  P A I N T */
		/*----------------------------------------------------------------------------
			%%Function: ScrollVertPaint
			%%Qualified: bg.BgGraph.ScrollVertPaint
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		private void ScrollVertPaint(object sender, System.EventArgs e)
		{
			if (m_fInPaint)
				return;

			m_fInPaint = true;

			VScrollBar sbv = (VScrollBar)sender;

			PictureBox pb = (PictureBox)sbv.Tag;

			if (BvFromPb(pb) == BoxView.Log)
				{
				// its a report.  scroll both views to this item.
				Reporter rpt = (Reporter)pb.Tag;

				rpt.SetFirstLine(sbv.Value);
				DateTime dttm = rpt.GetFirstDateTime();

				SetViewDateTimeScroll(m_picbUpper, m_sbhUpper, m_sbvUpper, dttm, -1);
				SetViewDateTimeScroll(m_picbLower, m_sbhLower, m_sbvLower, dttm, -1);

				pb.Invalidate();
				}
			m_fInPaint = false;
		}

	}
}
