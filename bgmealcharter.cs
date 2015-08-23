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
public class MealCharter : GraphicBox
{
	class MPG
	{
		MP m_mp;
		ArrayList m_plsbge;

		public MPG(MP mp)
		{
			m_mp = mp;
		}

		public ArrayList Plsbge	{ get { return m_plsbge; } set { m_plsbge = value; } }
		public MP Mp { get { return m_mp; } set { m_mp = value; } }
		
	}

	ArrayList m_plmp;

	int m_impFirst;
	VScrollBar m_sbv;
	HScrollBar m_sbh;
	GrapherParams m_gp;
	RectangleF m_rcf;
	ArrayList m_plmpg;

	float m_dxOffsetForSbge;
	float m_dyOffsetForSbge;

	public MealCharter(RectangleF rcf, Graphics gr)
	{
		m_rcf = rcf;
	}

	public void Paint(Graphics gr)
	{
//		int yTopBase = (int)sbge.RectF.Top + 17;

		foreach (MPG mpg in m_plmpg)
			{
			Font font = new Font("Tahoma", 10);
			SolidBrush brushBlue = new SolidBrush(Color.Blue);
			Pen pen = new Pen(brushBlue, 1);
	
			SBGE sbge = (SBGE)mpg.Plsbge[0];
	
			const int dxSep = 30;
			const int dxLeft = 20;

			int xLeft = (int)m_rcf.Left + dxLeft;
			int yTop = (int)sbge.RectF.Top;
	
			MP mp = mpg.Mp;
			gr.DrawString(mp.SGetDescription(), font, brushBlue, sbge.RectF.Left - 150, yTop - 15);
	
	
			gr.DrawRectangle(pen, xLeft - 4, yTop, dxSep * 4 + 4, 43);
			gr.DrawLine(pen, xLeft + dxSep - 1, yTop, xLeft + dxSep - 1, yTop + 43);
			gr.DrawLine(pen, xLeft + dxSep * 2 - 1, yTop, xLeft + dxSep * 2 - 1, yTop + 43);
			gr.DrawLine(pen, xLeft + dxSep * 3 - 1, yTop, xLeft + dxSep * 3 - 1, yTop + 43);
	
			gr.DrawLine(pen, xLeft - 4, yTop + 20, xLeft + dxSep * 4, yTop + 20);
			gr.DrawString("M+0", font, brushBlue, xLeft, yTop + 3);
			gr.DrawString("M+1", font, brushBlue, xLeft + dxSep * 1, yTop + 3);
			gr.DrawString("M+2", font, brushBlue, xLeft + dxSep * 2, yTop + 3);
			gr.DrawString("M+4", font, brushBlue, xLeft + dxSep * 3, yTop + 3);
	
			for (int i = 0; i < 4; i++)
				{
				gr.DrawString(mp.NGetAvgForMpt((MP.MPT)i).ToString(), font, brushBlue, xLeft + i * dxSep, yTop + 23);
				}
	
			yTop += 50;
			xLeft = (int)m_rcf.Left + dxLeft + 30;
	
			gr.DrawRectangle(pen, xLeft - 4, yTop, dxSep * 2 + 4, 43);
			gr.DrawLine(pen, xLeft + dxSep - 1, yTop, xLeft + dxSep - 1, yTop + 43);
	
			gr.DrawLine(pen, xLeft - 4, yTop + 20, xLeft + dxSep * 2, yTop + 20);
			gr.DrawString("Post", font, brushBlue, xLeft, yTop + 3);
			gr.DrawString("Morn", font, brushBlue, xLeft + dxSep * 1, yTop + 3);
	
			for (int i = 0; i < 2; i++)
				{
				gr.DrawString(mp.NGetAvgForMpt((MP.MPT)i + 4).ToString(), font, brushBlue, xLeft + i * dxSep, yTop + 23);
				}
	
			sbge.SetNoCurves();
			sbge.PaintGraphGridlines(gr);
			foreach (SBGE sbgeT in mpg.Plsbge)
				{
				sbgeT.PaintGraph(gr);
				}
			}
	}

	public void SetFirstFromScroll(int i)
	{
		m_impFirst = i;
	}

	public void Calc()
	{
		RectangleF rcf = new RectangleF(m_rcf.Left + 180.0f, m_rcf.Top + 25.0f, m_rcf.Width - 150.0f, 100.0f);

		// we are going to have a number of graphs on the page; 
		// calc them all.

		// let's assume we're only graphing one for now
		m_plmpg = new ArrayList();

		foreach (MP mp in m_plmp)
			{
			// graph the meals

			MPG mpg = new MPG(mp);
			mpg.Plsbge = new ArrayList();

			int i = 0, iMac = mp.NGetSampleSize();

			for (i = 0; i < iMac; i++)
				{
				SBGE sbge = new SBGE(m_gp, m_dxOffsetForSbge, m_dyOffsetForSbge + 15.0f, false);

				sbge.Tag = mp;
				sbge.SetDataSet(mp.SlbgeForSample(i), null);
				sbge.SetMealLegend(mp.DttmMealForSample(i));
				sbge.SetLineWidth(0.2f);
				sbge.CalcGraph(rcf);
				mpg.Plsbge.Add(sbge);
				}
			m_plmpg.Add(mpg);
			rcf.Y += 140.0f;
			

//			rcf = new RectangleF(m_rcf.Left + 120.0f, m_rcf.Top + 110.0f, m_rcf.Width - 150.0f, 100.0f);

//			i = 0;
//			iMac = mp.NGetSampleSize();

//			for (i = 0; i < iMac; i++)
//				{
//				SBGE sbge = new SBGE(m_gp, m_dxOffsetForSbge, m_dyOffsetForSbge, false);
//
//				sbge.SetDataSet(mp.SlbgeForSample(i), null);
//				sbge.SetMealLegend(mp.DttmMealForSample(i));
//				sbge.SetLineWidth(0.2f);
//				sbge.CalcGraph(rcf);
//				mpg.Plsbge.Add(sbge);
//				}
//			m_plmpg.Add(mpg);
			}
	}

	public bool FHitTest(Point pt, out object oHit, out RectangleF rectfHit)
	{
		oHit = null;
		rectfHit = new RectangleF();
		return false;
	}

	public void SetDataPoints(object oData, VScrollBar sbv, HScrollBar sbh)
	{
		m_plmp = (ArrayList)oData;
		m_sbv = sbv;
		m_sbh = sbh;
	}

	public void SetProps(GrapherParams gp)
	{
		m_gp = gp;
	}

	public int GetFirstForScroll()
	{
		return m_impFirst;
	}

	public void SetColor(bool fColor)
	{
	}

	public int GetDaysPerPage()
	{
		return 0;
	}

	public void SetDaysPerPage(int nDaysPerPage)
	{
	}

	public DateTime GetFirstDateTime()
	{
		return new DateTime(2005, 01, 01);
	}

	public void SetFirstDateTime(DateTime dttm)
	{
	}

	public bool FGetLastDateTimeOnPage(out DateTime dttm)
	{
		dttm = new DateTime(2005, 01, 01);
		return false;
	}
}

}
