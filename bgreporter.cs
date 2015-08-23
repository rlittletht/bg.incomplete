using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Xml;
using System.Globalization;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using UWin32;
using System.Threading;
using System.Text;
using System.Diagnostics;

namespace bg
{
//	 ______ _______  _____   _____   ______ _______ _______  ______
//	|_____/ |______ |_____] |     | |_____/    |    |______ |_____/
//	|    \_ |______ |       |_____| |    \_    |    |______ |    \_

public class Reporter : GraphicBox
{
	//  _  _ ____ _  _ ___  ____ ____     _  _ ____ ____ _ ____ ___  _    ____ ____
	//  |\/| |___ |\/| |__] |___ |__/     |  | |__| |__/ | |__| |__] |    |___ [__
	//  |  | |___ |  | |__] |___ |  \      \/  |  | |  \ | |  | |__] |___ |___ ___]

	float m_nHeight;
	float m_nWidth;
	SortedList m_slbge;
	GrapherParams m_cgp;
	double m_dHoursBefore = 2.0;
	double m_dHoursAfter = 1.5;
	RectangleF m_rcfDrawing;
	bool m_fColor = true;

	COLD []m_mpicold;

	const int icolDay = 0;
	const int icolMealBreakfast = 1;
	const int icolMealLunch = 4;
	const int icolMealDinner = 7;
	const int icolBed = 10;
	const int icolComments = 11;

	const int iMealBreakfast = 0;
	const int iMealLunch = 1;
	const int iMealDinner = 2;

	struct PTB // PaintBox
	{
		public Font fontText;
		public SolidBrush brushText;
		public SolidBrush brushHeavy;
		public SolidBrush brushLight;
		public SolidBrush brushLightFill;
		public SolidBrush brushBorderFill;
		public Pen penHeavy;
		public Pen penLight;
	}

	struct MD // MealData
	{
		public int nBefore;
		public int nAfter;
		public int nCarbs;
	};

	PTB m_ptb;

	//	____ _    ___
	//	|__/ |    |  \
	//	|  \ |___ |__/

	class RLD	// ReportLineData
	{
		public string sDay;
		public int nBedReading;
		public string sComment;
		public string sDateHeader;
		public DateTime dttm;
		public MD []mpimd;

		/* R  L  D */
		/*----------------------------------------------------------------------------
			%%Function: RLD
			%%Qualified: bg.Reporter:RLD.RLD
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public RLD()
		{
			sDay = "";
			nBedReading = 0;
			sComment = "";
			sDateHeader = "";
			mpimd = new MD[3];
			mpimd[0].nBefore = mpimd[0].nAfter = mpimd[0].nCarbs;
			mpimd[1].nBefore = mpimd[1].nAfter = mpimd[1].nCarbs;
			mpimd[2].nBefore = mpimd[2].nAfter = mpimd[2].nCarbs;
		}

	};

	public enum BorderType
	{
		None,
		Solid,
		Double
	};

	//	____ ____ _    ___
	//	|    |  | |    |  \
	//	|___ |__| |___ |__/

	public class COLD	// COLumn Definition
	{
		public float xpLeft;
		public float dxpCol;
		public BorderType btLeft;
		public BorderType btRight;

		/* C  O  L  D */
		/*----------------------------------------------------------------------------
			%%Function: COLD
			%%Qualified: bg.Reporter:COLD.COLD
			%%Contact: rlittle

			
		----------------------------------------------------------------------------*/
		public COLD(COLD coldPrev, float dxpLeftSpace, float dxpColIn, BorderType btLeftIn, BorderType btRightIn)
		{
			float xpPrev = 0;
			if (coldPrev != null)
				{
				xpPrev = coldPrev.xpLeft + coldPrev.dxpCol;
				}
			xpLeft = xpPrev + dxpLeftSpace;
			dxpCol = dxpColIn;
			btLeft = btLeftIn;
			btRight = btRightIn;
		}

		/* D X P  R I G H T */
		/*----------------------------------------------------------------------------
			%%Function: dxpRight
			%%Qualified: bg.Reporter:COLD.dxpRight
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public float dxpRight
		{
			get
			{
				return xpLeft + dxpCol;
			}
		}

		/* R C F  F R O M  C O L U M N */
		/*----------------------------------------------------------------------------
			%%Function: RcfFromColumn
			%%Qualified: bg.Reporter:COLD.RcfFromColumn
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public RectangleF RcfFromColumn(float yp, float dyp)
		{
			return new RectangleF(xpLeft, yp, this.dxpRight - xpLeft, dyp);
		}

		/* D R A W  S I N G L E  B O R D E R */
		/*----------------------------------------------------------------------------
			%%Function: DrawSingleBorder
			%%Qualified: bg.Reporter:COLD.DrawSingleBorder
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		void DrawSingleBorder(Graphics gr, Brush brushFill, Pen pen, float xp, float yp, float dyp, BorderType bt)
		{
			switch (bt)
				{
				case BorderType.Solid:
					{
					float dxpAdjustForWidth = pen.Width / 2.0F;

					xp -= dxpAdjustForWidth;
					gr.DrawLine(pen, xp, yp, xp, yp + dyp);
					break;
					}
				case BorderType.Double:
					{
					float penWidth = (float)pen.Width;
					float dxpAdjustForWidth = (penWidth * 3.0F) / 2.0F;

					xp -= dxpAdjustForWidth;
					// clear the region for the border
					gr.FillRectangle(brushFill, xp + penWidth / 2.0F, yp, penWidth * 1.5F, dyp);

					gr.DrawLine(pen, xp, yp, xp, yp + dyp);

					gr.DrawLine(pen, xp + penWidth * 2.0F, yp, xp + penWidth * 2.0F, yp + dyp);
					break;
					}
				}
		}

		/* D R A W  B O R D E R S */
		/*----------------------------------------------------------------------------
			%%Function: DrawBorders
			%%Qualified: bg.Reporter:COLD.DrawBorders
			%%Contact: rlittle

		----------------------------------------------------------------------------*/
		public void DrawBorders(Graphics gr, float ypTop, float dyp, Pen pen, Brush brushFill)
		{
			DrawSingleBorder(gr, brushFill, pen, this.xpLeft, ypTop, dyp, this.btLeft);
			DrawSingleBorder(gr, brushFill, pen, this.xpLeft, ypTop, dyp, this.btRight);
		}
	}


	/* S E T  C O L O R */
	/*----------------------------------------------------------------------------
		%%Function: SetColor
		%%Qualified: bg.Reporter.SetColor
		%%Contact: rlittle
		
	----------------------------------------------------------------------------*/
	public void SetColor(bool fColor)
	{
		m_fColor = fColor;
	}

	/* S E T  F I R S T  F R O M  S C R O L L */
	/*----------------------------------------------------------------------------
		%%Function: SetFirstFromScroll
		%%Qualified: bg.Reporter:COLD.SetFirstFromScroll
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void SetFirstFromScroll(int i)
	{
		SetFirstLine(i);
	}

	/* D X P  F R O M  D X A */
	/*----------------------------------------------------------------------------
		%%Function: DxpFromDxa
		%%Qualified: bg.Reporter.DxpFromDxa
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	static public float DxpFromDxa(Graphics gr, float dxa)
	{
		return (float)(((double)dxa * gr.DpiX) / 1440.0);
	}

	static public float DxpFromDxaPrint(Graphics gr, float dxa)
	{
		return (float)(((double)dxa * 100.0) / 1440.0);
	}

	/* D Y P  F R O M  D Y A */
	/*----------------------------------------------------------------------------
		%%Function: DypFromDya
		%%Qualified: bg.Reporter.DypFromDya
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	static public float DypFromDya(Graphics gr, float dya)
	{
		return (float)(((double)dya * gr.DpiY) / 1440.0);
	}

	static public float DypFromDyaPrint(Graphics gr, float dya)
	{
		return (float)(((double)dya * 100.0) / 1440.0);
	}

	/* S E T  F I R S T  L I N E */
	/*----------------------------------------------------------------------------
		%%Function: SetFirstLine
		%%Qualified: bg.Reporter.SetFirstLine
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void SetFirstLine(int iLine)
	{
		m_iLineFirst = iLine;
	}

	/* G E T  F I R S T  L I N E */
	/*----------------------------------------------------------------------------
		%%Function: GetFirstLine
		%%Qualified: bg.Reporter.GetFirstLine
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public int GetFirstLine()
	{
		return m_iLineFirst;
	}

	/* G E T  F I R S T  F O R  S C R O L L */
	/*----------------------------------------------------------------------------
		%%Function: GetFirstForScroll
		%%Qualified: bg.Reporter.GetFirstForScroll
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public int GetFirstForScroll()
	{
		return m_iLineFirst;
	}

	/* G E T  F I R S T  D A T E  T I M E */
	/*----------------------------------------------------------------------------
		%%Function: GetFirstDateTime
		%%Qualified: bg.Reporter.GetFirstDateTime
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public DateTime GetFirstDateTime()
	{
		return ((RLD)m_plrld[m_iLineFirst]).dttm;
	}

	/* S E T  F I R S T  D A T E  T I M E */
	/*----------------------------------------------------------------------------
		%%Function: SetFirstDateTime
		%%Qualified: bg.Reporter.SetFirstDateTime
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void SetFirstDateTime(DateTime dttm)
	{
		// try to normalize around dttm
		int i, iMax;

		for (i = 0, iMax = m_plrld.Count; i < iMax; i++)
			{
			if (dttm <= ((RLD)m_plrld[i]).dttm)
				break;
			}

		if (i < iMax)
			m_iLineFirst = i;
	}



	/* S E T  C O L  W I D T H */
	/*----------------------------------------------------------------------------
		%%Function: SetColWidth
		%%Qualified: bg.Reporter.SetColWidth
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	void SetColWidth(int iCol, int nPercent, BorderType btLeft, BorderType btRight)
	{
		float dPercent = (m_nWidth * (float)nPercent) / 100.0F;

		if (iCol > 0)
			{
			if (dPercent == 0)
				{
				dPercent = m_rcfDrawing.Right - m_mpicold[iCol - 1].dxpRight;
				}

			float dxpSpace = 0.0F;

			if (m_mpicold[iCol - 1].btRight != BorderType.None)
				{
				dxpSpace = 2.0F;
				}

			m_mpicold[iCol] = new COLD(m_mpicold[iCol - 1], dxpSpace, dPercent, btLeft, btRight);
			}
		else
			{
			m_mpicold[iCol] = new COLD(null, m_rcfDrawing.Left, dPercent, btLeft, btRight);
			}
	}

	/* R E P O R T E R */
	/*----------------------------------------------------------------------------
		%%Function: Reporter
		%%Qualified: bg.Reporter.Reporter
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public Reporter(RectangleF rcfDrawing, Graphics gr)	// int nWidth, int nHeight, 
	{
		// draw the first line
		m_ptb.fontText = new Font("Tahoma", 9);
		m_ptb.brushText = new SolidBrush(Color.Black);
		m_ptb.brushHeavy = new SolidBrush(Color.Blue);
		m_ptb.brushLight = new SolidBrush(Color.LightBlue);
		m_ptb.penHeavy = new Pen(m_ptb.brushHeavy, 1.0F);
		m_ptb.penLight = new Pen(m_ptb.brushLight, 0.5F);
		m_ptb.brushLightFill = new SolidBrush(Color.LightGray);
		m_ptb.brushBorderFill = new SolidBrush(Color.White);

		m_dyLine = gr.MeasureString("M", m_ptb.fontText).Height;
		m_mpicold = new COLD[13];

		m_rcfDrawing = rcfDrawing;
		m_nHeight = rcfDrawing.Height; // nHeight;
		m_nWidth = rcfDrawing.Width; // nWidth;
		m_cgp.dBgLow = 30.0;
		m_cgp.dBgHigh = 220.0;
		m_cgp.nHalfDays = 14;
		m_cgp.nIntervals = 19;
		m_cgp.fShowMeals = false;

		float dy = YFromLine(2) - YFromLine(1);


		m_nLinesPerPage = (int)(m_nHeight / dy) - 1 ;

		Font font = new Font("Tahoma", 8);

		SetColWidth(icolDay, 5, BorderType.Solid, BorderType.None);

		SetColWidth(icolMealBreakfast, 4, BorderType.Double, BorderType.None);
		SetColWidth(icolMealBreakfast + 1, 3, BorderType.Solid, BorderType.None);
		SetColWidth(icolMealBreakfast + 2, 4, BorderType.Solid, BorderType.None);

		SetColWidth(icolMealLunch, 4, BorderType.Double, BorderType.None);
		SetColWidth(icolMealLunch + 1, 3, BorderType.Solid, BorderType.None);
		SetColWidth(icolMealLunch + 2, 4, BorderType.Solid, BorderType.None);

		SetColWidth(icolMealDinner, 4, BorderType.Double, BorderType.None);
		SetColWidth(icolMealDinner + 1, 3, BorderType.Solid, BorderType.None);
		SetColWidth(icolMealDinner + 2, 4, BorderType.Solid, BorderType.None);

		SetColWidth(icolBed, 4, BorderType.Double, BorderType.None);

		SetColWidth(icolComments, 0, BorderType.Double, BorderType.None);
	}

	//	___  ____ _ _  _ ___ _ _  _ ____
	//	|__] |__| | |\ |  |  | |\ | | __
	//	|    |  | | | \|  |  | | \| |__]

	/* P A I N T  G R I D L I N E S */
	/*----------------------------------------------------------------------------
		%%Function: PaintGridlines
		%%Qualified: bg.Reporter.PaintGridlines
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void PaintGridlines(Graphics gr)
	{

		gr.FillRectangle(m_ptb.brushLightFill, m_mpicold[icolMealBreakfast].RcfFromColumn(m_dyLine + m_rcfDrawing.Top, m_nHeight - m_dyLine));
		gr.FillRectangle(m_ptb.brushLightFill, m_mpicold[icolMealBreakfast + 2].RcfFromColumn(m_dyLine + m_rcfDrawing.Top, m_nHeight - m_dyLine));

		gr.FillRectangle(m_ptb.brushLightFill, m_mpicold[icolMealLunch].RcfFromColumn(m_dyLine + m_rcfDrawing.Top, m_nHeight - m_dyLine));
		gr.FillRectangle(m_ptb.brushLightFill, m_mpicold[icolMealLunch + 2].RcfFromColumn(m_dyLine + m_rcfDrawing.Top, m_nHeight - m_dyLine));

		gr.FillRectangle(m_ptb.brushLightFill, m_mpicold[icolMealDinner].RcfFromColumn(m_dyLine + m_rcfDrawing.Top, m_nHeight - m_dyLine));
		gr.FillRectangle(m_ptb.brushLightFill, m_mpicold[icolMealDinner + 2].RcfFromColumn(m_dyLine + m_rcfDrawing.Top, m_nHeight - m_dyLine));

		gr.FillRectangle(m_ptb.brushLightFill, m_mpicold[icolBed].RcfFromColumn(m_dyLine + m_rcfDrawing.Top, m_nHeight - m_dyLine));

		gr.DrawRectangle(m_ptb.penHeavy, m_mpicold[0].xpLeft, m_rcfDrawing.Top, m_nWidth, m_nHeight);

		// draw the column borders
		m_mpicold[icolMealBreakfast].DrawBorders(gr, m_rcfDrawing.Top, m_nHeight, m_ptb.penHeavy, m_ptb.brushBorderFill);
		m_mpicold[icolMealBreakfast + 1].DrawBorders(gr, m_rcfDrawing.Top + m_dyLine, m_nHeight - m_dyLine, m_ptb.penLight, m_ptb.brushBorderFill);
		m_mpicold[icolMealBreakfast + 2].DrawBorders(gr, m_rcfDrawing.Top + m_dyLine, m_nHeight - m_dyLine, m_ptb.penLight, m_ptb.brushBorderFill);

		m_mpicold[icolMealLunch].DrawBorders(gr, m_rcfDrawing.Top, m_nHeight, m_ptb.penHeavy, m_ptb.brushBorderFill);
		m_mpicold[icolMealLunch + 1].DrawBorders(gr, m_rcfDrawing.Top + m_dyLine, m_nHeight - m_dyLine, m_ptb.penLight, m_ptb.brushBorderFill);
		m_mpicold[icolMealLunch + 2].DrawBorders(gr, m_rcfDrawing.Top + m_dyLine, m_nHeight - m_dyLine, m_ptb.penLight, m_ptb.brushBorderFill);

		m_mpicold[icolMealDinner].DrawBorders(gr, m_rcfDrawing.Top, m_nHeight, m_ptb.penHeavy, m_ptb.brushBorderFill);
		m_mpicold[icolMealDinner + 1].DrawBorders(gr, m_rcfDrawing.Top + m_dyLine, m_nHeight - m_dyLine, m_ptb.penLight, m_ptb.brushBorderFill);
		m_mpicold[icolMealDinner + 2].DrawBorders(gr, m_rcfDrawing.Top + m_dyLine, m_nHeight - m_dyLine, m_ptb.penLight, m_ptb.brushBorderFill);

		m_mpicold[icolBed].DrawBorders(gr, m_rcfDrawing.Top, m_nHeight, m_ptb.penHeavy, m_ptb.brushBorderFill);
		m_mpicold[icolComments].DrawBorders(gr, m_rcfDrawing.Top, m_nHeight, m_ptb.penHeavy, m_ptb.brushBorderFill);

		gr.DrawLine(m_ptb.penHeavy, m_mpicold[0].xpLeft, m_dyLine + m_rcfDrawing.Top, m_nWidth + m_mpicold[0].xpLeft, m_dyLine + m_rcfDrawing.Top);

	}

	/* P A I N T  H E A D E R */
	/*----------------------------------------------------------------------------
		%%Function: PaintHeader
		%%Qualified: bg.Reporter.PaintHeader
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void PaintHeader(Graphics gr)
	{
		float y = YFromLine(0);

		DrawTextInColumn(gr, "day", m_ptb.fontText, m_ptb.brushText, 0, y, 1, HorizontalAlignment.Left);
		DrawTextInColumn(gr, "Breakfast", m_ptb.fontText, m_ptb.brushText, icolMealBreakfast, y, 3, HorizontalAlignment.Center);
		DrawTextInColumn(gr, "Lunch", m_ptb.fontText, m_ptb.brushText, icolMealLunch, y, 3, HorizontalAlignment.Center);
		DrawTextInColumn(gr, "Dinner", m_ptb.fontText, m_ptb.brushText, icolMealDinner, y, 3, HorizontalAlignment.Center);
		DrawTextInColumn(gr, "Bed", m_ptb.fontText, m_ptb.brushText, icolBed, y, 1, HorizontalAlignment.Center);
		DrawTextInColumn(gr, "Comments", m_ptb.fontText, m_ptb.brushText, icolComments, y, 1, HorizontalAlignment.Left);
	}

	/* P A I N T  L I N E */
	/*----------------------------------------------------------------------------
		%%Function: PaintLine
		%%Qualified: bg.Reporter.PaintLine
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	void PaintLine(Graphics gr, RLD rld, int iLine)
	{
		int iMeal;

		DrawTextInColumn(gr, rld.sDay, m_ptb.fontText, m_ptb.brushText, 0, YFromLine(iLine), 1, HorizontalAlignment.Center);

		for (iMeal = 0; iMeal <= iMealDinner; iMeal++)
			{
			if (rld.mpimd[iMeal].nBefore > 0)
				DrawTextInColumn(gr, rld.mpimd[iMeal].nBefore.ToString(), m_ptb.fontText, m_ptb.brushText, 1 + iMeal * 3, YFromLine(iLine), 1, HorizontalAlignment.Right);
			if (rld.mpimd[iMeal].nCarbs > 0)
				DrawTextInColumn(gr, rld.mpimd[iMeal].nCarbs.ToString(), m_ptb.fontText, m_ptb.brushText, 2 + iMeal * 3, YFromLine(iLine), 1, HorizontalAlignment.Center);
			if (rld.mpimd[iMeal].nAfter > 0)
				DrawTextInColumn(gr, rld.mpimd[iMeal].nAfter.ToString(), m_ptb.fontText, m_ptb.brushText, 3 + iMeal * 3, YFromLine(iLine), 1, HorizontalAlignment.Right);
			}

		DrawTextInColumn(gr, rld.nBedReading.ToString(), m_ptb.fontText, m_ptb.brushText, icolBed, YFromLine(iLine), 1, HorizontalAlignment.Right);
		DrawTextInColumn(gr, /*rld.sDateHeader + "] " + */rld.sComment, m_ptb.fontText, m_ptb.brushText, icolComments, YFromLine(iLine), 1, HorizontalAlignment.Left);
	}

	int m_nLines;
	int m_iLineFirst;
	int m_nLinesPerPage;
	/* S E T  D A T A  P O I N T S */
	/*----------------------------------------------------------------------------
		%%Function: SetDataPoints
		%%Qualified: bg.Reporter.SetDataPoints
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void SetDataPoints(object oData, VScrollBar sbv, HScrollBar sbh)
	{
		m_slbge = (SortedList)oData;

		// figure out how many lines we're going to have...
		// get the first entry and the last
		DateTime dttmFirst = ((BGE)m_slbge.GetByIndex(0)).Date;
		DateTime dttmLast = ((BGE)m_slbge.GetByIndex(m_slbge.Count - 1)).Date;

		dttmLast = dttmLast.AddMinutes(-241.0);	// subtract just over 4 hours to account for the fact that our day turns at 4am...  
		TimeSpan ts = dttmLast.Subtract(dttmFirst);
		// the number of days is the number of lines.
		m_nLines = ts.Days + 1;
		m_iLineFirst = 0;
		if (sbv != null)
			{
			if (m_nLines <= m_nLinesPerPage)
				sbv.Visible = false;
			else
				{
				sbv.Visible = true;
				sbv.Minimum = 0;
				sbv.Maximum = m_nLines + m_nLinesPerPage;
				}
			}
	}

	/* S E T  P R O P S */
	/*----------------------------------------------------------------------------
		%%Function: SetProps
		%%Qualified: bg.Reporter.SetProps
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void SetProps(GrapherParams cgp)
	{
		m_cgp = cgp;
	}

	float m_dyLine = 0.0F;

	/* Y  F R O M  L I N E */
	/*----------------------------------------------------------------------------
		%%Function: YFromLine
		%%Qualified: bg.Reporter.YFromLine
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public float YFromLine(int nLine)
	{
		return (float)m_dyLine * nLine + m_rcfDrawing.Top;
	}

	/* D R A W  T E X T  I N  C O L U M N */
	/*----------------------------------------------------------------------------
		%%Function: DrawTextInColumn
		%%Qualified: bg.Reporter.DrawTextInColumn
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void DrawTextInColumn(Graphics gr, string s, Font font, SolidBrush br, int iCol, float y, int cColSpan, HorizontalAlignment jc)
	{
		int iColMax = iCol + cColSpan - 1;
		COLD cold = m_mpicold[iCol];
		float dxpRight = m_mpicold[iColMax].dxpRight;
		float dxpWidth = 0.0F;

		if (cColSpan == 1)
			dxpWidth = cold.dxpCol;
		else
			dxpWidth = dxpRight - cold.xpLeft;

		RectangleF rectfClip = new RectangleF(cold.xpLeft, y, dxpWidth, y + 20);
		gr.SetClip(rectfClip);

		switch (jc)
			{
			case HorizontalAlignment.Left:
				gr.DrawString(s, font, br, cold.xpLeft, y);
				break;
			case HorizontalAlignment.Center:
				gr.DrawString(s, font, br, cold.xpLeft + (dxpWidth - gr.MeasureString(s, font).Width) / 2, y);
				break;
			case HorizontalAlignment.Right:
				gr.DrawString(s, font, br, cold.xpLeft + (dxpWidth - gr.MeasureString(s, font).Width), y);
				break;
			}
		gr.ResetClip();
	}

	ArrayList m_plrld;

	/* C A L C  R E P O R T */
	/*----------------------------------------------------------------------------
		%%Function: CalcReport
		%%Qualified: bg.Reporter.CalcReport
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void Calc()
	{
		m_plrld = new ArrayList();

		// ok, let's walk through and figure out what gets reported and what doesn't
		//
		// first, draw the date we are working with.
		BGE bge = (BGE)m_slbge.GetByIndex(0);
		DateTime dttmFirst = new DateTime(bge.Date.Year, bge.Date.Month, bge.Date.Day);

		int ibge, ibgeMax;

		ibge = 0;
		ibgeMax = m_slbge.Values.Count;
		RLD rld = new RLD();

		while (ibge < ibgeMax)
			{
			float []dMeals = { 8.0F, 12.0F, 18.0F };	// our defaults for meals
			float dHours = 0.0F;
			string []rgsDay = { "S", "M", "T", "W", "T", "F", "S" };
			bool []rgfMeals = { false, false, false };

			while (ibge < ibgeMax)
				{
				bge = (BGE)m_slbge.GetByIndex(ibge);
				if (!bge.InterpReading)
					break;
				ibge++;
				}

			DateTime dttm = new DateTime(bge.Date.Year, bge.Date.Month, bge.Date.Day);
			DateTime dttmNextDay = dttm.AddHours(29.5);// next day is 4:30am the next day
			int ibgeT = ibge;
			rld.dttm = dttm;
			rld.sDay = rgsDay[(int)dttm.DayOfWeek];
			if (dttm.DayOfWeek == DayOfWeek.Sunday)
				{
				rld.sDay = dttm.ToString("M-dd");
				rld.sDateHeader = dttm.ToString("d");
				}

			// let's see what meals we have manually accounted for...
			while (bge.Date < dttmNextDay)
				{
				dHours = bge.Date.Hour + bge.Date.Minute / 60.0F;

				if (bge.InterpReading)
					{
					}
				else if (bge.Type == BGE.ReadingType.Breakfast)
					{
					dMeals[0] = dHours;
					rgfMeals[0] = true;
					}
				else if (bge.Type == BGE.ReadingType.Lunch)
					{
					dMeals[1] = dHours;
					rgfMeals[1] = true;
					}
				else if (bge.Type == BGE.ReadingType.Dinner)
					{
					dMeals[2] = dHours;
					rgfMeals[2] = true;
					}

				if (ibgeT + 1 >= ibgeMax)
					break;

				bge = (BGE)m_slbge.GetByIndex(++ibgeT);
				}

			// ok, we have figured out the meal boundaries.  now lets match the readings...
			// any reading within 1 hour of the meal time is considered a "before", and the first reading
			// between 2 hours and 3 hours after meal time is considered "after"
			bge = (BGE)m_slbge.GetByIndex(ibgeT = ibge);
			int iMealNext = 0;
			bool fBeforeMatched = false;
			int readingBefore = 0;
			int readingBed = 0;
			int readingFirst = 0; // the first reading of the day will serve as the "pre-breakfast" unless we get a better reading.
			float dBedMin = dMeals[iMealDinner] + 2.0F;
			bool fMealBumped = false;

			while (bge.Date < dttmNextDay)	
				{
				while (bge.Date < dttmNextDay)
					{
					bge = (BGE)m_slbge.GetByIndex(ibgeT);
					if (!bge.InterpReading)
						break;
					if (ibgeT + 1 >= ibgeMax)
						break;
					ibgeT++;
					}
				dHours = bge.Date.Hour + bge.Date.Minute / 60.0F;

				// don't duplicate comments...
				if (!fMealBumped && bge.FullComment.Length > 0)
					{
					if (rld.sComment.Length > 0)
						rld.sComment += "; ";

					rld.sComment += bge.FullComment;
					}

				fMealBumped = false;
				if (bge.Date.Day == dttmNextDay.Day)
					dHours += 24.0F;

				if (readingFirst == 0 && dHours <= dMeals[iMealBreakfast])
					readingFirst = bge.Reading;

				if (iMealNext <= iMealDinner)
					{
					// are we looking for a 'before' reading?

					if ((iMealNext == iMealBreakfast && bge.Type == BGE.ReadingType.Breakfast)
						|| (iMealNext == iMealLunch && bge.Type == BGE.ReadingType.Lunch)
						|| (iMealNext == iMealDinner && bge.Type == BGE.ReadingType.Dinner))
						{
						rld.mpimd[iMealNext].nCarbs = bge.Carbs;
						}

					if (!fBeforeMatched
						&& bge.Reading != 0
						&& dHours >= dMeals[iMealNext] - m_dHoursBefore && dHours <= dMeals[iMealNext])
						{
						// got a "before" match
						readingBefore = bge.Reading;
						}

					// did we find a "before" reading for the current meal and are looking
					// for that meal (or a reading post-meal) to confirm that this is the last
					// "before" reading?

					if (readingBefore != 0 && !fBeforeMatched && dHours > dMeals[iMealNext])
						{
						rld.mpimd[iMealNext].nBefore = readingBefore;
						fBeforeMatched = true;
						}

					// does this reading qualify as an 'after' reading?
					if (dHours >= dMeals[iMealNext] + m_dHoursAfter && dHours <= dMeals[iMealNext] + 4.0)
						{
						// got an "after" match
						rld.mpimd[iMealNext].nAfter = bge.Reading;
						iMealNext++;
						fBeforeMatched = false;
						if (readingBefore != 0)
							readingFirst = 0;
						readingBefore = 0;
						fMealBumped = true;
						}

					if (!fMealBumped)
						{
						// check to see if we are ready to bump to the next meal
						if (dHours > dMeals[iMealNext] + 4.0)
							{
							iMealNext++;
							fBeforeMatched = false;
							if (readingBefore != 0)
								readingFirst = 0;
							readingBefore = 0;
							fMealBumped = true;
							}
						}

					if (fMealBumped && iMealNext == iMealLunch)
						{
						if (readingFirst != 0)
							{
							// we still have a "first reading" which means that the breakfast
							// meal never posted a before reading.  use that first reading now.
							rld.mpimd[iMealBreakfast].nBefore = readingFirst;
							readingFirst = 0;
							}
						}

					if (!fMealBumped && ibgeT + 1 >= ibgeMax)
						{
						break;
						}

					}

				if (dHours >= dBedMin)
					readingBed = bge.Reading;

				// if we bumped the meal, then go through again considering this reading
				// for the next meal.
				if (!fMealBumped)
					{
					if (ibgeT + 1 >= ibgeMax)
						break;
					bge = (BGE)m_slbge.GetByIndex(++ibgeT);
					}
				}
			// need to flush everything here
			if (!fBeforeMatched && readingBefore != 0)
				{
				rld.mpimd[iMealNext].nBefore = readingBefore;
				}
			if (!fBeforeMatched && iMealNext == iMealBreakfast && readingFirst != 0)
				{
				rld.mpimd[iMealNext].nBefore = readingFirst;
				}

			rld.nBedReading = readingBed;
			m_plrld.Add(rld);
			rld = new RLD();

			// consume the rest of the day
			ibge = ibgeT;
			bge = (BGE)m_slbge.GetByIndex(ibge);
			while (bge.Date < dttmNextDay)
				{
				ibge++;
				if (ibge >= ibgeMax)
					break;

				bge = (BGE)m_slbge.GetByIndex(ibge);
				}
			}
		// at this point, we have all the data we'll ever want...

	}


	public int GetDaysPerPage()
	{
		return m_nLinesPerPage;
	}

	public void SetDaysPerPage(int nDaysPerPage)
	{
		throw new Exception("Cannot set days per page in a report!");
	}

	/* P A I N T */
	/*----------------------------------------------------------------------------
		%%Function: Paint
		%%Qualified: bg.Reporter.Paint
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void Paint(Graphics gr)
	{

		PaintHeader(gr);
		PaintGridlines(gr);

		int iLine, iLineMax;
		int iLinePainting = 1;

		for (iLine = m_iLineFirst, iLineMax = Math.Min(m_iLineFirst + m_nLinesPerPage, m_nLines); iLine < iLineMax; iLine++)
			{
			if (iLine >= m_plrld.Count)
				break;
			PaintLine(gr, (RLD)m_plrld[iLine], iLinePainting++);
			}
	}

	public bool FGetLastDateTimeOnPage(out DateTime dttm)
	{
		dttm = GetFirstDateTime();
		if (m_iLineFirst + m_nLinesPerPage > m_nLines)
			{
			return false;
			}

		dttm = dttm.AddDays(m_nLinesPerPage);
		return true;
	}

	/* F  H I T  T E S T */
	/*----------------------------------------------------------------------------
		%%Function: FHitTest
		%%Qualified: bg.Reporter.FHitTest
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public bool FHitTest(Point pt, out object oHit, out RectangleF rectfHit)
	{
		oHit = null;
		rectfHit = new RectangleF(0F,0F,0F,0F);
		return false;
	}

}
}
