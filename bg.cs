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
using System.Text.RegularExpressions;

namespace bg
{

//	___  ____ ____
//	|__] | __ |___
//	|__] |__] |___

public class BGE // BG Entry
{
	public enum ReadingType
	{
		Breakfast,
		Lunch,
		Dinner,
		Snack,
		SpotTest,
		Control,
		New
	};

	public enum CompareType
	{
		Date,
		Type,
		Reading,
		Carbs,
		Comment
	};

	DateTime m_dttm;
	int m_nBg;
	ReadingType m_type;
	int m_nCarbs;
	string m_sComment;
	string m_sMeal;
	int m_nMinutesSinceLastCarb;
	int m_nCarbsInLast4Hours;
	bool m_fInterp;

	int m_nWgtAvg;

	/* R E A D I N G  T Y P E  F R O M  S T R I N G */
	/*----------------------------------------------------------------------------
		%%Function: ReadingTypeFromString
		%%Qualified: bg.BGE.ReadingTypeFromString
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	public static ReadingType ReadingTypeFromString(string s)
	{
		if (String.Compare(s, "breakfast", true) == 0)
			return ReadingType.Breakfast;
		else if (String.Compare(s, "lunch", true) == 0)
			return ReadingType.Lunch;
		else if (String.Compare(s, "dinner", true) == 0)
			return ReadingType.Dinner;
		else if (String.Compare(s, "snack", true) == 0)
			return ReadingType.Snack;
		else if (String.Compare(s, "new", true) == 0)
			return ReadingType.New;
		else if (String.Compare(s, "control", true) == 0)
			return ReadingType.Control;
		else
			return ReadingType.SpotTest;
	}

	/* S T R I N G  F R O M  R E A D I N G  T Y P E */
	/*----------------------------------------------------------------------------
		%%Function: StringFromReadingType
		%%Qualified: bg.BGE.StringFromReadingType
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	public static string StringFromReadingType(ReadingType type)
	{
		switch (type)
			{
			case ReadingType.Breakfast:
				return "Breakfast";
			case ReadingType.Lunch:
				return "Lunch";
			case ReadingType.Snack:
				return "Snack";
			case ReadingType.Dinner:
				return "Dinner";
			case ReadingType.New:
				return "*NEW*";
			case ReadingType.Control:
				return "Control";
			default:
				return "SpotTest";
			}
	}


	override public string ToString()
	{
		return String.Format("{0}: {1}={2} [{3}]", m_dttm.ToString("s"), StringFromReadingType(m_type), m_nBg, this.FullComment);
	}

	/* B  G  E */
	/*----------------------------------------------------------------------------
		%%Function: BGE
		%%Qualified: bg.BGE.BGE
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	public BGE(string sDate, string sTime, ReadingType type, int bg, int nCarbs, string sComment, string sMeal)
	{
		m_dttm = DateTime.Parse(sDate + " " + sTime);
		m_nBg = bg;
		m_type = type;
		m_nCarbs = nCarbs;
		m_sComment = sComment;
		m_sMeal = sMeal;
		m_fInterp = false;
	}

	public BGE(BGE bge)
	{
		m_dttm = bge.m_dttm;
		m_nBg = bge.m_nBg;
		m_type = bge.m_type;
		m_nCarbs = bge.m_nCarbs;
		m_sComment = bge.m_sComment;
		m_sMeal = bge.m_sMeal;
		m_fInterp = bge.m_fInterp;
		m_nMinutesSinceLastCarb = bge.m_nMinutesSinceLastCarb;
		m_nCarbsInLast4Hours = bge.m_nCarbsInLast4Hours;
		m_nWgtAvg = bge.m_nWgtAvg;
	}

	public BGE Clone()
	{
		return new BGE(this);
	}

	public int 			Carbs 				{ get { return m_nCarbs; }  				set { m_nCarbs = value; } }
	public int 			MinutesSinceCarbs 	{ get { return m_nMinutesSinceLastCarb; }  	set { m_nMinutesSinceLastCarb = value; } }
	public int 			CarbsIn4 			{ get { return m_nCarbsInLast4Hours; }  	set { m_nCarbsInLast4Hours = value; } }
	public string 		Comment				{ get { return m_sComment; }                set { m_sComment = value; } }
	public string 		Meal				{ get { return m_sMeal; }                   set { m_sMeal = value; } }
	public ReadingType 	Type				{ get { return m_type; }                    set { m_type = value; } }
	public DateTime 	Date				{ get { return m_dttm; } 					set { m_dttm = value; } }
	public int 			Reading				{ get { return m_nBg; } }
	public bool			InterpReading		{ get { return m_fInterp; }					set { m_fInterp = value; } }
	public int 			WgtAvg				{ get { return m_nWgtAvg; } 				set { m_nWgtAvg = value; } }

	public string		Key					{ get { return m_dttm.ToString("s"); } }

	public string FullComment
	{
		get
		{
			if (m_sMeal.Length > 0)
				{
				if (m_sComment.Length > 0)
					return m_sMeal + "(" + m_sComment + ")";
				else
					return m_sMeal;
				}
			if (m_sComment.Length > 0)
				return "("+m_sComment+")";
			else
				return "";
		}
	}

	/* C O M P A R E  T O */
	/*----------------------------------------------------------------------------
		%%Function: CompareTo
		%%Qualified: bg.BGE.CompareTo
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	public int CompareTo(BGE bge, CompareType type)
	{	
		return Compare(this, bge, type);
	}

	/* C O M P A R E */
	/*----------------------------------------------------------------------------
		%%Function: Compare
		%%Qualified: bg.BGE.Compare
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	static public int Compare(BGE bge1, BGE bge2, CompareType type)
	{
		switch (type)
			{
			case CompareType.Type:
				return (int)bge1.Type - (int)bge2.Type;
			case CompareType.Reading:
				return bge1.Reading - bge2.Reading;
			case CompareType.Date:
				return DateTime.Compare(bge1.Date, bge2.Date);
			case CompareType.Carbs:
				return bge1.Carbs - bge2.Carbs;
			case CompareType.Comment:
				return String.Compare(bge1.FullComment, bge2.FullComment);
			default:
				return 0; 
			}
	}

	public void SetTo(BGE bge)
	{
		m_type = bge.Type;
		m_dttm = bge.Date;
		m_nBg = bge.Reading;
		m_nCarbs = bge.Carbs;
		m_sComment = bge.Comment;
		m_sMeal = bge.Meal;
	}

	
}

public struct PTFI
{
	public PointF ptf;
	public BGE bge;
};

public struct GrapherParams
{
	public int nHalfDays;
	public int nIntervals;
	public bool fShowMeals;
	public double dBgLow;
	public double dBgHigh;
	public bool fLandscape;
	public bool fGraphAvg;
}

//  ______  ______ _______  _____  _     _ _____ _______   ______   _____  _     _
// |  ____ |_____/ |_____| |_____] |_____|   |   |         |_____] |     |  \___/
// |_____| |    \_ |     | |       |     | __|__ |_____    |_____] |_____| _/   \_

public interface GraphicBox
{
	void Paint(Graphics gr);
	void SetFirstFromScroll(int i);
	void Calc();
	bool FHitTest(Point pt, out object oHit, out RectangleF rectfHit);
	void SetDataPoints(object oData, VScrollBar sbv, HScrollBar sbh);
	void SetProps(GrapherParams gp);
	int GetFirstForScroll();
	void SetColor(bool fColor);
	int GetDaysPerPage();
	void SetDaysPerPage(int nDaysPerPage);
	DateTime GetFirstDateTime();
	void SetFirstDateTime(DateTime dttm);
	bool FGetLastDateTimeOnPage(out DateTime dttm);
}


class MP // Meal Profile
{
	public enum MPT : int
	{
		Hour0 = 0,
		Hour1 = 1,
		Hour2 = 2,
		Hour4 = 3,
		PostMeal = 4,	// anything > 1.5H and < 3H
		Morning = 5,	// the next morning, if this was a dinner
		Last = Morning
	}

	int []m_mpmptcMeals;
	int []m_mpmptnMealSum;

	string m_sDescription;

	ArrayList m_plslbgeMeals;
	ArrayList m_pldttmMeals;

	public MP()
	{
		m_mpmptcMeals = new int[(int)MPT.Morning + 1];
		m_mpmptnMealSum = new int[(int)MPT.Morning + 1];

		m_plslbgeMeals = new ArrayList();
	}

	public string SGetDescription()
	{
		return m_sDescription;
	}

	public int NGetSampleSize()
	{
		// every entry in mpmptnMeals should be the same except for
		// (possibly) mptMorning -- that is only valid if the meal
		// was a dinner, so a particular meal could have occured
		// at dinner and also at non-dinner...
		return m_mpmptcMeals[0];
	}

	public int NGetAvgForMpt(MPT mpt)
	{
		if (m_mpmptcMeals[(int)mpt] == 0)
			return 0;

		return m_mpmptnMealSum[(int)mpt] / m_mpmptcMeals[(int)mpt];
	}


	public SortedList SlbgeForSample(int n)
	{
		return (SortedList)m_plslbgeMeals[n];
	}

	public DateTime DttmMealForSample(int n)
	{
		return (DateTime)m_pldttmMeals[n];
	}


	/* N O R M A L I Z E  M E A L S */
	/*----------------------------------------------------------------------------
		%%Function: NormalizeMeals
		%%Qualified: bg.MP.NormalizeMeals
		%%Contact: rlittle

		Make all the meals start at the same time
	----------------------------------------------------------------------------*/
	public void NormalizeMeals()
	{
		TimeSpan tsMeal = new TimeSpan(0,0,0,0);
		ArrayList plbgeMeal = new ArrayList();

		m_pldttmMeals = new ArrayList();

		// figure out the largest delta between the first meal and the 
		// first reading (pre meal)
		foreach (SortedList slbge in m_plslbgeMeals)
			{
			DateTime dttmFirst = ((BGE)slbge.GetByIndex(0)).Date;
			DateTime dttmMeal = dttmFirst;
			bool fFound = false;

			foreach (BGE bge in slbge.Values)
				{
				if (bge.Carbs > 0)
					{
					dttmMeal = bge.Date;
					plbgeMeal.Add(bge);
					fFound = true;
					break;
					}
				}

			if (!fFound)
				throw(new Exception("NormalizeMeals could not find a meal!"));

			if (tsMeal < dttmMeal - dttmFirst)
				tsMeal = dttmMeal - dttmFirst;
			}  
		// ok, make all meals occur at 0:00 + ts.

		int iMeal = 0;
		foreach (SortedList slbge in m_plslbgeMeals)
			{
			BGE bgeMeal = (BGE)plbgeMeal[iMeal];
			
			TimeSpan tsAdjust = bgeMeal.Date.Subtract(tsMeal) - new DateTime(2003,1,1);

			DateTime dttmMeal = new DateTime(2003,1,1).Add(tsMeal);
			m_pldttmMeals.Add(dttmMeal);

			foreach(BGE bge in slbge.Values)
				bge.Date = bge.Date.Subtract(tsAdjust);
			iMeal++;
			}
#if VERBOSE
#if DEBUG
		Debug.WriteLine("Normalizing");
		iMeal = 0;

		foreach (SortedList slbge in m_plslbgeMeals)
			{
			BGE bgeMeal = (BGE)plbgeMeal[iMeal];
			Debug.Write(String.Format("BGE({0}", iMeal));

			foreach(BGE bge in slbge.Values)
				{
				Debug.Write(String.Format(" -- {0}({1})", bge.Date.ToString("s"), bge.Carbs));
				if (bge.Carbs > 0)
					break;
				}
			Debug.WriteLine(".");
			}

#endif // DEBUG
#endif // VERBOSE
	}

	int NGetForDttm(SortedList slbge, BGE bgeRef, DateTime dttmNext, double dMinToleranceBack, double dMinToleranceForward)
	{
		int iFirst = 0;
		int iKey = 0;
		BGE bge = null;

		iKey = iFirst = slbge.IndexOfKey(bgeRef.Key);

		for ( ; iFirst >= 0; iFirst--)
			{
			bge = (BGE)slbge.GetByIndex(iFirst);

			if (bge.Reading <= 0
				|| bge.Type == BGE.ReadingType.Control)
				{
				continue;
				}

			if (bge.Date >= dttmNext.AddMinutes(-dMinToleranceBack)
				&& bge.Date <= dttmNext.AddMinutes(dMinToleranceForward))
				{
				return bge.Reading;
				}

			if (bge.Date < dttmNext.AddMinutes(-dMinToleranceBack))
				break;
			}

		// no luck finding it going back; now look going forward
		for (iFirst = iKey; iFirst < slbge.Count; iFirst++)
			{
			bge = (BGE)slbge.GetByIndex(iFirst);

			if (bge.Reading <= 0
				|| bge.Type == BGE.ReadingType.Control)
				{
				continue;
				}

			if (bge.Date >= dttmNext.AddMinutes(-dMinToleranceBack)
				&& bge.Date <= dttmNext.AddMinutes(dMinToleranceForward))
				{
				return bge.Reading;
				}

			if (bge.Date> dttmNext.AddMinutes(dMinToleranceForward))
				break;
			}

		return 0;
	}

	/* A D D  M E A L */
	/*----------------------------------------------------------------------------
		%%Function: AddMeal
		%%Qualified: bg._bg:MP.AddMeal
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	public void AddMeal(SortedList slbge, SortedList slbgeRef, BGE bgeMeal)
	{
		if (m_sDescription == null)
			m_sDescription = bgeMeal.Meal;

		m_plslbgeMeals.Add(slbge);

		int n;

		n = NGetForDttm(slbgeRef, bgeMeal, bgeMeal.Date, 90.0, 0.0);
		if (n != 0)
			{
			m_mpmptnMealSum[(int)MPT.Hour0] += n;
			m_mpmptcMeals[(int)MPT.Hour0]++;
			}

		n = NGetForDttm(slbgeRef, bgeMeal, bgeMeal.Date.AddMinutes(60), 15.0, 15.0);
		if (n != 0)
			{
			m_mpmptnMealSum[(int)MPT.Hour1] += n;
			m_mpmptcMeals[(int)MPT.Hour1]++;
			}

		n = NGetForDttm(slbgeRef, bgeMeal, bgeMeal.Date.AddMinutes(120), 30.0, 30.0);
		if (n != 0)
			{
			m_mpmptnMealSum[(int)MPT.Hour2] += n;
			m_mpmptcMeals[(int)MPT.Hour2]++;
			}

		n = NGetForDttm(slbgeRef, bgeMeal, bgeMeal.Date.AddMinutes(240), 50.0, 50.0);
		if (n != 0)
			{
			m_mpmptnMealSum[(int)MPT.Hour4] += n;
			m_mpmptcMeals[(int)MPT.Hour4]++;
			}

		n = NGetForDttm(slbgeRef, bgeMeal, bgeMeal.Date.AddMinutes(120), 30.0, 45.0);
		if (n != 0)
			{
			m_mpmptnMealSum[(int)MPT.PostMeal] += n;
			m_mpmptcMeals[(int)MPT.PostMeal]++;
			}

		if (bgeMeal.Type == BGE.ReadingType.Dinner)
			{
			// look for the following mornings first reading
			n = NGetForDttm(slbgeRef, bgeMeal, new DateTime(bgeMeal.Date.Year, bgeMeal.Date.Month, bgeMeal.Date.Day).AddDays(1.0).AddHours(5.0), 0.0, 240.0);
			if (n != 0)
				{
				m_mpmptnMealSum[(int)MPT.Morning] += n;
				m_mpmptcMeals[(int)MPT.Morning]++;
				}
			}
	}
};

public class ListViewItemComparer : IComparer
{
	private int m_col;
	private bool m_fReverse;

	/* L I S T  V I E W  I T E M  C O M P A R E R */
	/*----------------------------------------------------------------------------
		%%Function: ListViewItemComparer
		%%Qualified: bg.ListViewItemComparer.ListViewItemComparer
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	public ListViewItemComparer()
	{
		m_col = 0;
		m_fReverse = true;
	}

	/* L I S T  V I E W  I T E M  C O M P A R E R */
	/*----------------------------------------------------------------------------
		%%Function: ListViewItemComparer
		%%Qualified: bg.ListViewItemComparer.ListViewItemComparer
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	public ListViewItemComparer(int col)
	{
		m_col = col;
		m_fReverse = true;
	}

	/* S E T  C O L U M N */
	/*----------------------------------------------------------------------------
		%%Function: SetColumn
		%%Qualified: bg.ListViewItemComparer.SetColumn
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	public void SetColumn(int col)
	{
		if (m_col == col)
			m_fReverse = !m_fReverse;
		else
			m_fReverse = true;
		m_col = col;
	}

	/* C O M P A R E */
	/*----------------------------------------------------------------------------
		%%Function: Compare
		%%Qualified: bg.ListViewItemComparer.Compare
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	public int Compare(object x, object y)
	{
		ListViewItem lvi1 = (ListViewItem)x;
		ListViewItem lvi2 = (ListViewItem)y;
		BGE bge1 = (BGE)lvi1.Tag;
		BGE bge2 = (BGE)lvi2.Tag;
		
		if (bge1 == null)
			return -1;

		if (bge2 == null)
			return 1;

		int n = 0;

		switch (m_col)
			{
			case 0:
				n = BGE.Compare(bge1, bge2, BGE.CompareType.Date);
				break;
			case 1:
				n = BGE.Compare(bge1, bge2, BGE.CompareType.Type);
				break;
			case 2:
				n = BGE.Compare(bge1, bge2, BGE.CompareType.Reading);
				break;
			case 3:
				n = BGE.Compare(bge1, bge2, BGE.CompareType.Carbs);
				break;
			case 4:
				n = BGE.Compare(bge1, bge2, BGE.CompareType.Comment);
				break;
			}
		return m_fReverse ? -n : n;
	}

}

/// <summary>
/// Summary description for Form1.
/// </summary>
public class _bg : System.Windows.Forms.Form
{

	private System.Windows.Forms.TabControl m_tabc;
	private System.Windows.Forms.TabPage m_tabEntry;
	private System.Windows.Forms.TabPage m_tabAnalysis;
	private System.Windows.Forms.Label label1;
	private System.Windows.Forms.Label label2;
	private System.Windows.Forms.Label label3;
	private System.Windows.Forms.TextBox m_ebDate;
	private System.Windows.Forms.Label label4;
	private System.Windows.Forms.TextBox m_ebTime;
	private System.Windows.Forms.Label label5;
	private System.Windows.Forms.TextBox m_ebReading;
	private System.Windows.Forms.Label label6;
	private System.Windows.Forms.Label label7;
	private System.Windows.Forms.ListView m_lvHistory;
	private SortedList m_slbge;
	private System.Windows.Forms.Button m_pbAdd;
	private System.Windows.Forms.ComboBox m_cbxType;
	private XmlDocument m_dom;
	private XmlNamespaceManager m_nsmgr;
	private System.Windows.Forms.Label label8;
	private System.Windows.Forms.TextBox m_ebCarbs;
	private System.Windows.Forms.Label label9;
	private System.Windows.Forms.TextBox m_ebComment;
	private BGE m_bgeCurrent;
	private System.Windows.Forms.Button m_pbGraph;
	private System.Windows.Forms.CheckBox m_cbSpot;
	private System.Windows.Forms.CheckBox m_cbBreakfast;
	private System.Windows.Forms.CheckBox m_cbLunch;
	private System.Windows.Forms.CheckBox m_cbDinner;
	private System.Windows.Forms.CheckBox m_cbSnack;
	private System.Windows.Forms.Label label12;
	private System.Windows.Forms.Label label14;
	private System.Windows.Forms.TextBox m_ebFastLength;
	private System.Windows.Forms.Label label28;
	private System.Windows.Forms.Label label29;
	private System.Windows.Forms.CheckBox m_cbShowMeals;
	private System.Windows.Forms.TextBox m_ebIntervals;
	private System.Windows.Forms.TextBox m_ebHigh;
	private System.Windows.Forms.TextBox m_ebLow;
	private System.Windows.Forms.Label label19;
	private System.Windows.Forms.Label label18;
	private System.Windows.Forms.Label label17;
	private System.Windows.Forms.TextBox m_ebDays;
	private System.Windows.Forms.Label label16;
	private System.Windows.Forms.Label label15;
	private System.Windows.Forms.Label label11;
	private System.Windows.Forms.Label label10;
	private System.Windows.Forms.TextBox m_ebLast;
	private System.Windows.Forms.TextBox m_ebFirst;
	private System.Windows.Forms.Label label30;
	private System.Windows.Forms.Label label31;
	private System.Windows.Forms.ComboBox m_cbxUpper;
	private System.Windows.Forms.ComboBox m_cbxLower;
	private System.Windows.Forms.ComboBox m_cbxOrient;
	private System.Windows.Forms.ComboBox m_cbxDateRange;
	private System.Windows.Forms.ComboBox m_cbxFilterType;
	private System.Windows.Forms.Label label13;
	private System.Windows.Forms.Button button1;
	private System.Windows.Forms.CheckBox m_cbShowInterp;
	private System.Windows.Forms.ListView m_lvStats; 
	private SortedList m_slsMeals;
	private System.Windows.Forms.Label label20;
	private System.Windows.Forms.ComboBox m_cbxMeal;
	private System.Windows.Forms.TabPage m_tabMealCharts;
	private System.Windows.Forms.ComboBox m_cbxSearch;
	private System.Windows.Forms.Label label21;
	private System.Windows.Forms.ListView m_lvMeals;
	private System.Windows.Forms.Button m_pbDoMealChart;
	private System.Windows.Forms.Button m_pbSearch;
	private System.Windows.Forms.CheckBox m_cbRegEx;
	private System.Windows.Forms.Label label22;
	private System.Windows.Forms.Button m_pbClear;
	private System.Windows.Forms.MainMenu mainMenu1;
	private System.Windows.Forms.MenuItem m_mnuFile;
	private System.Windows.Forms.MenuItem m_mnuiFindSuspect;

	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public _bg()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();

		//
		// TODO: Add any constructor code after InitializeComponent call
		//
		m_cbxUpper.SelectedIndex = 1;
		m_cbxLower.SelectedIndex = 0;
		SetupListView(m_lvHistory);
		SetupListViewSearch(m_lvMeals);
		SetupListViewStats(m_lvStats);
		LoadBgData();

		m_cbxOrient.SelectedIndex = 1;
		m_cbxDateRange.SelectedIndex = 3;
		m_cbxFilterType.SelectedIndex = 0;
	}

	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose( bool disposing )
	{
		if( disposing )
		{
			if (components != null) 
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
		this.m_tabc = new System.Windows.Forms.TabControl();
		this.m_tabEntry = new System.Windows.Forms.TabPage();
		this.label20 = new System.Windows.Forms.Label();
		this.m_cbxMeal = new System.Windows.Forms.ComboBox();
		this.button1 = new System.Windows.Forms.Button();
		this.m_ebComment = new System.Windows.Forms.TextBox();
		this.label9 = new System.Windows.Forms.Label();
		this.m_ebCarbs = new System.Windows.Forms.TextBox();
		this.label8 = new System.Windows.Forms.Label();
		this.m_pbAdd = new System.Windows.Forms.Button();
		this.m_lvHistory = new System.Windows.Forms.ListView();
		this.label7 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.m_ebReading = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.m_ebTime = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.m_ebDate = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.m_cbxType = new System.Windows.Forms.ComboBox();
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.m_tabAnalysis = new System.Windows.Forms.TabPage();
		this.m_lvStats = new System.Windows.Forms.ListView();
		this.m_cbShowInterp = new System.Windows.Forms.CheckBox();
		this.label13 = new System.Windows.Forms.Label();
		this.m_cbxDateRange = new System.Windows.Forms.ComboBox();
		this.m_cbxFilterType = new System.Windows.Forms.ComboBox();
		this.m_cbxOrient = new System.Windows.Forms.ComboBox();
		this.m_cbxLower = new System.Windows.Forms.ComboBox();
		this.m_cbxUpper = new System.Windows.Forms.ComboBox();
		this.label31 = new System.Windows.Forms.Label();
		this.label30 = new System.Windows.Forms.Label();
		this.m_cbShowMeals = new System.Windows.Forms.CheckBox();
		this.m_ebIntervals = new System.Windows.Forms.TextBox();
		this.m_ebHigh = new System.Windows.Forms.TextBox();
		this.m_ebLow = new System.Windows.Forms.TextBox();
		this.label19 = new System.Windows.Forms.Label();
		this.label18 = new System.Windows.Forms.Label();
		this.label17 = new System.Windows.Forms.Label();
		this.m_ebDays = new System.Windows.Forms.TextBox();
		this.label16 = new System.Windows.Forms.Label();
		this.label15 = new System.Windows.Forms.Label();
		this.label11 = new System.Windows.Forms.Label();
		this.label10 = new System.Windows.Forms.Label();
		this.m_ebLast = new System.Windows.Forms.TextBox();
		this.m_ebFirst = new System.Windows.Forms.TextBox();
		this.label29 = new System.Windows.Forms.Label();
		this.label28 = new System.Windows.Forms.Label();
		this.m_ebFastLength = new System.Windows.Forms.TextBox();
		this.label14 = new System.Windows.Forms.Label();
		this.label12 = new System.Windows.Forms.Label();
		this.m_cbSnack = new System.Windows.Forms.CheckBox();
		this.m_cbDinner = new System.Windows.Forms.CheckBox();
		this.m_cbLunch = new System.Windows.Forms.CheckBox();
		this.m_cbBreakfast = new System.Windows.Forms.CheckBox();
		this.m_cbSpot = new System.Windows.Forms.CheckBox();
		this.m_pbGraph = new System.Windows.Forms.Button();
		this.m_tabMealCharts = new System.Windows.Forms.TabPage();
		this.m_pbClear = new System.Windows.Forms.Button();
		this.label22 = new System.Windows.Forms.Label();
		this.m_cbRegEx = new System.Windows.Forms.CheckBox();
		this.m_pbSearch = new System.Windows.Forms.Button();
		this.m_pbDoMealChart = new System.Windows.Forms.Button();
		this.m_lvMeals = new System.Windows.Forms.ListView();
		this.label21 = new System.Windows.Forms.Label();
		this.m_cbxSearch = new System.Windows.Forms.ComboBox();
		this.mainMenu1 = new System.Windows.Forms.MainMenu();
		this.m_mnuFile = new System.Windows.Forms.MenuItem();
		this.m_mnuiFindSuspect = new System.Windows.Forms.MenuItem();
		this.m_tabc.SuspendLayout();
		this.m_tabEntry.SuspendLayout();
		this.m_tabAnalysis.SuspendLayout();
		this.m_tabMealCharts.SuspendLayout();
		this.SuspendLayout();
		// 
		// m_tabc
		// 
		this.m_tabc.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right);
		this.m_tabc.Controls.AddRange(new System.Windows.Forms.Control[] {
																			 this.m_tabEntry,
																			 this.m_tabAnalysis,
																			 this.m_tabMealCharts});
		this.m_tabc.Location = new System.Drawing.Point(16, 24);
		this.m_tabc.Name = "m_tabc";
		this.m_tabc.SelectedIndex = 0;
		this.m_tabc.Size = new System.Drawing.Size(520, 528);
		this.m_tabc.TabIndex = 0;
		this.m_tabc.SelectedIndexChanged += new System.EventHandler(this.ChangeTabs);
		// 
		// m_tabEntry
		// 
		this.m_tabEntry.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.label20,
																				 this.m_cbxMeal,
																				 this.button1,
																				 this.m_ebComment,
																				 this.label9,
																				 this.m_ebCarbs,
																				 this.label8,
																				 this.m_pbAdd,
																				 this.m_lvHistory,
																				 this.label7,
																				 this.label6,
																				 this.m_ebReading,
																				 this.label5,
																				 this.m_ebTime,
																				 this.label4,
																				 this.m_ebDate,
																				 this.label3,
																				 this.m_cbxType,
																				 this.label2,
																				 this.label1});
		this.m_tabEntry.Location = new System.Drawing.Point(4, 22);
		this.m_tabEntry.Name = "m_tabEntry";
		this.m_tabEntry.Size = new System.Drawing.Size(512, 502);
		this.m_tabEntry.TabIndex = 0;
		this.m_tabEntry.Text = "Data Entry";
		// 
		// label20
		// 
		this.label20.Location = new System.Drawing.Point(16, 133);
		this.label20.Name = "label20";
		this.label20.Size = new System.Drawing.Size(56, 23);
		this.label20.TabIndex = 19;
		this.label20.Text = "Comment";
		// 
		// m_cbxMeal
		// 
		this.m_cbxMeal.ItemHeight = 13;
		this.m_cbxMeal.Location = new System.Drawing.Point(72, 104);
		this.m_cbxMeal.MaxDropDownItems = 25;
		this.m_cbxMeal.Name = "m_cbxMeal";
		this.m_cbxMeal.Size = new System.Drawing.Size(424, 21);
		this.m_cbxMeal.Sorted = true;
		this.m_cbxMeal.TabIndex = 18;
		// 
		// button1
		// 
		this.button1.Location = new System.Drawing.Point(416, 56);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(72, 24);
		this.button1.TabIndex = 17;
		this.button1.Text = "Read";
		this.button1.Click += new System.EventHandler(this.ReadFromDevice);
		// 
		// m_ebComment
		// 
		this.m_ebComment.Location = new System.Drawing.Point(72, 128);
		this.m_ebComment.Name = "m_ebComment";
		this.m_ebComment.Size = new System.Drawing.Size(424, 20);
		this.m_ebComment.TabIndex = 13;
		this.m_ebComment.Text = "";
		// 
		// label9
		// 
		this.label9.Location = new System.Drawing.Point(16, 109);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(56, 23);
		this.label9.TabIndex = 12;
		this.label9.Text = "Meal";
		// 
		// m_ebCarbs
		// 
		this.m_ebCarbs.Location = new System.Drawing.Point(200, 37);
		this.m_ebCarbs.Name = "m_ebCarbs";
		this.m_ebCarbs.Size = new System.Drawing.Size(56, 20);
		this.m_ebCarbs.TabIndex = 4;
		this.m_ebCarbs.Text = "";
		// 
		// label8
		// 
		this.label8.Location = new System.Drawing.Point(152, 40);
		this.label8.Name = "label8";
		this.label8.TabIndex = 3;
		this.label8.Text = "Carbs";
		// 
		// m_pbAdd
		// 
		this.m_pbAdd.Location = new System.Drawing.Point(416, 24);
		this.m_pbAdd.Name = "m_pbAdd";
		this.m_pbAdd.TabIndex = 15;
		this.m_pbAdd.Text = "Add";
		this.m_pbAdd.Click += new System.EventHandler(this.AddEntry);
		// 
		// m_lvHistory
		// 
		this.m_lvHistory.LabelEdit = true;
		this.m_lvHistory.Location = new System.Drawing.Point(16, 184);
		this.m_lvHistory.Name = "m_lvHistory";
		this.m_lvHistory.Size = new System.Drawing.Size(488, 312);
		this.m_lvHistory.TabIndex = 16;
		this.m_lvHistory.Click += new System.EventHandler(this.HandleHistoryClick);
		// 
		// label7
		// 
		this.label7.Location = new System.Drawing.Point(16, 160);
		this.label7.Name = "label7";
		this.label7.TabIndex = 14;
		this.label7.Text = "History";
		// 
		// label6
		// 
		this.label6.Location = new System.Drawing.Point(360, 72);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(32, 23);
		this.label6.TabIndex = 11;
		this.label6.Text = "mg/dl";
		// 
		// m_ebReading
		// 
		this.m_ebReading.Location = new System.Drawing.Point(320, 67);
		this.m_ebReading.Name = "m_ebReading";
		this.m_ebReading.Size = new System.Drawing.Size(40, 20);
		this.m_ebReading.TabIndex = 10;
		this.m_ebReading.Text = "";
		// 
		// label5
		// 
		this.label5.Location = new System.Drawing.Point(264, 72);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(48, 23);
		this.label5.TabIndex = 9;
		this.label5.Text = "Reading";
		// 
		// m_ebTime
		// 
		this.m_ebTime.Location = new System.Drawing.Point(200, 69);
		this.m_ebTime.Name = "m_ebTime";
		this.m_ebTime.Size = new System.Drawing.Size(56, 20);
		this.m_ebTime.TabIndex = 8;
		this.m_ebTime.Text = "";
		// 
		// label4
		// 
		this.label4.Location = new System.Drawing.Point(152, 72);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(32, 23);
		this.label4.TabIndex = 7;
		this.label4.Text = "Time";
		// 
		// m_ebDate
		// 
		this.m_ebDate.Location = new System.Drawing.Point(56, 69);
		this.m_ebDate.Name = "m_ebDate";
		this.m_ebDate.Size = new System.Drawing.Size(80, 20);
		this.m_ebDate.TabIndex = 6;
		this.m_ebDate.Text = "";
		// 
		// label3
		// 
		this.label3.Location = new System.Drawing.Point(16, 72);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(32, 23);
		this.label3.TabIndex = 5;
		this.label3.Text = "Date";
		// 
		// m_cbxType
		// 
		this.m_cbxType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_cbxType.Items.AddRange(new object[] {
													   "Breakfast",
													   "Control",
													   "Lunch",
													   "Dinner",
													   "Snack",
													   "SpotTest"});
		this.m_cbxType.Location = new System.Drawing.Point(56, 37);
		this.m_cbxType.Name = "m_cbxType";
		this.m_cbxType.Size = new System.Drawing.Size(80, 21);
		this.m_cbxType.TabIndex = 2;
		// 
		// label2
		// 
		this.label2.Location = new System.Drawing.Point(16, 40);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(40, 16);
		this.label2.TabIndex = 1;
		this.label2.Text = "Type";
		// 
		// label1
		// 
		this.label1.Location = new System.Drawing.Point(8, 8);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(88, 16);
		this.label1.TabIndex = 0;
		this.label1.Text = "Reading Entry";
		// 
		// m_tabAnalysis
		// 
		this.m_tabAnalysis.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.m_lvStats,
																					this.m_cbShowInterp,
																					this.label13,
																					this.m_cbxDateRange,
																					this.m_cbxFilterType,
																					this.m_cbxOrient,
																					this.m_cbxLower,
																					this.m_cbxUpper,
																					this.label31,
																					this.label30,
																					this.m_cbShowMeals,
																					this.m_ebIntervals,
																					this.m_ebHigh,
																					this.m_ebLow,
																					this.label19,
																					this.label18,
																					this.label17,
																					this.m_ebDays,
																					this.label16,
																					this.label15,
																					this.label11,
																					this.label10,
																					this.m_ebLast,
																					this.m_ebFirst,
																					this.label29,
																					this.label28,
																					this.m_ebFastLength,
																					this.label14,
																					this.label12,
																					this.m_cbSnack,
																					this.m_cbDinner,
																					this.m_cbLunch,
																					this.m_cbBreakfast,
																					this.m_cbSpot,
																					this.m_pbGraph});
		this.m_tabAnalysis.Location = new System.Drawing.Point(4, 22);
		this.m_tabAnalysis.Name = "m_tabAnalysis";
		this.m_tabAnalysis.Size = new System.Drawing.Size(512, 502);
		this.m_tabAnalysis.TabIndex = 1;
		this.m_tabAnalysis.Text = "Analysis";
		// 
		// m_lvStats
		// 
		this.m_lvStats.Location = new System.Drawing.Point(8, 296);
		this.m_lvStats.Name = "m_lvStats";
		this.m_lvStats.Size = new System.Drawing.Size(504, 144);
		this.m_lvStats.TabIndex = 46;
		// 
		// m_cbShowInterp
		// 
		this.m_cbShowInterp.Checked = true;
		this.m_cbShowInterp.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbShowInterp.Location = new System.Drawing.Point(152, 248);
		this.m_cbShowInterp.Name = "m_cbShowInterp";
		this.m_cbShowInterp.Size = new System.Drawing.Size(152, 24);
		this.m_cbShowInterp.TabIndex = 45;
		this.m_cbShowInterp.Text = "Show Interpolations";
		// 
		// label13
		// 
		this.label13.Location = new System.Drawing.Point(8, 200);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(496, 16);
		this.label13.TabIndex = 42;
		this.label13.Text = "Graph Options";
		this.label13.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderWithLine);
		// 
		// m_cbxDateRange
		// 
		this.m_cbxDateRange.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_cbxDateRange.Items.AddRange(new object[] {
															"7 Days",
															"15 Days",
															"30 Days",
															"Custom"});
		this.m_cbxDateRange.Location = new System.Drawing.Point(232, 104);
		this.m_cbxDateRange.Name = "m_cbxDateRange";
		this.m_cbxDateRange.Size = new System.Drawing.Size(88, 21);
		this.m_cbxDateRange.TabIndex = 41;
		this.m_cbxDateRange.SelectedIndexChanged += new System.EventHandler(this.SelectDateRange);
		// 
		// m_cbxFilterType
		// 
		this.m_cbxFilterType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_cbxFilterType.Items.AddRange(new object[] {
															 "Custom",
															 "Fasting"});
		this.m_cbxFilterType.Location = new System.Drawing.Point(384, 56);
		this.m_cbxFilterType.Name = "m_cbxFilterType";
		this.m_cbxFilterType.Size = new System.Drawing.Size(128, 21);
		this.m_cbxFilterType.TabIndex = 40;
		// 
		// m_cbxOrient
		// 
		this.m_cbxOrient.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_cbxOrient.Items.AddRange(new object[] {
														 "Portrait",
														 "Landscape"});
		this.m_cbxOrient.Location = new System.Drawing.Point(16, 160);
		this.m_cbxOrient.Name = "m_cbxOrient";
		this.m_cbxOrient.Size = new System.Drawing.Size(128, 21);
		this.m_cbxOrient.TabIndex = 39;
		// 
		// m_cbxLower
		// 
		this.m_cbxLower.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_cbxLower.Items.AddRange(new object[] {
														"Empty",
														"Graph",
														"Log"});
		this.m_cbxLower.Location = new System.Drawing.Point(352, 160);
		this.m_cbxLower.Name = "m_cbxLower";
		this.m_cbxLower.Size = new System.Drawing.Size(72, 21);
		this.m_cbxLower.TabIndex = 38;
		// 
		// m_cbxUpper
		// 
		this.m_cbxUpper.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_cbxUpper.Items.AddRange(new object[] {
														"Empty",
														"Graph",
														"Log"});
		this.m_cbxUpper.Location = new System.Drawing.Point(216, 160);
		this.m_cbxUpper.Name = "m_cbxUpper";
		this.m_cbxUpper.Size = new System.Drawing.Size(72, 21);
		this.m_cbxUpper.TabIndex = 37;
		// 
		// label31
		// 
		this.label31.Location = new System.Drawing.Point(288, 164);
		this.label31.Name = "label31";
		this.label31.Size = new System.Drawing.Size(64, 23);
		this.label31.TabIndex = 36;
		this.label31.Text = "Lower Box";
		// 
		// label30
		// 
		this.label30.Location = new System.Drawing.Point(152, 164);
		this.label30.Name = "label30";
		this.label30.Size = new System.Drawing.Size(64, 23);
		this.label30.TabIndex = 35;
		this.label30.Text = "Upper Box";
		// 
		// m_cbShowMeals
		// 
		this.m_cbShowMeals.Checked = true;
		this.m_cbShowMeals.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbShowMeals.Location = new System.Drawing.Point(16, 248);
		this.m_cbShowMeals.Name = "m_cbShowMeals";
		this.m_cbShowMeals.Size = new System.Drawing.Size(128, 24);
		this.m_cbShowMeals.TabIndex = 19;
		this.m_cbShowMeals.Text = "Connect meal points";
		// 
		// m_ebIntervals
		// 
		this.m_ebIntervals.Location = new System.Drawing.Point(304, 224);
		this.m_ebIntervals.Name = "m_ebIntervals";
		this.m_ebIntervals.Size = new System.Drawing.Size(48, 20);
		this.m_ebIntervals.TabIndex = 29;
		this.m_ebIntervals.Text = "19";
		// 
		// m_ebHigh
		// 
		this.m_ebHigh.Location = new System.Drawing.Point(192, 224);
		this.m_ebHigh.Name = "m_ebHigh";
		this.m_ebHigh.Size = new System.Drawing.Size(48, 20);
		this.m_ebHigh.TabIndex = 27;
		this.m_ebHigh.Text = "220";
		// 
		// m_ebLow
		// 
		this.m_ebLow.Location = new System.Drawing.Point(72, 224);
		this.m_ebLow.Name = "m_ebLow";
		this.m_ebLow.Size = new System.Drawing.Size(48, 20);
		this.m_ebLow.TabIndex = 25;
		this.m_ebLow.Text = "30";
		// 
		// label19
		// 
		this.label19.Location = new System.Drawing.Point(248, 227);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(48, 23);
		this.label19.TabIndex = 28;
		this.label19.Text = "Intervals";
		// 
		// label18
		// 
		this.label18.Location = new System.Drawing.Point(136, 227);
		this.label18.Name = "label18";
		this.label18.Size = new System.Drawing.Size(48, 23);
		this.label18.TabIndex = 26;
		this.label18.Text = "High bg";
		// 
		// label17
		// 
		this.label17.Location = new System.Drawing.Point(16, 227);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(48, 23);
		this.label17.TabIndex = 24;
		this.label17.Text = "Low bg";
		// 
		// m_ebDays
		// 
		this.m_ebDays.Location = new System.Drawing.Point(448, 224);
		this.m_ebDays.Name = "m_ebDays";
		this.m_ebDays.Size = new System.Drawing.Size(24, 20);
		this.m_ebDays.TabIndex = 18;
		this.m_ebDays.Text = "7";
		// 
		// label16
		// 
		this.label16.Location = new System.Drawing.Point(360, 227);
		this.label16.Name = "label16";
		this.label16.TabIndex = 17;
		this.label16.Text = "Days per Page";
		// 
		// label15
		// 
		this.label15.Location = new System.Drawing.Point(8, 136);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(496, 16);
		this.label15.TabIndex = 12;
		this.label15.Text = "Report Options";
		this.label15.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderWithLine);
		// 
		// label11
		// 
		this.label11.Location = new System.Drawing.Point(128, 104);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(32, 23);
		this.label11.TabIndex = 15;
		this.label11.Text = "To";
		// 
		// label10
		// 
		this.label10.Location = new System.Drawing.Point(16, 104);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(40, 23);
		this.label10.TabIndex = 13;
		this.label10.Text = "From";
		// 
		// m_ebLast
		// 
		this.m_ebLast.Location = new System.Drawing.Point(160, 104);
		this.m_ebLast.Name = "m_ebLast";
		this.m_ebLast.Size = new System.Drawing.Size(56, 20);
		this.m_ebLast.TabIndex = 16;
		this.m_ebLast.Text = "";
		// 
		// m_ebFirst
		// 
		this.m_ebFirst.Location = new System.Drawing.Point(56, 104);
		this.m_ebFirst.Name = "m_ebFirst";
		this.m_ebFirst.Size = new System.Drawing.Size(48, 20);
		this.m_ebFirst.TabIndex = 14;
		this.m_ebFirst.Text = "";
		// 
		// label29
		// 
		this.label29.Location = new System.Drawing.Point(472, 86);
		this.label29.Name = "label29";
		this.label29.Size = new System.Drawing.Size(40, 16);
		this.label29.TabIndex = 10;
		this.label29.Text = "hours";
		// 
		// label28
		// 
		this.label28.Location = new System.Drawing.Point(8, 280);
		this.label28.Name = "label28";
		this.label28.Size = new System.Drawing.Size(496, 16);
		this.label28.TabIndex = 30;
		this.label28.Text = "Statistics";
		this.label28.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderWithLine);
		// 
		// m_ebFastLength
		// 
		this.m_ebFastLength.Location = new System.Drawing.Point(432, 83);
		this.m_ebFastLength.Name = "m_ebFastLength";
		this.m_ebFastLength.Size = new System.Drawing.Size(32, 20);
		this.m_ebFastLength.TabIndex = 9;
		this.m_ebFastLength.Text = "7";
		// 
		// label14
		// 
		this.label14.Location = new System.Drawing.Point(360, 86);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(96, 24);
		this.label14.TabIndex = 8;
		this.label14.Text = "Fast Length:";
		// 
		// label12
		// 
		this.label12.Location = new System.Drawing.Point(8, 32);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(496, 23);
		this.label12.TabIndex = 0;
		this.label12.Text = "Filter Options";
		this.label12.Paint += new System.Windows.Forms.PaintEventHandler(this.RenderWithLine);
		// 
		// m_cbSnack
		// 
		this.m_cbSnack.Checked = true;
		this.m_cbSnack.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbSnack.Location = new System.Drawing.Point(200, 56);
		this.m_cbSnack.Name = "m_cbSnack";
		this.m_cbSnack.TabIndex = 3;
		this.m_cbSnack.Text = "Snack";
		// 
		// m_cbDinner
		// 
		this.m_cbDinner.Checked = true;
		this.m_cbDinner.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbDinner.Location = new System.Drawing.Point(128, 72);
		this.m_cbDinner.Name = "m_cbDinner";
		this.m_cbDinner.TabIndex = 5;
		this.m_cbDinner.Text = "Dinner";
		// 
		// m_cbLunch
		// 
		this.m_cbLunch.Checked = true;
		this.m_cbLunch.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbLunch.Location = new System.Drawing.Point(128, 56);
		this.m_cbLunch.Name = "m_cbLunch";
		this.m_cbLunch.TabIndex = 2;
		this.m_cbLunch.Text = "Lunch";
		// 
		// m_cbBreakfast
		// 
		this.m_cbBreakfast.Checked = true;
		this.m_cbBreakfast.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbBreakfast.Location = new System.Drawing.Point(16, 72);
		this.m_cbBreakfast.Name = "m_cbBreakfast";
		this.m_cbBreakfast.TabIndex = 4;
		this.m_cbBreakfast.Text = "Breakfast";
		// 
		// m_cbSpot
		// 
		this.m_cbSpot.Checked = true;
		this.m_cbSpot.CheckState = System.Windows.Forms.CheckState.Checked;
		this.m_cbSpot.Location = new System.Drawing.Point(16, 56);
		this.m_cbSpot.Name = "m_cbSpot";
		this.m_cbSpot.TabIndex = 1;
		this.m_cbSpot.Text = "SpotTests";
		// 
		// m_pbGraph
		// 
		this.m_pbGraph.Location = new System.Drawing.Point(440, 8);
		this.m_pbGraph.Name = "m_pbGraph";
		this.m_pbGraph.Size = new System.Drawing.Size(72, 24);
		this.m_pbGraph.TabIndex = 6;
		this.m_pbGraph.Text = "&Generate";
		this.m_pbGraph.Click += new System.EventHandler(this.DoGraph);
		// 
		// m_tabMealCharts
		// 
		this.m_tabMealCharts.Controls.AddRange(new System.Windows.Forms.Control[] {
																					  this.m_pbClear,
																					  this.label22,
																					  this.m_cbRegEx,
																					  this.m_pbSearch,
																					  this.m_pbDoMealChart,
																					  this.m_lvMeals,
																					  this.label21,
																					  this.m_cbxSearch});
		this.m_tabMealCharts.Location = new System.Drawing.Point(4, 22);
		this.m_tabMealCharts.Name = "m_tabMealCharts";
		this.m_tabMealCharts.Size = new System.Drawing.Size(512, 502);
		this.m_tabMealCharts.TabIndex = 2;
		this.m_tabMealCharts.Text = "Meal Charts";
		// 
		// m_pbClear
		// 
		this.m_pbClear.Location = new System.Drawing.Point(440, 80);
		this.m_pbClear.Name = "m_pbClear";
		this.m_pbClear.Size = new System.Drawing.Size(72, 24);
		this.m_pbClear.TabIndex = 7;
		this.m_pbClear.Text = "Clear";
		this.m_pbClear.Click += new System.EventHandler(this.ClearSearch);
		// 
		// label22
		// 
		this.label22.Location = new System.Drawing.Point(8, 32);
		this.label22.Name = "label22";
		this.label22.Size = new System.Drawing.Size(496, 23);
		this.label22.TabIndex = 6;
		this.label22.Text = "Meal Searching";
		// 
		// m_cbRegEx
		// 
		this.m_cbRegEx.Location = new System.Drawing.Point(8, 56);
		this.m_cbRegEx.Name = "m_cbRegEx";
		this.m_cbRegEx.Size = new System.Drawing.Size(152, 16);
		this.m_cbRegEx.TabIndex = 5;
		this.m_cbRegEx.Text = "Use Regular Expressions";
		// 
		// m_pbSearch
		// 
		this.m_pbSearch.Location = new System.Drawing.Point(440, 56);
		this.m_pbSearch.Name = "m_pbSearch";
		this.m_pbSearch.Size = new System.Drawing.Size(72, 24);
		this.m_pbSearch.TabIndex = 4;
		this.m_pbSearch.Text = "Search";
		this.m_pbSearch.Click += new System.EventHandler(this.DoMealSearch);
		// 
		// m_pbDoMealChart
		// 
		this.m_pbDoMealChart.Location = new System.Drawing.Point(440, 8);
		this.m_pbDoMealChart.Name = "m_pbDoMealChart";
		this.m_pbDoMealChart.Size = new System.Drawing.Size(72, 24);
		this.m_pbDoMealChart.TabIndex = 3;
		this.m_pbDoMealChart.Text = "&Generate";
		this.m_pbDoMealChart.Click += new System.EventHandler(this.GenerateMealChart);
		// 
		// m_lvMeals
		// 
		this.m_lvMeals.Location = new System.Drawing.Point(16, 112);
		this.m_lvMeals.Name = "m_lvMeals";
		this.m_lvMeals.Size = new System.Drawing.Size(472, 368);
		this.m_lvMeals.TabIndex = 2;
		// 
		// label21
		// 
		this.label21.Location = new System.Drawing.Point(8, 80);
		this.label21.Name = "label21";
		this.label21.Size = new System.Drawing.Size(56, 24);
		this.label21.TabIndex = 1;
		this.label21.Text = "Search";
		// 
		// m_cbxSearch
		// 
		this.m_cbxSearch.Location = new System.Drawing.Point(64, 77);
		this.m_cbxSearch.Name = "m_cbxSearch";
		this.m_cbxSearch.Size = new System.Drawing.Size(368, 21);
		this.m_cbxSearch.Sorted = true;
		this.m_cbxSearch.TabIndex = 0;
		this.m_cbxSearch.Text = "Penne Puttanesca";
		// 
		// mainMenu1
		// 
		this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																				  this.m_mnuFile});
		// 
		// m_mnuFile
		// 
		this.m_mnuFile.Index = 0;
		this.m_mnuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																				  this.m_mnuiFindSuspect});
		this.m_mnuFile.Text = "&File";
		// 
		// m_mnuiFindSuspect
		// 
		this.m_mnuiFindSuspect.Index = 0;
		this.m_mnuiFindSuspect.Text = "Find Suspect Readings";
		this.m_mnuiFindSuspect.Click += new System.EventHandler(this.FindSuspectReadings);
		// 
		// _bg
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(544, 574);
		this.Controls.AddRange(new System.Windows.Forms.Control[] {
																	  this.m_tabc});
		this.Menu = this.mainMenu1;
		this.Name = "_bg";
		this.Text = "bgGraph";
		this.m_tabc.ResumeLayout(false);
		this.m_tabEntry.ResumeLayout(false);
		this.m_tabAnalysis.ResumeLayout(false);
		this.m_tabMealCharts.ResumeLayout(false);
		this.ResumeLayout(false);

	}
	#endregion

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main() 
	{
		Application.Run(new _bg());
	}

	/* S E T U P  L I S T  V I E W */
	/*----------------------------------------------------------------------------
		%%Function: SetupListView
		%%Qualified: bg._bg.SetupListView
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	void SetupListView(ListView lv)
	{
		lv.Columns.Add(new ColumnHeader());
		lv.Columns[0].Text = "Date";
		lv.Columns[0].Width = 128;

		lv.Columns.Add(new ColumnHeader());
		lv.Columns[1].Text = "Type";
		lv.Columns[1].Width = 128;

		lv.Columns.Add(new ColumnHeader());
		lv.Columns[2].Text = "bg (mg/dl)";
		lv.Columns[2].Width = 32;
		lv.Columns[2].TextAlign = HorizontalAlignment.Right;

		lv.Columns.Add(new ColumnHeader());
		lv.Columns[3].Text = "Carbs";
		lv.Columns[3].Width = 48;
		lv.Columns[3].TextAlign = HorizontalAlignment.Right;

		lv.Columns.Add(new ColumnHeader());
		lv.Columns[4].Text = "Comment";
		lv.Columns[4].Width = 256;

		lv.Columns[0].Width = 128;

		lv.FullRowSelect = true;
		lv.MultiSelect = false;
		lv.View = View.Details;

		m_lvHistory.ListViewItemSorter = new ListViewItemComparer(0);
		m_lvHistory.ColumnClick += new ColumnClickEventHandler(HandleColumn);
		AddBge(null);
	}

	void SetupListViewSearch(ListView lv)
	{
		lv.Columns.Add(new ColumnHeader());
		lv.Columns[0].Text = "Group";
		lv.Columns[0].Width = 32;

		lv.Columns.Add(new ColumnHeader());
		lv.Columns[1].Text = "Date";
		lv.Columns[1].Width = 128;

		lv.Columns.Add(new ColumnHeader());
		lv.Columns[2].Text = "Carbs";
		lv.Columns[2].Width = 48;
		lv.Columns[2].TextAlign = HorizontalAlignment.Right;

		lv.Columns.Add(new ColumnHeader());
		lv.Columns[3].Text = "Comment";
		lv.Columns[3].Width = 256;

		lv.Columns[0].Width = 32;

		lv.FullRowSelect = true;
		lv.MultiSelect = false;
		lv.View = View.Details;

//		m_lvHistory.ListViewItemSorter = new ListViewItemComparer(0);
//		m_lvHistory.ColumnClick += new ColumnClickEventHandler(HandleColumn);
//		AddBge(null);
	}

	/* S E T U P  L I S T  V I E W  S T A T S */
	/*----------------------------------------------------------------------------
		%%Function: SetupListViewStats
		%%Qualified: bg._bg.SetupListViewStats
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	void SetupListViewStats(ListView lv)
	{
		lv.Columns.Add(new ColumnHeader());
		lv.Columns[0].Text = "Description";
		lv.Columns[0].Width = 100;

		lv.Columns.Add(new ColumnHeader());
		lv.Columns[1].Text = "All";
		lv.Columns[1].Width = 45;
		lv.Columns[1].TextAlign = HorizontalAlignment.Right;

		lv.Columns.Add(new ColumnHeader());
		lv.Columns[2].Text = "Fast";
		lv.Columns[2].Width = 45;
		lv.Columns[2].TextAlign = HorizontalAlignment.Right;

		lv.Columns.Add(new ColumnHeader());
		lv.Columns[3].Text = "Wgt";
		lv.Columns[3].Width = 45;
		lv.Columns[3].TextAlign = HorizontalAlignment.Right;

		lv.Columns.Add(new ColumnHeader());
		lv.Columns[4].Text = "A1c";
		lv.Columns[4].Width = 45;
		lv.Columns[4].TextAlign = HorizontalAlignment.Right;


		lv.FullRowSelect = true;
		lv.MultiSelect = false;
		lv.View = View.Details;
	}

	/* I N I T  R E A D I N G S */
	/*----------------------------------------------------------------------------
		%%Function: InitReadings
		%%Qualified: bg._bg.InitReadings
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	void InitReadings()
	{
		m_slbge = new SortedList();
		m_slsMeals = new SortedList();
	}

	void AddMeal(string sMeal)
	{
		m_slsMeals.Add(sMeal, sMeal);
		m_cbxMeal.Items.Add(sMeal);
		m_cbxSearch.Items.Add(sMeal);
	}

	/* L O A D  B G  D A T A */
	/*----------------------------------------------------------------------------
		%%Function: LoadBgData
		%%Qualified: bg._bg.LoadBgData
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	void LoadBgData()
	{
		XmlDocument dom = new XmlDocument();

		InitReadings();

		dom.Load("c:\\docs\\bg.xml");

		XmlNamespaceManager nsmgr = new XmlNamespaceManager(dom.NameTable);

		nsmgr.AddNamespace("b", "http://www.thetasoft.com/schemas/bg");

		XmlNodeList nodes = dom.SelectNodes("//b:reading", nsmgr);

		if (nodes != null && nodes.Count > 0)
			{
			foreach (XmlNode nodeT in nodes)
				{
				BGE bge;

				string sDate, sTime, sType, sComment, sMeal;
				int nBg, nCarbs;
				BGE.ReadingType type;

				int iChild = 0;

				sDate = nodeT.ChildNodes[iChild++].InnerText;
				sTime = nodeT.ChildNodes[iChild++].InnerText;
				nBg = 0;
				nCarbs = 0;

				if (iChild < nodeT.ChildNodes.Count
					&& String.Compare(nodeT.ChildNodes[iChild].LocalName, "bg") == 0)
					{
					string sBg = nodeT.ChildNodes[iChild].InnerText;

					if (sBg.Length > 0)
						nBg = Int32.Parse(sBg);

					iChild++;
					}

				if (iChild < nodeT.ChildNodes.Count
					&& String.Compare(nodeT.ChildNodes[iChild].LocalName, "carbs") == 0)
					{
					string sCarbs = nodeT.ChildNodes[iChild].InnerText;

					if (sCarbs.Length > 0)
						nCarbs = Int32.Parse(sCarbs);

					iChild++;
					}

				if (iChild < nodeT.ChildNodes.Count
					&& String.Compare(nodeT.ChildNodes[iChild].LocalName, "comment") == 0)
					{
					sComment = nodeT.ChildNodes[iChild].InnerText;
					iChild++;
					}
				else
					{
					sComment = "";
					}

				if (iChild < nodeT.ChildNodes.Count
					&& String.Compare(nodeT.ChildNodes[iChild].LocalName, "meal") == 0)
					{
					sMeal = nodeT.ChildNodes[iChild].InnerText;
					iChild++;
					}
				else
					{
					sMeal = "";
					}

				sType = nodeT.Attributes.GetNamedItem("type", "").Value;

//				sDate = nodeT.SelectSingleNode("b:date", nsmgr).InnerText;
//				sTime = nodeT.SelectSingleNode("b:time", nsmgr).InnerText;
//				sType = nodeT.SelectSingleNode("@type").Value;
//				try
//				{
//					sComment = nodeT.SelectSingleNode("b:comment", nsmgr).InnerText;
//				} catch { sComment = ""; };

//				try
//				{
//					sMeal = nodeT.SelectSingleNode("b:meal", nsmgr).InnerText;
//				} catch { sMeal = ""; };

//				try
//				{
//					nCarbs = Int32.Parse(nodeT.SelectSingleNode("b:carbs", nsmgr).InnerText);
//				} catch { nCarbs = 0; };

				type = BGE.ReadingTypeFromString(sType);
				
				bge = new BGE(sDate, sTime, type, nBg, nCarbs, sComment, sMeal);
				m_slbge.Add(bge.Key, bge);
				}
			}

		foreach (BGE bge in m_slbge.Values)
			{
			AddBge(bge);

			try
				{
				if (bge.Meal.Length > 0)
					{
					AddMeal(bge.Meal);
					}
				}
			catch 
				{
				}
			}

		m_dom = dom;
		m_nsmgr = nsmgr;
	}

	/* U P D A T E  B G E */
	/*----------------------------------------------------------------------------
		%%Function: UpdateBge
		%%Qualified: bg._bg.UpdateBge
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	void UpdateBge(BGE bge, ListViewItem lvi)
	{
		lvi.Tag = bge;

		if (bge == null)
			{
			lvi.SubItems[0].Text = "* (New Entry)";
			}
		else
			{
			lvi.SubItems[4].Text = bge.FullComment;
			if (bge.Carbs != 0)
				lvi.SubItems[3].Text = bge.Carbs.ToString();
			lvi.SubItems[2].Text = bge.Reading.ToString();
			lvi.SubItems[1].Text = BGE.StringFromReadingType(bge.Type);
			lvi.SubItems[0].Text = bge.Date.ToString();
			if (bge.Type == BGE.ReadingType.New)
				lvi.BackColor = Color.Yellow;
			else
				lvi.BackColor = m_lvHistory.BackColor;
			}
	}

	/* A D D  B G E */
	/*----------------------------------------------------------------------------
		%%Function: AddBge
		%%Qualified: bg._bg.AddBge
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	void AddBge(BGE bge)
	{
		ListViewItem lvi = new ListViewItem();

		lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
		lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
		lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
		lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
		lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
		if (bge != null && bge.Type == BGE.ReadingType.New)
			lvi.BackColor = Color.Yellow;
		m_lvHistory.Items.Add(lvi);
		UpdateBge(bge, lvi);
	}

	ListViewItem LviFindBge(BGE bge)
	{
		foreach (ListViewItem lviT in m_lvHistory.Items)
			{
			if (((BGE)lviT.Tag) == bge)
				return lviT;
			}
		return null;
	}

	/* A D D  E N T R Y  C O R E */
	/*----------------------------------------------------------------------------
		%%Function: AddEntryCore
		%%Qualified: bg._bg.AddEntryCore
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	void AddEntryCore(BGE bge, bool fFromDevice)
	{
		BGE bgeCurrent = m_bgeCurrent;

		if (fFromDevice)
			{
			int iKey = m_slbge.IndexOfKey(bge.Key);

			if (iKey >= 0)
				{
				bgeCurrent = (BGE)m_slbge.GetByIndex(iKey);

				if (bgeCurrent.Reading == bge.Reading)
					// readings match as does the date; nothing to do
					return;

				MessageBox.Show("Conflicting entry read from device!  bgeOld.Reading = " + bgeCurrent.Reading.ToString() + "("+bgeCurrent.Date.ToString("s")+"), bgeNew.Reading = " + bge.Reading.ToString());
				return;
				}
			// no match yet...look for a "close" match
			DateTime dttm = new DateTime(bge.Date.Year, bge.Date.Month, bge.Date.Day, bge.Date.Hour, bge.Date.Minute, 0);
			iKey = m_slbge.IndexOfKey(dttm.ToString("s"));
			if (iKey >= 0)
				{
				//ooh a match.  update this one if the readings match
				bgeCurrent = (BGE)m_slbge.GetByIndex(iKey);

				if (bgeCurrent.Reading != bge.Reading)
					{
					MessageBox.Show("Conflicting entry read from device!  bgeOld.Reading = " + bgeCurrent.Reading.ToString() + "("+bgeCurrent.Date.ToString("s")+"), bgeNew.Reading = " + bge.Reading.ToString());
					return;
					}
				// otherwise, fallthrough and do the update
				bge.Comment = bgeCurrent.Comment;
				bge.Meal = bgeCurrent.Meal;
				bge.Type = bgeCurrent.Type;
				}
			}

		XmlNode nodeReading = m_dom.CreateElement("", "reading", "http://www.thetasoft.com/schemas/bg");
		XmlNode nodeBg = m_dom.CreateElement("", "bg", "http://www.thetasoft.com/schemas/bg");
		XmlNode nodeDate = m_dom.CreateElement("", "date", "http://www.thetasoft.com/schemas/bg");
		XmlNode nodeTime = m_dom.CreateElement("", "time", "http://www.thetasoft.com/schemas/bg");

		XmlNode nodeComment = null;
		XmlNode nodeMeal = null;
		XmlNode nodeCarbs = null;

		if (m_ebCarbs.Text.Length > 0)
			nodeCarbs = m_dom.CreateElement("", "carbs", "http://www.thetasoft.com/schemas/bg");

		if (m_ebComment.Text.Length > 0)
			nodeComment = m_dom.CreateElement("", "comment", "http://www.thetasoft.com/schemas/bg");

		if (m_cbxMeal.Text.Length > 0)
			nodeMeal = m_dom.CreateElement("", "meal", "http://www.thetasoft.com/schemas/bg");

		nodeBg.InnerText = bge.Reading.ToString();
		nodeDate.InnerText = bge.Date.ToString("d");
		nodeTime.InnerText = bge.Date.ToString("T", DateTimeFormatInfo.InvariantInfo);
		nodeReading.AppendChild(nodeDate);
		nodeReading.AppendChild(nodeTime);
		nodeReading.AppendChild(nodeBg);
		if (nodeCarbs != null)
			{
			nodeCarbs.InnerText = bge.Carbs.ToString();
			nodeReading.AppendChild(nodeCarbs);
			}

		if (nodeComment != null)
			{
			nodeComment.InnerText = bge.Comment;
			nodeReading.AppendChild(nodeComment);
			}

		if (nodeMeal != null)
			{
			nodeMeal.InnerText = bge.Meal;
			nodeReading.AppendChild(nodeMeal);
			}

		nodeReading.Attributes.Append(m_dom.CreateAttribute("type"));
		nodeReading.Attributes["type"].Value = BGE.StringFromReadingType(bge.Type);

		if (bgeCurrent == null)
			{
			int iKey;
			if ((iKey = m_slbge.IndexOfKey(bge.Key)) >= 0)
				{
				if (!fFromDevice)
					{
					MessageBox.Show("Duplicate entry detected!");
					return;
					}

				BGE bgeOld = (BGE)m_slbge.GetByIndex(iKey);
				if (bgeOld.Reading == bge.Reading)
					{
					// readings match, so does date, do nothing
					return;
					}

				MessageBox.Show("Conflicting entry read from device!  bgeOld.Reading = " + bgeOld.Reading.ToString() + ", bgeNew.Reading = " + bge.Reading.ToString());
				return;
				}

			AddBge(bge);
			m_slbge.Add(bge.Key, bge);
			m_dom.SelectSingleNode("/b:bg", m_nsmgr).AppendChild(nodeReading);
			m_dom.Save("c:\\docs\\bg.xml");
			}
		else
			{
			XmlNode node = m_dom.SelectSingleNode("/b:bg/b:reading[b:date='"+bgeCurrent.Date.ToString("d")+"' and b:time='"+bgeCurrent.Date.ToString("T", DateTimeFormatInfo.InvariantInfo)+"']", m_nsmgr);
			XmlNode nodeRoot = m_dom.SelectSingleNode("/b:bg", m_nsmgr);

			nodeRoot.RemoveChild(node);
			nodeRoot.AppendChild(nodeReading);

			// we need to edit the current item.
			bgeCurrent.SetTo(bge);	// this takes care of m_slbge
			ListViewItem lvi = null;

			// try to add it to the list of available meals
			try
				{
				if (bge.Meal.Length > 0)
					{
					AddMeal(bge.Meal);
					}
				}
			catch 
				{
				}

			if (bgeCurrent != m_bgeCurrent)
				{
				lvi = LviFindBge(bgeCurrent);

				if (lvi == null)
					MessageBox.Show("Couldn't find matching LVI for BGE");
				}
			else
				lvi = m_lvHistory.SelectedItems[0];

			UpdateBge(bgeCurrent, lvi);				// this handles updating the listbox
			if (fFromDevice)
				lvi.BackColor = Color.LightBlue;
			m_dom.Save("c:\\docs\\bg.xml");
			}

		m_fDirtyStats = true;
	}

	/* A D D  E N T R Y */
	/*----------------------------------------------------------------------------
		%%Function: AddEntry
		%%Qualified: bg._bg.AddEntry
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	private void AddEntry(object sender, System.EventArgs e) 
	{  
		int nCarbs = m_ebCarbs.Text.Length > 0 ? Int32.Parse(m_ebCarbs.Text) : 0;
		BGE bge = new BGE(m_ebDate.Text, m_ebTime.Text, BGE.ReadingTypeFromString(m_cbxType.Text), Int32.Parse(m_ebReading.Text), nCarbs, m_ebComment.Text, m_cbxMeal.Text);

		AddEntryCore(bge, false);
	}

	/* H A N D L E  C O L U M N */
	/*----------------------------------------------------------------------------
		%%Function: HandleColumn
		%%Qualified: bg._bg.HandleColumn
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	private void HandleColumn(object sender, System.Windows.Forms.ColumnClickEventArgs e) 
	{
		if (((ListView)sender).ListViewItemSorter == null)
			((ListView)sender).ListViewItemSorter = new ListViewItemComparer(e.Column);
		else
			((ListViewItemComparer)(((ListView)sender).ListViewItemSorter)).SetColumn(e.Column);
		((ListView)sender).Sort();
	}

	/* D I S P L A Y  B G E */
	/*----------------------------------------------------------------------------
		%%Function: DisplayBge
		%%Qualified: bg._bg.DisplayBge
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	void DisplayBge(BGE bge)
	{
		if (bge == null)
			{
			m_ebDate.Text = System.DateTime.Now.ToString("d");
			m_ebTime.Text = System.DateTime.Now.ToString("T", DateTimeFormatInfo.InvariantInfo);
			m_ebComment.Text = "";
			m_cbxMeal.Text = "";
			m_cbxType.Text = "SpotTest";
			m_ebReading.Text = "";
			m_ebCarbs.Text = "";
			m_pbAdd.Text = "Add";
			m_bgeCurrent = null;
			}
		else
			{
			m_ebDate.Text = bge.Date.ToString("d");
			m_ebTime.Text = bge.Date.ToString("T", DateTimeFormatInfo.InvariantInfo);
			m_ebComment.Text = bge.Comment;
			m_cbxMeal.Text = bge.Meal;
			m_ebReading.Text = bge.Reading.ToString();
			if (bge.Carbs != 0)
				m_ebCarbs.Text = bge.Carbs.ToString(); 
			else
				m_ebCarbs.Text = "";
			m_cbxType.Text = BGE.StringFromReadingType(bge.Type);
			m_pbAdd.Text = "Update";
			m_bgeCurrent = bge;
			}
	}

	/* H A N D L E  H I S T O R Y  C L I C K */
	/*----------------------------------------------------------------------------
		%%Function: HandleHistoryClick
		%%Qualified: bg._bg.HandleHistoryClick
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	private void HandleHistoryClick(object sender, System.EventArgs e) 
	{
		ListView lv = (ListView)sender;
		ListViewItem lvi = lv.SelectedItems[0];
		BGE bge = (BGE)lvi.Tag;

		DisplayBge(bge);
	}

	/* S L B G E  C A L C  C U S T O M */
	/*----------------------------------------------------------------------------
		%%Function: SlbgeCalcCustom
		%%Qualified: bg._bg.SlbgeCalcCustom
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	SortedList SlbgeCalcCustom(SortedList slbgeValues)
	{
		SortedList slbge = new SortedList();
		DateTime dttmFirst = DateTime.Parse(m_ebFirst.Text);
		DateTime dttmLast = DateTime.Parse(m_ebLast.Text).AddDays(1);

		foreach (BGE bge in slbgeValues.Values)
			{
			if (bge.Type == BGE.ReadingType.Control)
				continue;

			if (bge.Date < dttmFirst)
				continue;

			if (bge.Date >= dttmLast)
				continue;

			switch (bge.Type)
				{
				case BGE.ReadingType.Snack:
					if (m_cbSnack.Checked != true)
						continue;
					break;
				case BGE.ReadingType.Lunch:
					if (m_cbLunch.Checked != true)
						continue;
					break;
				case BGE.ReadingType.Dinner:
					if (m_cbDinner.Checked != true)
						continue;
					break;
				case BGE.ReadingType.Breakfast:
					if (m_cbBreakfast.Checked != true)
						continue;
					break;
				case BGE.ReadingType.SpotTest:
					if (m_cbSpot.Checked != true)
						continue;
					break;
				}
			slbge.Add(bge.Key, bge);
			}
		return slbge;
	}

	/* D O  G R A P H */
	/*----------------------------------------------------------------------------
		%%Function: DoGraph
		%%Qualified: bg._bg.DoGraph
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	private void DoGraph(object sender, System.EventArgs e) 
	{
		SortedList slbge;

		if (m_cbxFilterType.Text == "Fasting")
            slbge = SlbgeCalcFasting(m_cbShowInterp.Checked ? m_slbgeStats : m_slbge);
		else
			slbge = SlbgeCalcCustom(m_cbShowInterp.Checked ? m_slbgeStats : m_slbge);

		BgGraph bgg = new BgGraph();
		BgGraph.BoxView bvUpper, bvLower;

		bvUpper = BgGraph.BvFromString(m_cbxUpper.Text);
		bvLower = BgGraph.BvFromString(m_cbxLower.Text);

		bgg.SetGraphicViews(bvUpper, bvLower);
		SetGraphBounds(bgg);
		bgg.SetDataPoints(slbge);
		bgg.CalcGraph();
		bgg.ShowDialog();
	}

	/* S E T  G R A P H  B O U N D S */
	/*----------------------------------------------------------------------------
		%%Function: SetGraphBounds
		%%Qualified: bg._bg.SetGraphBounds
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	void SetGraphBounds(BgGraph bgg)
	{
		int nDays = 7;
		if (m_ebDays.Text.Length > 0)
			nDays = Int32.Parse(m_ebDays.Text);

		int nIntervals = 19;
		if (m_ebIntervals.Text.Length > 0)
			nIntervals = Int32.Parse(m_ebIntervals.Text);

		int nLow = 30;
		if (m_ebLow.Text.Length > 0)
			nLow = Int32.Parse(m_ebLow.Text);

		int nHigh = 30;
		if (m_ebHigh.Text.Length > 0)
			nHigh = Int32.Parse(m_ebHigh.Text);

		bool fLandscape = true;

		if (m_cbxOrient.Text == "Portrait")
			fLandscape = false;

		bgg.SetBounds(nLow, nHigh, nDays, nIntervals, m_cbShowMeals.Checked, fLandscape);
	}

	/* S L B G E  C A L C  F A S T I N G */
	/*----------------------------------------------------------------------------
		%%Function: SlbgeCalcFasting
		%%Qualified: bg._bg.SlbgeCalcFasting
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	SortedList SlbgeCalcFasting(SortedList slbgeValues)
	{
		DateTime dttmFirst = DateTime.Parse(m_ebFirst.Text);
		DateTime dttmLast = DateTime.Parse(m_ebLast.Text).AddDays(1);
		SortedList slbge = new SortedList();

		int nFastLength = 8;

		if (m_ebFastLength.Text.Length > 0)
			{
			nFastLength = Int32.Parse(m_ebFastLength.Text);
			}

		DateTime dttmNextFast = DateTime.Parse("1/1/1900 12:00 AM");

		foreach (BGE bge in slbgeValues.Values)
			{
			if (bge.Type == BGE.ReadingType.Control)
				continue;

			// see if this one is a fasting
			if (bge.Date > dttmNextFast)
				{
				if (bge.Reading != 0)
					{
					if (bge.Date >= dttmFirst && bge.Date < dttmLast)
						slbge.Add(bge.Key, bge);
					}
				}

			// now see if this one should reset the fasting counter
			if (bge.Type != BGE.ReadingType.SpotTest)
				{
				dttmNextFast = bge.Date.AddHours((double)nFastLength);
				}
			}
		return slbge;
	}

	/* R E N D E R  W I T H  L I N E */
	/*----------------------------------------------------------------------------
		%%Function: RenderWithLine
		%%Qualified: bg._bg.RenderWithLine
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	private void RenderWithLine(object sender, System.Windows.Forms.PaintEventArgs e)
	{
		Label lbl = (Label)sender;
		Font hFont = lbl.Font; // new Font("Tahoma", 8);
		SolidBrush hBrush = new SolidBrush(lbl.ForeColor);
		Pen pen = new Pen(Color.DarkGray, (float)1.0);
		Pen pen2 = new Pen(Color.LightGray, (float)1.0);

		e.Graphics.Clear(lbl.BackColor);
		e.Graphics.DrawString(lbl.Text, hFont, hBrush, 0, 0);
		SizeF szf =  e.Graphics.MeasureString(lbl.Text, hFont);

		e.Graphics.DrawLine(pen, szf.Width + (float)2.0, (szf.Height) / 2, lbl.Width, (szf.Height) / 2);
		e.Graphics.DrawLine(pen2, szf.Width + (float)1.0, (szf.Height) / 2 - (float)1.0, lbl.Width - (float)1.0, (szf.Height) / 2 - (float)1.0);

	}

	private bool m_fDirtyStats = true;

	/* U P D A T E  C A R B  L I S T */
	/*----------------------------------------------------------------------------
		%%Function: UpdateCarbList
		%%Qualified: bg._bg.UpdateCarbList
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	private int UpdateCarbList(BGE bgeCur, ref SortedList slbge)
	{
		// retire all the items at the beginning of the list
		while (slbge.Count > 0)
			{
			BGE bgeFirst = (BGE)slbge.GetByIndex(0);

			if (bgeFirst.Date.AddHours(4.0) > bgeCur.Date)
				break; // nothing left to retire

			slbge.RemoveAt(0);
			}

		// now, if bgeCur has carbs, then add it to the list
		if (bgeCur.Carbs > 0)
			slbge.Add(bgeCur.Key, bgeCur);

		int nCarbs = 0;

		for (int i = 0, iLast = slbge.Count; i < iLast; i++)
			{
			BGE bge = (BGE) slbge.GetByIndex(i);

			if (bge.Date != bgeCur.Date)
				nCarbs += bge.Carbs;
			}

		return nCarbs;
	}


	class STN
	{
		public string sDesc;
		public int nTotal;
		public int cTotal;

		public int nFastTotal;
		public int cFastTotal;

		public Int64 nWgtTotal;
		public Int64 cWgtTotal;

		public DateTime dttmCutoff;
		public DateTime dttmCutoffLast;

		public STN(string sDescIn, DateTime dttmCutoffIn, DateTime dttmCutoffLastIn)
		{
			sDesc = sDescIn;
			nTotal = cTotal = nFastTotal = cFastTotal = 0;
			nWgtTotal = cWgtTotal = 0;

			dttmCutoff = dttmCutoffIn;
			dttmCutoffLast = dttmCutoffLastIn;
		}

		public bool FMatch(BGE bge)
		{
			if (bge.Date >= dttmCutoff && bge.Date <= dttmCutoffLast)
				return true;

			return false;
		}

		public float A1c
		{
			get
			{
				int nAvg = (int)(nWgtTotal / cWgtTotal);

				float dA1c = (nAvg + 77.3f) / 35.6f;
//				float dA1c = ((nAvg / 1.12f) + 86f) / 33.3f;
				return dA1c;
			}
		}

		public int Avg { get { return  nTotal / cTotal; } }
		public int FastAvg { get { return nFastTotal / cFastTotal; } }
		public int WgtAvg { get { return (int)(nWgtTotal / cWgtTotal); } }

		/* S E T  T E X T */
		/*----------------------------------------------------------------------------
			%%Function: SetText
			%%Qualified: bg._bg:STN.SetText
			
			%%Contact: rlittle

			
		----------------------------------------------------------------------------*/
		public void SetText(ListView lv)
		{
			ListViewItem lvi = new ListViewItem();

			lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
			lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
			lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
			lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
			lvi.SubItems.Add(new ListViewItem.ListViewSubItem());

			lv.Items.Add(lvi);
			lvi.SubItems[4].Text = A1c.ToString("F");
			lvi.SubItems[3].Text = WgtAvg.ToString();
			lvi.SubItems[2].Text = FastAvg.ToString();
			lvi.SubItems[1].Text = Avg.ToString();
			lvi.SubItems[0].Text = sDesc;

//			ebAvg.Text = Avg.ToString();
//			ebFast.Text = FastAvg.ToString();
//			ebWgt.Text = WgtAvg.ToString();
//			ebA1c.Text = A1c.ToString();
		}

	};


	SortedList m_slbgeStats;
	struct II // Interpolated Item
	{
		public Int64 nEst;
		public Int64 cEst;
	};

	/* C L E A R  I N T E R P O L A T E D  E N T R I E S */
	/*----------------------------------------------------------------------------
		%%Function: ClearInterpolatedEntries
		%%Qualified: bg._bg.ClearInterpolatedEntries
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	void ClearInterpolatedEntries(SortedList slbge)
	{
		int i;

		for (i = slbge.Count - 1; i >= 0; i--)
			{
			BGE bge = (BGE)m_slbge.GetByIndex(i);

			if (bge.InterpReading)
				m_slbge.RemoveAt(i);
			}
	}

	/* A D D  I N T E R P O L A T E D  B G E */
	/*----------------------------------------------------------------------------
		%%Function: AddInterpolatedBge
		%%Qualified: bg._bg.AddInterpolatedBge
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	void AddInterpolatedBge(SortedList slbge, DateTime dttmEst, int nEst, string sComment)
	{
		BGE bgeEst = new BGE(dttmEst.ToString("d"), dttmEst.ToString("T"), BGE.ReadingType.SpotTest, (int)nEst, 0, "interpolated: " + sComment, "");
		int cTries = 0;
		bgeEst.InterpReading = true;

		while (cTries < 4) // we allow attempts to miss a duplicate item
			{
			try
				{
				slbge.Add(bgeEst.Key, bgeEst);
//				if (bgeEst.Date.Month == 6)
//					Debug.WriteLine(String.Format("   AddInterp {0}", bgeEst.ToString()));
				return;	// we succeeded!
				}
			catch
				{
				dttmEst = dttmEst.AddMinutes(1.0);
				bgeEst = new BGE(dttmEst.ToString("d"), dttmEst.ToString("T"), BGE.ReadingType.SpotTest, (int)nEst, 0, "interpolated: " + sComment, "");
				bgeEst.InterpReading = true;
				}
			cTries++;
			}

		throw(new Exception("Too many BGE conflicts on interp add"));
	}


	/* B G E  P R E V  R E A D I N G */
	/*----------------------------------------------------------------------------
		%%Function: BgePrevReading
		%%Qualified: bg._bg.BgePrevReading
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	BGE BgePrevReading(SortedList slbge, int iCur)
	{
		// walk backwards finding the first item with a reading that isn't a
		// control
		while (--iCur >= 0)
			{
			BGE bge = (BGE)slbge.GetByIndex(iCur);
			if (bge.Reading != 0 && bge.Type != BGE.ReadingType.Control)
				return bge;
			}
		return new BGE("1/1/1900", "00:00", BGE.ReadingType.Control, 0, 0, "null reading", "");
	}

	BGE BgePrevEntry(SortedList slbge, int iCur)
	{
		// walk backwards finding the first item with a reading that isn't a
		// control
		while (--iCur >= 0)
			{
			BGE bge = (BGE)slbge.GetByIndex(iCur);
			if (bge.Type != BGE.ReadingType.Control)
				return bge;
			}
		return new BGE("1/1/1900", "00:00", BGE.ReadingType.Control, 0, 0, "null reading", "");
	}

	/* C R E A T E  I N T E R P O L A T I O N S */
	/*----------------------------------------------------------------------------
		%%Function: CreateInterpolations
		%%Qualified: bg._bg.CreateInterpolations
		%%Contact: rlittle

		when we calculate weighted averages, we'd like to do it by just walking
		the list of readings and figuring out the weightings based on that list.
		it won't be that simple (since there are times when you weight forward
		and other times you weight backwards (e.g. around meals)), but we don't
		want to be in the business of creating interpolations at the same time.

		to that end, let's create the interpolations now.
	----------------------------------------------------------------------------*/
	SortedList SlbgeCreateInterpolations(SortedList slbge)
	{
		SortedList slbgeStats = (SortedList) slbge.Clone(); // new SortedList();

		int i;

		// note that we check against the real count every time since we are
		// adding items.

		// TAKE CARE not to add items before the current item without carefully
		// accounting for it with i
		i = 0;
		while (i < slbgeStats.Count)
			{
			BGE bge = (BGE)slbgeStats.GetByIndex(i);
			BGE bgePrevReading = BgePrevReading(slbgeStats, i);
			BGE bgePrevEntry = BgePrevEntry(slbgeStats, i);

//			if (bge.Date.Month == 6)
//				Debug.WriteLine(String.Format("Interp {0}", bge.ToString()));

			if (bge.Type == BGE.ReadingType.Control)
				{
				i++;
				continue;
				}

//			if (bge.Date.Month == 6 && bge.Date.Day == 30 && bge.Date.Hour > 22)
//				Debug.WriteLine("here!");

			if (bgePrevEntry.Carbs > 0)
				{
				TimeSpan tsCarb = bge.Date.Subtract(bgePrevEntry.Date);

				// if the reading since the carb were > 1.5 hours, we can 
				// assume we didn't capture the spike.  use a guess of 10 bgc rise
				// for each carb.  we will put this spike at 90 minutes postprandial
				// (this is one of many tunable places)

				if (tsCarb.TotalMinutes > 90)
					{
					// we are now saying that the last reading we have is 90 minutes
					// postprandial (the one we are estimating)

					// calculate an estimate as a 30bgc per carb rise
					int nEst = bgePrevReading.Reading + bgePrevEntry.Carbs * 15;// * 30;
					AddInterpolatedBge(slbgeStats, bgePrevEntry.Date.AddMinutes(90.0), nEst, "PostPrandial");

					// now, do we have to estimate that 4 hours post prandial we go back to 
					// what we were pre-meal?
					if (tsCarb.TotalMinutes > 240)
						{
						// no reading for 4 hours past the meal -- assume we go
						// back to the pre-reading
						AddInterpolatedBge(slbgeStats, bgePrevEntry.Date.AddMinutes(240.0), bgePrevReading.Reading, "PostPostPrandial");
						}

					// now we want to just continue and reevaluate starting from the previous
					// entry (essentially recursing)
					i = slbgeStats.IndexOfKey(bgePrevReading.Key);
					if (i < 0)
						throw(new Exception("bgePrevReading doesn't have a key but it must!"));

					continue;
					}
				}

			// if this entry has a reading, let's decide if we can weight the entire period of time from the last entry
			// to now, or if we need to interpolate some entries.
			if (bge.Reading != 0)
				{
				// if we have a previous reading...
				if (bgePrevReading.Reading != 0)//  && bge.Date < dttmStop)
					{
					TimeSpan ts = bge.Date.Subtract(bgePrevReading.Date);

					// if the current reading is higher than the previous, assume its
					// .75 of the delta, for been that way for 1/2 of the time
					if (bge.Reading > bgePrevReading.Reading && ts.TotalMinutes > 10.0) // bgePrevReading.Reading)
						{
						int nEst = bgePrevReading.Reading + ((bge.Reading - bgePrevReading.Reading)* 1) / 2;
						AddInterpolatedBge(slbgeStats, bgePrevReading.Date.AddMinutes(ts.TotalMinutes * 0.75), nEst, "75% for 0.5");

						// now we want to just continue and reevaluate starting from the previous
						// entry (essentially recursing)
						i = slbgeStats.IndexOfKey(bgePrevReading.Key);
						if (i < 0)
							throw(new Exception("bgePrevReading doesn't have a key but it must!"));

						continue;
						}
					if (bge.Reading < bgePrevReading.Reading && ts.TotalMinutes > 20.0) // bgePrevReading.Reading)
						{
						int nEst = bgePrevReading.Reading + ((bge.Reading - bgePrevReading.Reading)* 1) / 2;
						AddInterpolatedBge(slbgeStats, bgePrevReading.Date.AddMinutes(ts.TotalMinutes * 0.50), nEst, "75% for 0.5");

						// now we want to just continue and reevaluate starting from the previous
						// entry (essentially recursing)
						i = slbgeStats.IndexOfKey(bgePrevReading.Key);
						if (i < 0)
							throw(new Exception("bgePrevReading doesn't have a key but it must!"));

						continue;
						}
					}
				}
			i++;
			}
		return slbgeStats;
	}

	/* C A L C  S T A T S */
	/*----------------------------------------------------------------------------
		%%Function: CalcStats
		%%Qualified: bg._bg.CalcStats
		%%Contact: rlittle

	----------------------------------------------------------------------------*/
	private void CalcStats()
	{
		BGE bgeLast = (BGE)m_slbge.GetByIndex(m_slbge.Values.Count - 1);

		m_slbgeStats = new SortedList();

		DateTime dttm90 = bgeLast.Date.AddDays(-90);
		DateTime dttm30 = bgeLast.Date.AddDays(-30);
		DateTime dttm15 = bgeLast.Date.AddDays(-15);
		DateTime dttm7 = bgeLast.Date.AddDays(-7);
		DateTime dttmNil = DateTime.Parse("12/31/2199");
		DateTime dttm2ndVisit = DateTime.Parse("9/1/2004");
		DateTime dttmLastVisit = DateTime.Parse("11/24/2004");
		DateTime dttmFirstVisit = dttm2ndVisit.Date.AddDays(-90);

		// switching to analysis.  If anything is dirty, then recalc
		// the stats
		STN []rgstn = new STN[] 
			{ new STN("Lifetime", DateTime.Parse("1/1/1900"), dttmNil),
			  new STN("Last 90 Days", dttm90, dttmNil),
			  new STN("Last 30 Days", dttm30, dttmNil),
			  new STN("Last 15 Days", dttm15, dttmNil),
			  new STN("Last 7 Days", dttm7, dttmNil),
			  new STN("9-1-04 A1c", dttmFirstVisit, dttm2ndVisit),
			  new STN("11-24-04 A1c", dttm2ndVisit, dttmLastVisit),
			};

		int nFastLength = 8;

		if (m_ebFastLength.Text.Length > 0)
			nFastLength = Int32.Parse(m_ebFastLength.Text);

		DateTime dttmNextFast = DateTime.Parse("1/1/1900 12:00 AM");
		DateTime dttmNextMealCheckLow = DateTime.Parse("1/1/1900 12:00 AM");
		DateTime dttmNextMealCheckHigh = DateTime.Parse("1/1/1900 12:00 AM");
		DateTime dttmLastWgt = DateTime.Parse("1/1/1900 12:00 AM");
		DateTime dttmStop = DateTime.Parse("9/9/2004");

		int nLastWgt = 0;
		int nLastReadingReal = 0;
		BGE bgePrev = null;

		Int64 nWgtCur = 0;
		Int64 cWgtCur = 0;


		// we are interesting in keeping track of the last 4 hours of carb
		// intake.
		SortedList slbgeCarbs = new SortedList();
		DateTime dttmLastCarb = new DateTime(1900, 1, 1);

		Int64 nWgtDay = 0;
		Int64 cWgtDay = 0;

//		ClearInterpolatedValues(m_slbge);

		m_slbgeStats = SlbgeCreateInterpolations(m_slbge);

		foreach (BGE bgeStats in m_slbgeStats.Values)
			{
			BGE bgeReal = null;

			if (!bgeStats.InterpReading)
				bgeReal = (BGE)m_slbge.GetByIndex(m_slbge.IndexOfKey(bgeStats.Key));

			if (bgePrev != null && bgeStats.Date.Day != bgePrev.Date.Day)
				nWgtDay = cWgtDay = 0;

//			if (bge.Date.Day == 9 && bge.Date.Month == 8 )
//				Debugger.Break();

			if (bgeStats.Type == BGE.ReadingType.Control)
				continue;

			int nCarbs = UpdateCarbList(bgeStats, ref slbgeCarbs);
			
			if (dttmLastCarb.Year != 1900)
				{
				bgeStats.MinutesSinceCarbs = (int)((bgeStats.Date.Ticks - dttmLastCarb.Ticks) / (36000000000 / 60)); // number of hours
				}
			else
				bgeStats.MinutesSinceCarbs = -1;

			bgeStats.CarbsIn4 = nCarbs;

			// if this entry has a reading, let's decide if we can weight the entire period of time from the last entry
			// to now, or if we need to interpolate some netries.
			if (bgeStats.Reading != 0)
				{
				nWgtCur = cWgtCur = 0;

				// if we have a previous reading...
				if (nLastWgt != 0)//  && bge.Date < dttmStop)
					{
					TimeSpan ts = bgeStats.Date.Subtract(dttmLastWgt);

					Int64 nHours = ((Int64)(ts.TotalMinutes * 100) / 60);

					if (bgePrev.Carbs > 0)
						{
						// assume that a greater reading immediately following a "carbs" entry
						// will be higher previous to this reading
						if (bgeStats.Reading > nLastWgt)
							{
							// don't let the previous reading (which was preprandial) get weighted
							// as such; treat the larger reading (postprandial) as the average)
							nLastWgt = bgeStats.Reading;
							}
						}

					nWgtCur = ((Int64)nLastWgt) * nHours;
					cWgtCur = nHours;

					nWgtDay += nWgtCur;
					cWgtDay += cWgtCur;

					bgeStats.WgtAvg = (int)(nWgtDay / cWgtDay);
					}
				nLastWgt = nLastReadingReal = bgeStats.Reading;
				dttmLastWgt = bgeStats.Date;

				int nFast = 0;
				int cFast = 0;

				// see if this one is a fasting
				if (bgeStats.Date > dttmNextFast)
					{
					nFast = bgeStats.Reading;
					cFast = 1;
					}


				foreach (STN stn in rgstn)
					{
					if (stn.FMatch(bgeStats))
						{
						stn.nTotal += bgeStats.Reading;
						stn.cTotal++;
						stn.nFastTotal += nFast;
						stn.cFastTotal += cFast;
						stn.nWgtTotal += nWgtCur;
						stn.cWgtTotal += cWgtCur;
						}
					}
				}

			bgePrev = bgeStats;

			// now see if this one should reset the fasting counter
			if (bgeStats.Type != BGE.ReadingType.SpotTest)
				{
				dttmNextFast = bgeStats.Date.AddHours((double)nFastLength);
				}
			if (bgeStats.Carbs > 0)
				dttmLastCarb = bgeStats.Date;

			if (bgeReal != null)
				{
				// propagate all the values that we calced from bgeStats to bgeReal
				bgeReal.MinutesSinceCarbs = bgeStats.MinutesSinceCarbs;
				bgeReal.CarbsIn4 = bgeStats.CarbsIn4;
				bgeReal.WgtAvg = bgeStats.WgtAvg;
				}
			}

		m_lvStats.Clear();
		SetupListViewStats(m_lvStats);
		foreach (STN stn in rgstn)
			{
			stn.SetText(m_lvStats);
			}

		m_fDirtyStats = false;
	}

	/* C H A N G E  T A B S */
	/*----------------------------------------------------------------------------
		%%Function: ChangeTabs
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	private void ChangeTabs(object sender, System.EventArgs e) 
	{
		TabControl tabc = (TabControl)sender;

		if (tabc.SelectedIndex == 1 && m_fDirtyStats)
			{
			CalcStats();
			}
	}

	/* S E T U P  7  D A Y  G R A P H */
	/*----------------------------------------------------------------------------
		%%Function: Setup7DayGraph
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	private void Setup7DayGraph(object sender, System.EventArgs e)
	{
		BGE bgeLast = (BGE)m_slbge.GetByIndex(m_slbge.Values.Count - 1);

		m_ebLast.Text = bgeLast.Date.ToString("d");
		m_ebFirst.Text = bgeLast.Date.AddDays(-7).ToString("d");

		m_ebFirst.Enabled = false;
		m_ebLast.Enabled = false;
	}

	/* S E T U P  1  5  D A Y  G R A P H */
	/*----------------------------------------------------------------------------
		%%Function: Setup15DayGraph
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	private void Setup15DayGraph(object sender, System.EventArgs e)
	{
		BGE bgeLast = (BGE)m_slbge.GetByIndex(m_slbge.Values.Count - 1);
	
		m_ebLast.Text = bgeLast.Date.ToString("d");
		m_ebFirst.Text = bgeLast.Date.AddDays(-15).ToString("d");
	
		m_ebFirst.Enabled = false;
		m_ebLast.Enabled = false;
	}

	/* S E T U P  3  0  D A Y  G R A P H */
	/*----------------------------------------------------------------------------
		%%Function: Setup30DayGraph
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	private void Setup30DayGraph(object sender, System.EventArgs e)
	{
		BGE bgeLast = (BGE)m_slbge.GetByIndex(m_slbge.Values.Count - 1);
	
		m_ebLast.Text = bgeLast.Date.ToString("d");
		m_ebFirst.Text = bgeLast.Date.AddDays(-30).ToString("d");
	
		m_ebFirst.Enabled = false;
		m_ebLast.Enabled = false;
	}

	/* E N A B L E  C U S T O M  D A T E S */
	/*----------------------------------------------------------------------------
		%%Function: EnableCustomDates
		%%Qualified: bg._bg.EnableCustomDates
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	private void EnableCustomDates(object sender, System.EventArgs e) 
	{
		if (m_ebLast.Text.Length <= 0)
			{
			BGE bge = (BGE)m_slbge.GetByIndex(m_slbge.Values.Count - 1);
			m_ebLast.Text = bge.Date.ToString("d");
			}

		if (m_ebFirst.Text.Length <= 0)
			{
			BGE bge = (BGE)m_slbge.GetByIndex(0);
			m_ebFirst.Text = bge.Date.ToString("d");
			}
		m_ebFirst.Enabled = true;
		m_ebLast.Enabled = true;
	}

	/* S E L E C T  D A T E  R A N G E */
	/*----------------------------------------------------------------------------
		%%Function: SelectDateRange
		%%Qualified: bg._bg.SelectDateRange
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	private void SelectDateRange(object sender, System.EventArgs e) 
	{
		ComboBox cbx = (ComboBox)sender;
		
		if (cbx.Text == "7 Days")
			Setup7DayGraph(sender, e);
		else if (cbx.Text == "15 Days")
			Setup15DayGraph(sender, e);
		else if (cbx.Text == "30 Days")
			Setup30DayGraph(sender, e);
		else
			EnableCustomDates(sender, e);
	}

	public class DevComm : CommBase
	{
		CommBaseSettings m_cbs;

		/* C O M M  S E T T I N G S */
		/*----------------------------------------------------------------------------
			%%Function: CommSettings
			%%Qualified: bg._bg:DevComm.CommSettings
			%%Contact: rlittle

			
		----------------------------------------------------------------------------*/
		protected override CommBaseSettings CommSettings()
		{
			return m_cbs;
		}

		/* I N I T */
		/*----------------------------------------------------------------------------
			%%Function: Init
			%%Qualified: bg._bg:DevComm.Init
			%%Contact: rlittle

			
		----------------------------------------------------------------------------*/
		public void Init()
		{
			m_cbs = new CommBaseSettings();

			m_cbs.SetStandard("COM5:", 9600, CommBase.Handshake.none);
			rgbRxBuffer = new byte[512];
		}

		/* S E N D  C O M M A N D */
		/*----------------------------------------------------------------------------
			%%Function: SendCommand
			%%Qualified: bg._bg:DevComm.SendCommand
			%%Contact: rlittle

			
		----------------------------------------------------------------------------*/
		void SendCommand(string s)
		{
			byte []rgb = new byte[Encoding.ASCII.GetByteCount(s)];
			byte []rgbString = Encoding.ASCII.GetBytes(s);

			Buffer.BlockCopy(rgbString, 0, rgb, 0, rgbString.Length);
			
			int iMac = rgbString.Length;
			int i;

			for (i = 0; i < iMac; i++)
				{
				Send(rgb[i]);
				Sleep(150);
				}
//			Send(rgb);
		}

		private byte[] rgbRxBuffer;
		private uint ibRxBuffer = 0;
		private ASCII[] rgbRxTerm = { ASCII.CR, ASCII.LF };
//		private ASCII[] TxTerm;
//		private ASCII[] RxFilter;
		private string RxString = "";
		private ManualResetEvent TransFlag = new ManualResetEvent(true);

//		private uint TransTimeout;
		int ibRxTermWaiting = 0;
		ArrayList pls = new ArrayList();

		/* O N  R X  L I N E */
		/*----------------------------------------------------------------------------
			%%Function: OnRxLine
			%%Qualified: bg._bg:DevComm.OnRxLine
			%%Contact: rlittle

			
		----------------------------------------------------------------------------*/
		void OnRxLine(string s)
		{
			lock(pls)
				{
				pls.Add(s);
				}
		}

		/* O N  R X  C H A R */
		/*----------------------------------------------------------------------------
			%%Function: OnRxChar
			%%Qualified: bg._bg:DevComm.OnRxChar
			%%Contact: rlittle

			
		----------------------------------------------------------------------------*/
		protected override void OnRxChar(byte ch) 
		{
			ASCII ca = (ASCII)ch;
			if (ibRxTermWaiting < rgbRxTerm.Length)
				{
				if (ca == rgbRxTerm[ibRxTermWaiting])
					ibRxTermWaiting++;
				else
					ibRxTermWaiting = 0;	// bail out if we didn't get the next ASCII char in the sequence
				}

			if (ibRxTermWaiting >= rgbRxTerm.Length
				|| (ibRxBuffer > rgbRxBuffer.GetUpperBound(0)))
				{
				//JH 1.1: Use static encoder for efficiency. Thanks to Prof. Dr. Peter Jesorsky!
				lock(RxString) 
					{
					RxString = Encoding.ASCII.GetString(rgbRxBuffer, 0, (int)ibRxBuffer - ibRxTermWaiting);
					}
				ibRxBuffer = 0;
				ibRxTermWaiting = 0;
				if (TransFlag.WaitOne(0,false)) 
					{
//					ThrowException("Received line when noone was looking!");
					OnRxLine(RxString);
					}
				else 
					{
					TransFlag.Set();
					}
				}
			else
				{
				bool wr = true;
//				if (RxFilter != null) 
//					{
//					for (int i=0; i <= RxFilter.GetUpperBound(0); i++) 
//						if (RxFilter[i] == ca) 
//							wr = false;
//					}

				if (wr)
					{
					rgbRxBuffer[ibRxBuffer] = ch;
					ibRxBuffer++;
					}
				}
		}

		/* G E T  R E A D I N G S */
		/*----------------------------------------------------------------------------
			%%Function: GetReadings
			%%Qualified: bg._bg:DevComm.GetReadings
			%%Contact: rlittle

			
		----------------------------------------------------------------------------*/
		public ArrayList GetReadings()
		{
			// actually read the data
			lock(pls)
				{
				pls.Clear();
				}

			TransFlag.Reset();
			SendCommand("DMP");
			// the first line we get back will tell us how many lines to expect
			if (!TransFlag.WaitOne((int)5000, false))
				ThrowException("Timeout");

			// more lines are coming in, they are being collected in pls
			string s;
			lock(RxString)
				{
				s = RxString;
				}

			// find the leading "P"
			int ibp = s.IndexOf('P');
			if (ibp < 0)
				return null;

			int cLines = Int32.Parse(s.Substring(ibp + 2, 3));

			// and now we wait for that many lines to show up in pls
			int cMsecsTimeout = 20000;
			ArrayList plsFinal = null;
			int count = 0;

			while (cMsecsTimeout > 0 && plsFinal == null)
				{
				lock (pls)
					{
					if (pls.Count >= cLines)
						plsFinal = pls;
					count = pls.Count;
					}
				Sleep(2000);
				cMsecsTimeout -= 2000;
				}

			if (plsFinal == null)
				{
				MessageBox.Show("Only got " + count.ToString()+" readings, expected " + cLines.ToString());
				return null;
				}

			return plsFinal;
		}

		/* C H E C K  D E V I C E */
		/*----------------------------------------------------------------------------
			%%Function: CheckDevice
			%%Qualified: bg._bg:DevComm.CheckDevice
			%%Contact: rlittle

			
		----------------------------------------------------------------------------*/
		public bool CheckDevice()
		{
			SendCommand("DM");
			Sleep(500);
			TransFlag.Reset();
			SendCommand("DM?");

			// expecting one line back
			if (!TransFlag.WaitOne((int)500, false))
				ThrowException("Timeout");
			string s;
			lock(RxString)
				{
				s = RxString;
				}
			if (s.Substring(0,1) != "?")
				return false;

			return true;
		}

		/* C L O S E  D E V I C E */
		/*----------------------------------------------------------------------------
			%%Function: CloseDevice
			%%Qualified: bg._bg:DevComm.CloseDevice
			%%Contact: rlittle

			
		----------------------------------------------------------------------------*/
		public void CloseDevice()
		{
			this.Close();
		}
	}

	/* G E T  R E A D I N G S  F R O M  D E V I C E */
	/*----------------------------------------------------------------------------
		%%Function: GetReadingsFromDevice
		%%Qualified: bg._bg.GetReadingsFromDevice
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	ArrayList GetReadingsFromDevice()
	{
		ArrayList pls = new ArrayList();

		StreamReader sr = new StreamReader("c:\\temp\\dmp1.txt");
		string s;

		s = sr.ReadLine();	// consume the first one
		while ((s = sr.ReadLine()) != null)
			pls.Add(s);

		sr.Close();
		return pls;
	}

	/* S  G E T  N E X T  Q U O T E D  F I E L D */
	/*----------------------------------------------------------------------------
		%%Function: SGetNextQuotedField
		%%Qualified: bg._bg.SGetNextQuotedField
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	string SGetNextQuotedField(string s, int iFirst, out int iNext)
	{
		int ib;
		iNext = 0;
		ib = s.IndexOf('"', iFirst);
		if (ib < iFirst)
			return null;

		int ib2 = s.IndexOf('"', ib + 1);
		if (ib2 < ib)
			return null;

		string sRet = s.Substring(ib + 1, ib2 - ib - 1);
		if (ib2 + 1 >= s.Length)
			iNext = -1;
		else
			iNext = ib2 + 1;

		return sRet;
	}

	/* R E A D  F R O M  D E V I C E */
	/*----------------------------------------------------------------------------
		%%Function: ReadFromDevice
		%%Qualified: bg._bg.ReadFromDevice
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	private void ReadFromDevice(object sender, System.EventArgs e) 
	{
		ArrayList pls = GetReadingsFromDeviceReal();

		foreach (string s in pls)
			{
			int iFirst = 0, iNext;

			if (s.Substring(0, 2) != "P ")
				continue;

			string sDay = SGetNextQuotedField(s, iFirst, out iNext);
			string sDate = SGetNextQuotedField(s, iFirst = iNext, out iNext);
			string sTime = SGetNextQuotedField(s, iFirst = iNext, out iNext);
			string sReading = SGetNextQuotedField(s, iFirst = iNext, out iNext);

			if (sReading.Substring(0,1) == "C")
				continue;

			BGE bge = new BGE(sDate, sTime, BGE.ReadingType.New, Int32.Parse(sReading), 0, "", "");

			AddEntryCore(bge, true);
			}
	}

	/* G E T  R E A D I N G S  F R O M  D E V I C E  R E A L */
	/*----------------------------------------------------------------------------
		%%Function: GetReadingsFromDeviceReal
		%%Qualified: bg._bg.GetReadingsFromDeviceReal
		%%Contact: rlittle

		
	----------------------------------------------------------------------------*/
	private ArrayList GetReadingsFromDeviceReal() 
	{
		DevComm devComm = new DevComm();

		devComm.Init();
		CommBase.PortStatus ps = devComm.IsPortAvailable("COM5:");

		if (ps != CommBase.PortStatus.available)
			{
			MessageBox.Show("COM1 unavailable");
			return null;
			}

		if (!devComm.Open())
			{
			MessageBox.Show("COM1 unable to open");
			return null;
			}

		if (!devComm.CheckDevice())
			{
			MessageBox.Show("OTU did not respond appropriately to DM?");
			return null;
			}

		ArrayList pls = devComm.GetReadings();

		if (pls == null)
			{
			MessageBox.Show("OTU did not provide readings");
			return null;
			}

		devComm.CloseDevice();
		return pls;
	}

	void AddBgeToSearch(BGE bge)
	{
		ListViewItem lvi = new ListViewItem();

		lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
		lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
		lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
		lvi.SubItems.Add(new ListViewItem.ListViewSubItem());

		m_lvMeals.Items.Add(lvi);

		lvi.SubItems[3].Text = bge.FullComment;
		if (bge.Carbs != 0)
			lvi.SubItems[2].Text = bge.Carbs.ToString();
		lvi.SubItems[1].Text = bge.Date.ToString();
		lvi.SubItems[0].Text = m_idGroupCur.ToString();
		lvi.Tag = bge;
	}

	int m_idGroupCur = 0;

	private void DoMealSearch(object sender, System.EventArgs e)
	{
		// walk through all our entries and find matches
		Regex rx = null;
		int c = 0;
		m_idGroupCur++;
		if (m_cbRegEx.Checked)
			{
			try
				{
                rx = new Regex(m_cbxSearch.Text);
                }
			catch (Exception exc)
				{
				MessageBox.Show("Could not compile Regular Expression '" + m_cbxSearch.Text + "':\n"+ exc.ToString(), "BG");
				return;
				}
			}

		foreach (BGE bge in m_slbge.Values)
			{
			if ((rx != null && rx.IsMatch(bge.Meal))
				|| String.Compare(m_cbxSearch.Text, bge.Meal, true/*fIgnoreCase*/) == 0)
				{
				AddBgeToSearch(bge);
				c++;
				}
			}
		if (c == 0)
			{
			MessageBox.Show("The search yielded no results.", "BG");
			}
		else
			{
//			MessageBox.Show(c.ToString() + " item(s) returned and added to group 1.", "BG");
			}
	}

	SortedList SlbgeGetMealSubset(BGE bge)
	{
		int iMeal = m_slbge.IndexOfKey(bge.Key);
		int iFirst = iMeal;
		int iLast = iMeal + 1;

		while (iFirst >= 0)
			{
			BGE bgeT = (BGE)m_slbge.GetByIndex(iFirst);

			if (bgeT.Reading != 0 && bgeT.Type != BGE.ReadingType.Control)
				break;

			iFirst--;
			}

		if (iFirst == -1)
			return null;

		while (iLast < m_slbge.Count)
			{
			BGE bgeT = (BGE)m_slbge.GetByIndex(iLast);

			if (bgeT.Carbs > 0)
				break;
			iLast++;
			}

		if (iLast == m_slbge.Count)
			iLast--;

		SortedList slbge = new SortedList();

		for ( ; iFirst <= iLast; iFirst++)
			{
			BGE bgeT = (BGE)m_slbge.GetByIndex(iFirst);

			slbge.Add(bgeT.Key, bgeT.Clone());
			}

		return slbge;
	}

	private void GenerateMealChart(object sender, System.EventArgs e) 
	{
		ArrayList plmp = new ArrayList();

		// let's walk through the list of meals and build an array of
		// meal profiles for each group

		MP mp = new MP();
		int nGroup = -1;

		foreach (ListViewItem lvi in m_lvMeals.Items)
			{
			if (nGroup != -1 && nGroup != Int16.Parse(lvi.SubItems[0].Text))
				{
				// starting a new MP;
				mp.NormalizeMeals();
				plmp.Add(mp);
				mp = new MP();
				}
			nGroup = Int16.Parse(lvi.SubItems[0].Text);
			// build a subset of sorted entries for this meal, starting with
			// the first reading entry before (or at) the meal, and ending
			// with the entry immediately previous to the next meal.

			BGE bge = (BGE)lvi.Tag;
			SortedList slbge = SlbgeGetMealSubset(bge);

			mp.AddMeal(slbge, m_slbge, bge);
			}

		if (mp.NGetSampleSize() > 0)
			{
			mp.NormalizeMeals();
			plmp.Add(mp);
			}

		// at this point, plmp is our collection of meals...chart'em

		BgGraph bgg = new BgGraph();

		bgg.SetGraphicViews(BgGraph.BoxView.Meal, BgGraph.BoxView.None);
		bool fLandscape = true;

		if (m_cbxOrient.Text == "Portrait")
			fLandscape = false;

		bgg.SetBounds(60, 220, 1, 8, false, fLandscape);
		bgg.SetDataPoints(plmp);
		bgg.CalcGraph();
		bgg.ShowDialog();

	}

	private void ClearSearch(object sender, System.EventArgs e)
	{
		m_lvMeals.Items.Clear();
	}

	private void FindSuspectReadings(object sender, System.EventArgs e) 
	{
		BGE bgeLast = null;

		foreach(BGE bge in m_slbge.Values)
			{
			if (bgeLast != null)
				{
				if (bge.Date.Subtract(bgeLast.Date).TotalMinutes < 2)
					{
					ListViewItem lvi = LviFindBge(bge);

					lvi.BackColor = Color.HotPink;
					}
				}
			bgeLast = bge;
			}
	}


}}
