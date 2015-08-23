using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace yasc
{
	/// <summary>
	/// Summary description for Hover.
	/// </summary>
	public class Hover : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox m_pbxTip;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private string m_sTip;
		private Font m_hFont;
		private Brush m_hBrushTip;
		private Brush m_hBrushFill;

		private void PaintTip(Object sender, PaintEventArgs e)
		{
			PictureBox pbx = (PictureBox)sender;
			Graphics gr = e.Graphics;

			gr.FillRectangle(m_hBrushFill, 0, 0, m_pbxTip.Width, pbx.Height);
			gr.DrawString(m_sTip, m_hFont, m_hBrushTip, 2, 2);
		}

		public void ShowTip(Point pt, CalItem ci)
		{
			// figure out how much room we need
			string s = ci.GetStartFormatted() + " - " + ci.GetEndFormatted();

			s += ":  " + ci.GetTitle();
			m_sTip = s;

			Graphics gr = m_pbxTip.CreateGraphics();

			SizeF szf = gr.MeasureString(s, m_hFont);
			Size sz = new Size((int)Math.Floor(szf.Width), (int)Math.Floor(szf.Height));

			this.SetBounds(pt.X - sz.Width / 2, pt.Y - sz.Height, sz.Width + 4, sz.Height + 4, BoundsSpecified.All);
			this.Show();
		}

		public Hover()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			m_hFont = new Font("Tahoma", 8);
			m_hBrushTip = new SolidBrush(SystemColors.InfoText);
			m_hBrushFill = new SolidBrush(SystemColors.Info);


			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.m_pbxTip = new System.Windows.Forms.PictureBox();
			this.SuspendLayout();
			// 
			// m_pbxTip
			// 
			this.m_pbxTip.Name = "m_pbxTip";
			this.m_pbxTip.Size = new System.Drawing.Size(312, 184);
			this.m_pbxTip.TabIndex = 0;
			this.m_pbxTip.TabStop = false;
			this.m_pbxTip.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintTip);
			// 
			// Hover
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(304, 184);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.m_pbxTip});
			this.Enabled = false;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "Hover";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Hover";
			this.TopMost = true;
			this.ResumeLayout(false);

		}
		#endregion
	}
}
