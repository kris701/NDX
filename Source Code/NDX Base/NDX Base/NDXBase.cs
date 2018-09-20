using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace NDX_Base
{
    public enum DragBarLocation { None, Left, Right, Center };
    public enum NDXShowHideDesign { None };
    public enum NDXDragStyle { Normal, Opacity };

    public struct DragbarElementID
    {
        public DragBarLocation DragbarPos;
        public int ID;
        public int Width;

        public DragbarElementID(DragBarLocation _DragbarPos, int _ID, int _Width)
        {
            DragbarPos = _DragbarPos;
            ID = _ID;
            Width = _Width;
        }
    }

    public class NDX
    {
        #region Variables

        private enum NDXState { Initialized, Uninitialized };
        private enum NDXDragDir { N, E, S, W, NE, ES, SW, WN };

        public NDXShowHideDesign ActivateDesign = NDXShowHideDesign.None;
        public NDXShowHideDesign DeactivateDesign = NDXShowHideDesign.None;

        private NDXState CurNDXState = NDXState.Uninitialized;

        public NDXDragStyle ParentFormDragStyle = NDXDragStyle.Normal;
        public double DragStyleOpacityFactor = 0.75;

        private Form ParentForm;
        public Panel DragPanel;
        private Button XButton;
        private Button _Button;
        private Button MaxButton;
        private Label TitleLabel;
        private Panel SizablePanelLeft;
        private Panel SizablePanelTop;
        private Panel SizablePanelRight;
        private Panel SizablePanelBottom;
        private Panel VerticalScrollPanel;
        private Panel VerticalScrollPanelInner;
        private Panel HorisontalScrollPanel;
        private Panel HorisontalScrollPanelInner;

        private bool LoadingNDX = false;
        public int ScrollWheenSteps { get; set; } = 10;
        public bool AutoScroll { get; set; } = true;
        public bool ShowVerticalScrollBar { get; set; } = true;
        public bool ShowHorisontalScrollBar { get; set; } = true;
        public Color VerticalScrollBarColor { get; set; } = Color.White;
        public Color VerticalScrollBarInnerColor { get; set; } = Color.Black;
        public Color HorisontalScrollBarColor { get; set; } = Color.White;
        public Color HorisontalScrollBarInnerColor { get; set; } = Color.Black;
        public int VerticalScrollBarWidth { get; set; } = 25;
        public int HorisontalScrollBarHeight { get; set; } = 25;
        public int ScrollBarInnerMargin { get; set; } = 2;
        public int ScrollBarMargin { get; set; } = 5;
        private Point FarPoint;
        private Point ScrollBarInnerCursorDownPos;
        private Point ScrollBarInnerCursorDownPosStart;
        private bool VScrollBarInnerMove = false;
        private bool HScrollBarInnerMove = false;
        private List<Point> OriginalControlPoints = new List<Point>();
        public int VerticalScrollBarMinHeight { get; set; } = 20;
        public int HorisontalScrollBarMinWidth { get; set; } = 20;

        private NDXDragDir SizableBordersDragDir;
        public bool Sizable { get; set; } = false;
        public bool ShowSizableBorders { get; set; } = true;
        public bool SizableBordersTopMost { get; set; } = true;
        public bool WaitTilSizeingDone { get; set; } = false;
        public int SizableBordersWidth { get; set; } = 5;
        public Color SizableBordersColor { get; set; } = Color.White;
        private Point SizableBordersCursorDownPos;
        private Point SizableBordersCursorDownPosStart;
        private Size SizableBordersCursorDownSize;
        private bool SizableBordersMove = false;

        public Color DragbarBackColor { get; set; } = Color.Black;
        public int DragbarHeight { get; set; } = 30;

        public int DragBarInnerMargin { get; set; } = 5;

        public bool HasXButton { get; set; } = true;
        public Color XButtonBackColor { get; set; } = Color.Red;
        public Color XButtonForeColor { get; set; } = Color.White;
        public int XButtonHeight { get; set; } = 20;
        public int XButtonWidth { get; set; } = 20;
        public DragBarLocation XButtonPos = DragBarLocation.Right;
        public int XButtonDockID { get; set; } = 0;
        public Font XButtonFont { get; set; } = new Font("Microsoft Sans Serif", 6.0f, FontStyle.Bold);

        public bool Has_Button { get; set; } = true;
        public Color _ButtonBackColor { get; set; } = Color.Gray;
        public Color _ButtonForeColor { get; set; } = Color.White;
        public int _ButtonHeight { get; set; } = 20;
        public int _ButtonWidth { get; set; } = 20;
        public DragBarLocation _ButtonPos = DragBarLocation.Right;
        public int _ButtonDockID { get; set; } = 2;
        public Font _ButtonFont { get; set; } = new Font("Microsoft Sans Serif", 6.0f, FontStyle.Bold);

        public bool HasMaxButton { get; set; } = true;
        public Color MaxButtonBackColor { get; set; } = Color.Gray;
        public Color MaxButtonForeColor { get; set; } = Color.White;
        public int MaxButtonHeight { get; set; } = 20;
        public int MaxButtonWidth { get; set; } = 20;
        public DragBarLocation MaxButtonPos = DragBarLocation.Right;
        public int MaxButtonDockID { get; set; } = 1;
        public Font MaxButtonFont { get; set; } = new Font("Microsoft Sans Serif", 6.0f, FontStyle.Bold);

        public bool DoubleClickToMaximize { get; set; } = true;

        public bool HasTitle { get; set; } = true;
        public Color TitleTextColor { get; set; } = Color.White;
        public DragBarLocation TitlePos = DragBarLocation.Left;
        public int TitleDockID { get; set; } = 0;
        public int TitleWidth { get; set; } = 150;
        public Font TitleTextFont { get; set; } = new Font("Microsoft Sans Serif", 10.0f, FontStyle.Bold);

        private bool HasBeenDeactivated = false;
        private Point DragPanelCursorDownPos;
        private bool DragPanelMove = false;

        #endregion

        public NDX(Form _ParentForm)
        {
            ParentForm = _ParentForm;
        }

        public void InitializeNDX()
        {
            if (CurNDXState == NDXState.Uninitialized)
            {
                LoadingNDX = true;
                ParentForm.FormBorderStyle = FormBorderStyle.None;

                if (Sizable)
                {
                    foreach (Control _SenderControl in ParentForm.Controls)
                        _SenderControl.Location = new Point(_SenderControl.Location.X + SizableBordersWidth, _SenderControl.Location.Y + DragbarHeight + SizableBordersWidth);

                    ParentForm.Height = ParentForm.Height + DragbarHeight + SizableBordersWidth * 2;
                    ParentForm.Width = ParentForm.Width + SizableBordersWidth * 2;

                    GlobalMouseHandler gmh = new GlobalMouseHandler();
                    gmh.TheMouseMoved += new MouseMovedEvent(Sizable_MouseMove);
                    Application.AddMessageFilter(gmh);

                    if (ShowSizableBorders && SizableBordersTopMost)
                        CreateSizableBorders();
                }
                else
                {
                    foreach (Control _SenderControl in ParentForm.Controls)
                        _SenderControl.Location = new Point(_SenderControl.Location.X, _SenderControl.Location.Y + DragbarHeight);

                    ParentForm.Height = ParentForm.Height + DragbarHeight;
                    ParentForm.Width = ParentForm.Width + SizableBordersWidth * 2;
                }

                if (AutoScroll)
                {
                    ParentForm.AutoScroll = false;

                    VerticalScrollPanel = new Panel();
                    VerticalScrollPanel.BackColor = VerticalScrollBarColor;
                    VerticalScrollPanel.Width = VerticalScrollBarWidth;
                    VerticalScrollPanel.Tag = "NDX";

                    ParentForm.Controls.Add(VerticalScrollPanel);

                    GetFarPoint();

                    VerticalScrollPanelInner = new Panel();
                    VerticalScrollPanelInner.BackColor = VerticalScrollBarInnerColor;
                    VerticalScrollPanelInner.Width = VerticalScrollBarWidth - ScrollBarInnerMargin * 2;
                    VerticalScrollPanelInner.MouseDown += VScrollBarInner_MouseDown;
                    VerticalScrollPanelInner.MouseMove += ScrollBarInner_MouseMove;
                    VerticalScrollPanelInner.MouseUp += ScrollBarInner_MouseUp;
                    VerticalScrollPanelInner.Tag = "NDX";

                    VerticalScrollPanel.Controls.Add(VerticalScrollPanelInner);

                    HorisontalScrollPanel = new Panel();
                    HorisontalScrollPanel.BackColor = HorisontalScrollBarColor;
                    HorisontalScrollPanel.Height = HorisontalScrollBarHeight;
                    HorisontalScrollPanel.Tag = "NDX";

                    ParentForm.Controls.Add(HorisontalScrollPanel);

                    GetFarPoint();

                    HorisontalScrollPanelInner = new Panel();
                    HorisontalScrollPanelInner.BackColor = HorisontalScrollBarInnerColor;
                    HorisontalScrollPanelInner.Height = HorisontalScrollBarHeight - ScrollBarInnerMargin * 2;
                    HorisontalScrollPanelInner.MouseDown += HScrollBarInner_MouseDown;
                    HorisontalScrollPanelInner.MouseMove += ScrollBarInner_MouseMove;
                    HorisontalScrollPanelInner.MouseUp += ScrollBarInner_MouseUp;
                    HorisontalScrollPanelInner.Tag = "NDX";

                    HorisontalScrollPanel.Controls.Add(HorisontalScrollPanelInner);
                }

                DragPanel = new Panel();
                DragPanel.BackColor = DragbarBackColor;
                DragPanel.Location = new Point(0,0);
                DragPanel.Width = ParentForm.Width;
                DragPanel.Height = DragbarHeight;
                DragPanel.Name = "DragPanel";
                DragPanel.MouseDown += DragPanel_MouseDown;
                DragPanel.MouseMove += DragPanel_MouseMove;
                DragPanel.MouseUp += DragPanel_MouseUp;
                if (DoubleClickToMaximize)
                    DragPanel.DoubleClick += DragPanel_DoubleClick;
                DragPanel.Tag = "NDX";

                if (HasXButton)
                {
                    XButton = new Button();
                    XButton.BackColor = XButtonBackColor;
                    XButton.ForeColor = XButtonForeColor;
                    XButton.Height = XButtonHeight;
                    XButton.Width = XButtonWidth;
                    XButton.Font = XButtonFont;
                    XButton.FlatStyle = FlatStyle.Flat;
                    XButton.Text = "X";
                    XButton.TextAlign = ContentAlignment.MiddleCenter;
                    XButton.Name = "XButton";
                    XButton.Tag = new DragbarElementID(XButtonPos, XButtonDockID, XButtonWidth);
                    XButton.Location = GetControlPos(XButtonPos, XButtonDockID, XButtonWidth);
                    XButton.Click += CloseButton_Click;

                    DragPanel.Controls.Add(XButton);
                }

                if (Has_Button)
                {
                    _Button = new Button();
                    _Button.BackColor = _ButtonBackColor;
                    _Button.ForeColor = _ButtonForeColor;
                    _Button.Height = _ButtonHeight;
                    _Button.Width = _ButtonWidth;
                    _Button.Font = _ButtonFont;
                    _Button.FlatStyle = FlatStyle.Flat;
                    _Button.Text = "_";
                    _Button.TextAlign = ContentAlignment.MiddleCenter;
                    _Button.Name = "_Button";
                    _Button.Tag = new DragbarElementID(_ButtonPos, _ButtonDockID, _ButtonWidth);
                    _Button.Location = GetControlPos(_ButtonPos, _ButtonDockID, _ButtonWidth);
                    _Button.Click += MinimizeButton_Click;

                    DragPanel.Controls.Add(_Button);
                }

                if (HasMaxButton)
                {
                    MaxButton = new Button();
                    MaxButton.BackColor = MaxButtonBackColor;
                    MaxButton.ForeColor = MaxButtonForeColor;
                    MaxButton.Height = MaxButtonHeight;
                    MaxButton.Width = MaxButtonWidth;
                    MaxButton.Font = MaxButtonFont;
                    MaxButton.FlatStyle = FlatStyle.Flat;
                    MaxButton.Text = "☐";
                    MaxButton.TextAlign = ContentAlignment.MiddleCenter;
                    MaxButton.Name = "MaxButton";
                    MaxButton.Tag = new DragbarElementID(MaxButtonPos, MaxButtonDockID, MaxButtonWidth);
                    MaxButton.Location = GetControlPos(MaxButtonPos, MaxButtonDockID, MaxButtonWidth);
                    MaxButton.Click += MaximizeButton_Click;

                    DragPanel.Controls.Add(MaxButton);
                }

                if (HasTitle)
                {
                    TitleLabel = new Label();
                    TitleLabel.ForeColor = TitleTextColor;
                    TitleLabel.Font = TitleTextFont;
                    TitleLabel.Height = DragPanel.Height - 2 * DragBarInnerMargin;
                    TitleLabel.Width = TitleWidth;
                    TitleLabel.AutoSize = false;
                    TitleLabel.AutoEllipsis = true;
                    TitleLabel.TextAlign = ContentAlignment.MiddleCenter;
                    TitleLabel.Text = ParentForm.Text;
                    TitleLabel.Name = "TitleLabel";
                    TitleLabel.Tag = new DragbarElementID(TitlePos, TitleDockID, TitleLabel.Width);
                    TitleLabel.Location = GetControlPos(TitlePos, TitleDockID, TitleLabel.Width);
                    TitleLabel.MouseDown += DragPanel_MouseDown;
                    TitleLabel.MouseMove += DragPanel_MouseMove;
                    TitleLabel.MouseUp += DragPanel_MouseUp;

                    DragPanel.Controls.Add(TitleLabel);
                }

                ParentForm.Controls.Add(DragPanel);

                if (ShowSizableBorders && !SizableBordersTopMost)
                {
                    CreateSizableBorders();
                }

                ParentForm.Activated += Form_Activated;
                ParentForm.Deactivate += Form_Deactivated;
                ParentForm.SizeChanged += ParentForm_Resized;
                ParentForm.ControlAdded += ParentForm_ControlAdded;
                ParentForm.ControlRemoved += ParentForm_ControlRemoved;
                ParentForm.TextChanged += ParentForm_TextChanged;
                ParentForm.MouseWheel += ParentForm_Scroll;

                CurNDXState = NDXState.Initialized;

                LoadingNDX = false;
            }
            else
            {
                throw new ArgumentException("NDX have already been initialized!", "InitializeNDX()");
            }
        }

        void CreateSizableBorders()
        {
            if (ShowSizableBorders)
            {
                SizablePanelLeft = new Panel();
                SizablePanelLeft.BackColor = SizableBordersColor;
                SizablePanelLeft.Name = "SizablePanelLeft";
                SizablePanelLeft.Location = new Point(0, 0);
                SizablePanelLeft.MouseDown += SizableBorders_MouseDown;
                SizablePanelLeft.MouseMove += SizableBorders_MouseMove;
                SizablePanelLeft.MouseUp += SizableBorders_MouseUp;
                SizablePanelLeft.Tag = "NDX";

                ParentForm.Controls.Add(SizablePanelLeft);

                SizablePanelTop = new Panel();
                SizablePanelTop.BackColor = SizableBordersColor;
                SizablePanelTop.Name = "SizablePanelTop";
                SizablePanelTop.Location = new Point(0, 0);
                SizablePanelTop.MouseDown += SizableBorders_MouseDown;
                SizablePanelTop.MouseMove += SizableBorders_MouseMove;
                SizablePanelTop.MouseUp += SizableBorders_MouseUp;
                SizablePanelTop.Tag = "NDX";

                ParentForm.Controls.Add(SizablePanelTop);

                SizablePanelRight = new Panel();
                SizablePanelRight.BackColor = SizableBordersColor;
                SizablePanelRight.Name = "SizablePanelRight";
                SizablePanelRight.MouseDown += SizableBorders_MouseDown;
                SizablePanelRight.MouseMove += SizableBorders_MouseMove;
                SizablePanelRight.MouseUp += SizableBorders_MouseUp;
                SizablePanelRight.Tag = "NDX";

                ParentForm.Controls.Add(SizablePanelRight);

                SizablePanelBottom = new Panel();
                SizablePanelBottom.BackColor = SizableBordersColor;
                SizablePanelBottom.Name = "SizablePanelBottom";
                SizablePanelBottom.MouseDown += SizableBorders_MouseDown;
                SizablePanelBottom.MouseMove += SizableBorders_MouseMove;
                SizablePanelBottom.MouseUp += SizableBorders_MouseUp;
                SizablePanelBottom.Tag = "NDX";

                ParentForm.Controls.Add(SizablePanelBottom);
            }
        }

        private void ParentForm_TextChanged(object sender, EventArgs e)
        {
            TitleLabel.Text = ParentForm.Text;
        }

        private void ParentForm_ControlAdded(object sender, ControlEventArgs e)
        {
            if (Sizable)
                e.Control.Location = new Point(e.Control.Location.X + SizableBordersWidth, e.Control.Location.Y + DragbarHeight);
            else
                e.Control.Location = new Point(e.Control.Location.X, e.Control.Location.Y + DragbarHeight);

            if (!LoadingNDX)
            {
                int Count = 0;
                for (int i = 0; i < ParentForm.Controls.Count; i++)
                {
                    if ((string)ParentForm.Controls[i].Tag != "NDX")
                    {
                        ParentForm.Controls[i].Location = new Point(OriginalControlPoints[Count].X, OriginalControlPoints[Count].Y);
                        Count++;
                    }
                }
                OriginalControlPoints.Clear();
                for (int i = 0; i < ParentForm.Controls.Count; i++)
                    if ((string)ParentForm.Controls[i].Tag != "NDX")
                        OriginalControlPoints.Add(ParentForm.Controls[i].Location);
            }
        }

        private void ParentForm_ControlRemoved(object sender, ControlEventArgs e)
        {
            if (!LoadingNDX)
            {
                int Count = 0;
                for (int i = 0; i < ParentForm.Controls.Count; i++)
                {
                    if ((string)ParentForm.Controls[i].Tag != "NDX")
                    {
                        ParentForm.Controls[i].Location = new Point(OriginalControlPoints[Count].X, OriginalControlPoints[Count].Y);
                        Count++;
                    }
                }
                OriginalControlPoints.Clear();
                for (int i = 0; i < ParentForm.Controls.Count; i++)
                    if ((string)ParentForm.Controls[i].Tag != "NDX")
                        OriginalControlPoints.Add(ParentForm.Controls[i].Location);
            }
        }

        private void ParentForm_Validated(object sender, ControlEventArgs e)
        {
            GetFarPoint();
        }

        void GetFarPoint()
        {
            Point PrePoint = FarPoint;
            foreach (Control InnerControl in ParentForm.Controls)
            {
                if ((string)InnerControl.Tag != "NDX")
                {
                    if (InnerControl.Location.X + InnerControl.Width > FarPoint.X)
                        FarPoint = new Point(InnerControl.Location.X + InnerControl.Width, FarPoint.Y);
                    if (InnerControl.Location.Y + InnerControl.Height > FarPoint.Y)
                        FarPoint = new Point(FarPoint.X, InnerControl.Location.Y + InnerControl.Height);
                }
            }
            if (FarPoint.X == 0)
                FarPoint = new Point(1, FarPoint.Y);
            if (FarPoint.Y == 0)
                FarPoint = new Point(FarPoint.X, 1);

            if (PrePoint.X != FarPoint.X)
                FarPoint = new Point(FarPoint.X + VerticalScrollBarWidth, FarPoint.Y);
            if (PrePoint.Y != FarPoint.Y)
                FarPoint = new Point(FarPoint.X, FarPoint.Y + HorisontalScrollBarHeight);
        }

        public Point GetControlPos(DragBarLocation _ControlPos, int _ID, int _ControlWidth)
        {
            if (_ControlPos == DragBarLocation.Left)
            {
                int Offset = 0;
                foreach(Control _InnerControl in DragPanel.Controls)
                {
                    DragbarElementID ElementID = (DragbarElementID)_InnerControl.Tag;
                    if (ElementID.DragbarPos == DragBarLocation.Left)
                    {
                        if (ElementID.ID < _ID)
                        {
                            Offset += _InnerControl.Width + DragBarInnerMargin;
                        }
                        else
                            if (ElementID.ID == _ID)
                                _InnerControl.Location = new Point(_InnerControl.Location.X + (_ControlWidth + DragBarInnerMargin), _InnerControl.Location.Y);
                    }
                }
                return new Point(DragBarInnerMargin + Offset, DragBarInnerMargin);
            }
            if (_ControlPos == DragBarLocation.Center)
            {
                return Point.Empty;
            }
            if (_ControlPos == DragBarLocation.Right)
            {
                int Offset = 0;
                foreach (Control _InnerControl in DragPanel.Controls)
                {
                    DragbarElementID ElementID = (DragbarElementID)_InnerControl.Tag;
                    if (ElementID.DragbarPos == DragBarLocation.Right)
                    {
                        if (ElementID.ID < _ID)
                        {
                            Offset += _InnerControl.Width + DragBarInnerMargin;
                        }
                        else
                            if (ElementID.ID == _ID)
                                _InnerControl.Location = new Point(_InnerControl.Location.X - (_ControlWidth + DragBarInnerMargin), _InnerControl.Location.Y);
                    }
                }
                return new Point(DragPanel.Width - DragBarInnerMargin - _ControlWidth - Offset, DragBarInnerMargin);
            }
            return Point.Empty;
        }

        private void Form_Activated(object sender, EventArgs e)
        {
            if (ParentForm.WindowState == FormWindowState.Minimized)
            {

            }
            else
            {
                if (HasBeenDeactivated)
                {
                    foreach (Control _Innercontrol in DragPanel.Controls)
                    {
                        _Innercontrol.BackColor = Color.FromArgb((int)(_Innercontrol.BackColor.R * 2), (int)(_Innercontrol.BackColor.G * 2), (int)(_Innercontrol.BackColor.B * 2));
                    }
                    DragPanel.BackColor = Color.FromArgb((int)(DragPanel.BackColor.R * 2), (int)(DragPanel.BackColor.G * 2), (int)(DragPanel.BackColor.B * 2));
                    if (Sizable)
                    {
                        SizablePanelLeft.BackColor = Color.FromArgb((int)(SizablePanelLeft.BackColor.R * 2), (int)(SizablePanelLeft.BackColor.G * 2), (int)(SizablePanelLeft.BackColor.B * 2));
                        SizablePanelTop.BackColor = Color.FromArgb((int)(SizablePanelTop.BackColor.R * 2), (int)(SizablePanelTop.BackColor.G * 2), (int)(SizablePanelTop.BackColor.B * 2));
                        SizablePanelRight.BackColor = Color.FromArgb((int)(SizablePanelRight.BackColor.R * 2), (int)(SizablePanelRight.BackColor.G * 2), (int)(SizablePanelRight.BackColor.B * 2));
                        SizablePanelBottom.BackColor = Color.FromArgb((int)(SizablePanelBottom.BackColor.R * 2), (int)(SizablePanelBottom.BackColor.G * 2), (int)(SizablePanelBottom.BackColor.B * 2));
                    }
                    HasBeenDeactivated = false;
                }
            }
        }

        private void Form_Deactivated(object sender, EventArgs e)
        {
            if (ParentForm.WindowState != FormWindowState.Minimized)
            {
                if (!HasBeenDeactivated)
                {
                    HasBeenDeactivated = true;

                    foreach (Control _Innercontrol in DragPanel.Controls)
                    {
                        _Innercontrol.BackColor = Color.FromArgb((int)(_Innercontrol.BackColor.R * 0.5), (int)(_Innercontrol.BackColor.G * 0.5), (int)(_Innercontrol.BackColor.B * 0.5));
                    }
                    DragPanel.BackColor = Color.FromArgb((int)(DragPanel.BackColor.R * 0.5), (int)(DragPanel.BackColor.G * 0.5), (int)(DragPanel.BackColor.B * 0.5));
                    if (Sizable)
                    {
                        SizablePanelLeft.BackColor = Color.FromArgb((int)(SizablePanelLeft.BackColor.R * 0.5), (int)(SizablePanelLeft.BackColor.G * 0.5), (int)(SizablePanelLeft.BackColor.B * 0.5));
                        SizablePanelTop.BackColor = Color.FromArgb((int)(SizablePanelTop.BackColor.R * 0.5), (int)(SizablePanelTop.BackColor.G * 0.5), (int)(SizablePanelTop.BackColor.B * 0.5));
                        SizablePanelRight.BackColor = Color.FromArgb((int)(SizablePanelRight.BackColor.R * 0.5), (int)(SizablePanelRight.BackColor.G * 0.5), (int)(SizablePanelRight.BackColor.B * 0.5));
                        SizablePanelBottom.BackColor = Color.FromArgb((int)(SizablePanelBottom.BackColor.R * 0.5), (int)(SizablePanelBottom.BackColor.G * 0.5), (int)(SizablePanelBottom.BackColor.B * 0.5));
                    }
                }
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            if (DeactivateDesign == NDXShowHideDesign.None)
            {
                ParentForm.WindowState = FormWindowState.Minimized;
            }
        }

        private void MaximizeButton_Click(object sender, EventArgs e)
        {
            if (ParentForm.WindowState == FormWindowState.Maximized)
            {
                if (DeactivateDesign == NDXShowHideDesign.None)
                {
                    ParentForm.WindowState = FormWindowState.Normal;
                    ValidateLayout();
                }
            }
            else
            {
                if (DeactivateDesign == NDXShowHideDesign.None)
                {
                    ParentForm.WindowState = FormWindowState.Maximized;
                    ValidateLayout();
                }
            }
        }

        private void ParentForm_Resized(object sender, EventArgs e)
        {
            if (!WaitTilSizeingDone)
                ValidateLayout();
        }

        private void ValidateLayout()
        {
            DragPanel.Width = ParentForm.Width;
            if (HasXButton)
                XButton.Location = GetControlPos(XButtonPos, XButtonDockID, XButtonWidth);
            if (Has_Button)
                _Button.Location = GetControlPos(_ButtonPos, _ButtonDockID, _ButtonWidth);
            if (HasMaxButton)
                MaxButton.Location = GetControlPos(MaxButtonPos, MaxButtonDockID, MaxButtonWidth);
            if (HasTitle)
                TitleLabel.Location = GetControlPos(TitlePos, TitleDockID, TitleLabel.Width);

            DragPanel.BringToFront();

            if (Sizable && ShowSizableBorders)
            {
                SizablePanelLeft.Location = new Point(0, 0);
                SizablePanelLeft.Size = new Size(SizableBordersWidth, ParentForm.Height);
                SizablePanelTop.Location = new Point(0, 0);
                SizablePanelTop.Size = new Size(ParentForm.Width, SizableBordersWidth);
                SizablePanelRight.Location = new Point(ParentForm.Width - SizableBordersWidth, 0);
                SizablePanelRight.Size = new Size(SizableBordersWidth, ParentForm.Height);
                SizablePanelBottom.Location = new Point(0, ParentForm.Height - SizableBordersWidth);
                SizablePanelBottom.Size = new Size(ParentForm.Width, SizableBordersWidth);
                SizablePanelLeft.BringToFront();
                SizablePanelTop.BringToFront();
                SizablePanelRight.BringToFront();
                SizablePanelBottom.BringToFront();

                if (AutoScroll)
                {
                    UpdateScrollbars(true);
                }
            }
            else
            {
                if (AutoScroll)
                {
                    UpdateScrollbars(false);
                }
            }
        }

        void UpdateScrollbars(bool Sizable)
        {
            float ScrollPosV = (float)((float)(VerticalScrollPanelInner.Location.Y - ScrollBarInnerMargin) / (float)((VerticalScrollPanel.Height - ScrollBarInnerMargin * 2) - VerticalScrollPanelInner.Height));
            if (ScrollPosV > 1)
                ScrollPosV = 1;
            if (ScrollPosV < 0)
                ScrollPosV = 0;

            float ScrollPosH = (float)((float)(HorisontalScrollPanelInner.Location.X - ScrollBarInnerMargin) / (float)((HorisontalScrollPanel.Width - ScrollBarInnerMargin * 2) - HorisontalScrollPanelInner.Width));
            if (ScrollPosH > 1)
                ScrollPosH = 1;
            if (ScrollPosH < 0)
                ScrollPosH = 0;

            GetFarPoint();

            if (FarPoint.Y > ParentForm.Height - SizableBordersWidth - (HorisontalScrollBarHeight * Convert.ToInt32(HorisontalScrollPanel.Visible)))
                VerticalScrollPanel.Visible = true;
            else
                VerticalScrollPanel.Visible = false;

            if (FarPoint.X > ParentForm.Width - SizableBordersWidth - (VerticalScrollBarWidth * Convert.ToInt32(VerticalScrollPanel.Visible)))
                HorisontalScrollPanel.Visible = true;
            else
                HorisontalScrollPanel.Visible = false;

            int NewOuterHeight;
            if (Sizable)
                NewOuterHeight = ParentForm.Height - DragbarHeight - ScrollBarMargin * 2 - SizableBordersWidth;
            else
                NewOuterHeight = ParentForm.Height - DragbarHeight - ScrollBarMargin * 2;

            if (NewOuterHeight >= VerticalScrollBarMinHeight + ScrollBarInnerMargin * 2)
            {
                VerticalScrollPanel.Height = NewOuterHeight;
                if (Sizable)
                    VerticalScrollPanel.Location = new Point(ParentForm.Width - SizableBordersWidth - VerticalScrollBarWidth - ScrollBarMargin, DragbarHeight + ScrollBarMargin);
                else
                    VerticalScrollPanel.Location = new Point(ParentForm.Width - VerticalScrollBarWidth - ScrollBarMargin, DragbarHeight + ScrollBarMargin);
                int NewHeight = (int)(((float)VerticalScrollPanel.Height / (float)FarPoint.Y) * (float)VerticalScrollPanel.Height - (float)ScrollBarInnerMargin * 2);
                if (NewHeight <= VerticalScrollBarMinHeight)
                    NewHeight = VerticalScrollBarMinHeight;
                VerticalScrollPanelInner.Height = NewHeight;
                if (VerticalScrollPanel.Visible)
                    VerticalScrollPanelInner.Location = new Point(ScrollBarInnerMargin, (int)((VerticalScrollPanel.Height - NewHeight) * ScrollPosV));
                else
                    VerticalScrollPanelInner.Location = new Point(ScrollBarInnerMargin, ScrollBarInnerMargin);
                if (VerticalScrollPanelInner.Location.Y > (VerticalScrollPanel.Height - NewHeight - ScrollBarInnerMargin))
                    VerticalScrollPanelInner.Location = new Point(ScrollBarInnerMargin, (VerticalScrollPanel.Height - NewHeight - ScrollBarInnerMargin));
                if (VerticalScrollPanelInner.Location.Y < ScrollBarInnerMargin)
                    VerticalScrollPanelInner.Location = new Point(ScrollBarInnerMargin, ScrollBarInnerMargin);

                if (OriginalControlPoints.Count != 0)
                {
                    int Count = 0;
                    for (int i = 0; i < ParentForm.Controls.Count; i++)
                    {
                        if ((string)ParentForm.Controls[i].Tag != "NDX")
                        {
                            ParentForm.Controls[i].Location = new Point((int)(OriginalControlPoints[Count].X - ((FarPoint.X - ParentForm.Width + 10) * (float)((float)(HorisontalScrollPanelInner.Location.X - ScrollBarInnerMargin) / (float)((HorisontalScrollPanel.Width - ScrollBarInnerMargin * 2) - HorisontalScrollPanelInner.Width)))), (int)(OriginalControlPoints[Count].Y - ((FarPoint.Y - ParentForm.Height + 10) * (float)((float)(VerticalScrollPanelInner.Location.Y - ScrollBarInnerMargin) / (float)((VerticalScrollPanel.Height - ScrollBarInnerMargin * 2) - VerticalScrollPanelInner.Height)))));
                            Count++;
                        }
                    }

                    if (!VerticalScrollPanel.Visible)
                    {
                        Count = 0;
                        for (int i = 0; i < ParentForm.Controls.Count; i++)
                        {
                            if ((string)ParentForm.Controls[i].Tag != "NDX")
                            {
                                ParentForm.Controls[i].Location = OriginalControlPoints[Count];
                                Count++;
                            }
                        }
                    }
                }
            }
            else
                VerticalScrollPanel.Visible = false;

            int NewOuterWidth;
            if (Sizable)
                NewOuterWidth = ParentForm.Width - VerticalScrollBarWidth - ScrollBarMargin * 3 - SizableBordersWidth * 2;
            else
                NewOuterWidth = ParentForm.Width - VerticalScrollBarWidth - ScrollBarMargin * 3;

            if (NewOuterWidth >= HorisontalScrollBarMinWidth + ScrollBarInnerMargin * 2)
            {
                HorisontalScrollPanel.Width = NewOuterWidth;
                if (Sizable)
                    HorisontalScrollPanel.Location = new Point(ScrollBarMargin + SizableBordersWidth, ParentForm.Height - HorisontalScrollPanel.Height - ScrollBarMargin - SizableBordersWidth);
                else
                    HorisontalScrollPanel.Location = new Point(ScrollBarMargin, ParentForm.Height - HorisontalScrollPanel.Height - ScrollBarMargin);
                int NewWidth = (int)(((float)HorisontalScrollPanel.Width / (float)FarPoint.X) * (float)HorisontalScrollPanel.Width - (float)ScrollBarInnerMargin * 2);
                if (NewWidth <= HorisontalScrollBarMinWidth)
                    NewWidth = HorisontalScrollBarMinWidth;
                HorisontalScrollPanelInner.Width = NewWidth;
                if (HorisontalScrollPanel.Visible)
                    HorisontalScrollPanelInner.Location = new Point((int)((HorisontalScrollPanel.Width - NewWidth) * ScrollPosH), ScrollBarInnerMargin);
                else
                    HorisontalScrollPanelInner.Location = new Point(ScrollBarInnerMargin, ScrollBarInnerMargin);
                if (HorisontalScrollPanelInner.Location.X > (HorisontalScrollPanel.Width - NewWidth - ScrollBarInnerMargin))
                    HorisontalScrollPanelInner.Location = new Point((HorisontalScrollPanel.Width - NewWidth - ScrollBarInnerMargin), ScrollBarInnerMargin);
                if (HorisontalScrollPanelInner.Location.X < ScrollBarInnerMargin)
                    HorisontalScrollPanelInner.Location = new Point(ScrollBarInnerMargin, ScrollBarInnerMargin);

                if (OriginalControlPoints.Count != 0)
                {
                    int Count = 0;
                    for (int i = 0; i < ParentForm.Controls.Count; i++)
                    {
                        if ((string)ParentForm.Controls[i].Tag != "NDX")
                        {
                            ParentForm.Controls[i].Location = new Point((int)(OriginalControlPoints[Count].X - ((FarPoint.X - ParentForm.Width + 10) * (float)((float)(HorisontalScrollPanelInner.Location.X - ScrollBarInnerMargin) / (float)((HorisontalScrollPanel.Width - ScrollBarInnerMargin * 2) - HorisontalScrollPanelInner.Width)))), (int)(OriginalControlPoints[Count].Y - ((FarPoint.Y - ParentForm.Height + 10) * (float)((float)(VerticalScrollPanelInner.Location.Y - ScrollBarInnerMargin) / (float)((VerticalScrollPanel.Height - ScrollBarInnerMargin * 2) - VerticalScrollPanelInner.Height)))));
                            Count++;
                        }
                    }

                    if (!HorisontalScrollPanel.Visible)
                    {
                        Count = 0;
                        for (int i = 0; i < ParentForm.Controls.Count; i++)
                        {
                            if ((string)ParentForm.Controls[i].Tag != "NDX")
                            {
                                ParentForm.Controls[i].Location = OriginalControlPoints[Count];
                                Count++;
                            }
                        }
                    }
                }
            }
            else
                HorisontalScrollPanel.Visible = false;

            VerticalScrollPanel.BringToFront();
            HorisontalScrollPanel.BringToFront();
        }

        private void DragPanel_MouseDown(object sender, MouseEventArgs e)
        {
            DragPanelCursorDownPos = DragPanel.PointToClient(Cursor.Position);
            DragPanelMove = true;
        }

        private void DragPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (DragPanelMove)
            {
                if (ParentFormDragStyle == NDXDragStyle.Opacity)
                    ParentForm.Opacity = DragStyleOpacityFactor;
                if (ParentForm.WindowState == FormWindowState.Maximized)
                {
                    ParentForm.WindowState = FormWindowState.Normal;
                    ValidateLayout();
                    ParentForm.Location = Cursor.Position;
                    DragPanelCursorDownPos = DragPanel.PointToClient(Cursor.Position);
                }
                ParentForm.Location = new Point(Cursor.Position.X - DragPanelCursorDownPos.X, Cursor.Position.Y - DragPanelCursorDownPos.Y);
            }
        }

        private void DragPanel_MouseUp(object sender, MouseEventArgs e)
        {
            if (ParentFormDragStyle == NDXDragStyle.Opacity)
                ParentForm.Opacity = 1;
            DragPanelMove = false;
        }

        private void Sizable_MouseMove()
        {
            if (ParentForm.WindowState != FormWindowState.Maximized)
            {
                if (!SizableBordersMove)
                {
                    Point CursorPos = ParentForm.PointToClient(Cursor.Position);
                    ParentForm.Cursor = Cursors.Default;

                    if (CursorPos.X <= SizableBordersWidth)
                    {
                        if (CursorPos.Y <= SizableBordersWidth)
                        {
                            ParentForm.Cursor = Cursors.SizeNWSE;
                            SizableBordersDragDir = NDXDragDir.WN;
                        }
                        else
                        {
                            if (CursorPos.Y >= ParentForm.Height - SizableBordersWidth)
                            {
                                ParentForm.Cursor = Cursors.SizeNESW;
                                SizableBordersDragDir = NDXDragDir.SW;
                            }
                            else
                            {
                                ParentForm.Cursor = Cursors.SizeWE;
                                SizableBordersDragDir = NDXDragDir.W;
                            }
                        }
                    }
                    else
                    {
                        if (CursorPos.X >= ParentForm.Width - SizableBordersWidth)
                        {
                            if (CursorPos.Y <= SizableBordersWidth)
                            {
                                ParentForm.Cursor = Cursors.SizeNESW;
                                SizableBordersDragDir = NDXDragDir.NE;
                            }
                            else
                            {
                                if (CursorPos.Y >= ParentForm.Height - SizableBordersWidth)
                                {
                                    ParentForm.Cursor = Cursors.SizeNWSE;
                                    SizableBordersDragDir = NDXDragDir.ES;
                                }
                                else
                                {
                                    ParentForm.Cursor = Cursors.SizeWE;
                                    SizableBordersDragDir = NDXDragDir.E;
                                }
                            }
                        }
                        else
                        {
                            if (CursorPos.Y <= SizableBordersWidth)
                            {
                                ParentForm.Cursor = Cursors.SizeNS;
                                SizableBordersDragDir = NDXDragDir.N;
                            }
                            else
                            {
                                if (CursorPos.Y >= ParentForm.Height - SizableBordersWidth)
                                {
                                    ParentForm.Cursor = Cursors.SizeNS;
                                    SizableBordersDragDir = NDXDragDir.S;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SizableBorders_MouseDown(object sender, MouseEventArgs e)
        {
            if (!SizableBordersMove)
            {
                SizableBordersCursorDownPos = DragPanel.PointToClient(Cursor.Position);
                SizableBordersCursorDownPosStart = Cursor.Position;
                SizableBordersCursorDownSize = ParentForm.Size;
                SizableBordersMove = true;
            }
        }

        private void SizableBorders_MouseMove(object sender, MouseEventArgs e)
        {
            if (SizableBordersMove)
            {
                if (ParentForm.WindowState != FormWindowState.Maximized)
                {
                    Size NewSize;
                    switch (SizableBordersDragDir)
                    {
                        case NDXDragDir.N:
                            NewSize = new Size(ParentForm.Width, (SizableBordersCursorDownPosStart.Y - Cursor.Position.Y) + SizableBordersCursorDownSize.Height);
                            if (NewSize.Height > DragbarHeight)
                            {
                                ParentForm.Location = new Point(ParentForm.Location.X, Cursor.Position.Y - SizableBordersCursorDownPos.Y);
                                ParentForm.Size = NewSize;
                            }
                            break;
                        case NDXDragDir.E:
                            NewSize = new Size((Cursor.Position.X - SizableBordersCursorDownPosStart.X) + SizableBordersCursorDownSize.Width, ParentForm.Height);
                            if (NewSize.Width > SizableBordersWidth * 2)
                            {
                                ParentForm.Size = NewSize;
                            }
                            break;
                        case NDXDragDir.S:
                            NewSize = new Size(ParentForm.Width, (Cursor.Position.Y - SizableBordersCursorDownPosStart.Y) + SizableBordersCursorDownSize.Height);
                            if (NewSize.Height > DragbarHeight)
                            {
                                ParentForm.Size = NewSize;
                            }
                            break;
                        case NDXDragDir.W:
                            NewSize = new Size((SizableBordersCursorDownPosStart.X - Cursor.Position.X) + SizableBordersCursorDownSize.Width, ParentForm.Height);
                            if (NewSize.Width > SizableBordersWidth * 2)
                            {
                                ParentForm.Location = new Point(Cursor.Position.X - SizableBordersCursorDownPos.X, ParentForm.Location.Y);
                                ParentForm.Size = NewSize;
                            }
                            break;
                        case NDXDragDir.NE:
                            NewSize = new Size((Cursor.Position.X - SizableBordersCursorDownPosStart.X) + SizableBordersCursorDownSize.Width, (SizableBordersCursorDownPosStart.Y - Cursor.Position.Y) + SizableBordersCursorDownSize.Height);
                            if (NewSize.Height > DragbarHeight && NewSize.Width > SizableBordersWidth * 2)
                            {
                                ParentForm.Location = new Point(ParentForm.Location.X, Cursor.Position.Y - SizableBordersCursorDownPos.Y);
                                ParentForm.Size = NewSize;
                            }
                            else
                            {
                                NewSize = new Size(ParentForm.Width, (SizableBordersCursorDownPosStart.Y - Cursor.Position.Y) + SizableBordersCursorDownSize.Height);
                                if (NewSize.Height > DragbarHeight)
                                {
                                    ParentForm.Location = new Point(ParentForm.Location.X, Cursor.Position.Y - SizableBordersCursorDownPos.Y);
                                    ParentForm.Size = NewSize;
                                }
                                else
                                {
                                    NewSize = new Size((Cursor.Position.X - SizableBordersCursorDownPosStart.X) + SizableBordersCursorDownSize.Width, ParentForm.Height);
                                    if (NewSize.Width > SizableBordersWidth * 2)
                                    {
                                        ParentForm.Size = NewSize;
                                    }
                                }
                            }
                            break;
                        case NDXDragDir.ES:
                            NewSize = new Size((Cursor.Position.X - SizableBordersCursorDownPosStart.X) + SizableBordersCursorDownSize.Width, (Cursor.Position.Y - SizableBordersCursorDownPosStart.Y) + SizableBordersCursorDownSize.Height);
                            if (NewSize.Height > DragbarHeight && NewSize.Width > SizableBordersWidth * 2)
                            {
                                ParentForm.Size = NewSize;
                            }
                            else
                            {
                                NewSize = new Size((Cursor.Position.X - SizableBordersCursorDownPosStart.X) + SizableBordersCursorDownSize.Width, ParentForm.Height);
                                if (NewSize.Width > SizableBordersWidth * 2)
                                {
                                    ParentForm.Size = NewSize;
                                }
                                else
                                {
                                    NewSize = new Size(ParentForm.Width, (Cursor.Position.Y - SizableBordersCursorDownPosStart.Y) + SizableBordersCursorDownSize.Height);
                                    if (NewSize.Height > DragbarHeight)
                                    {
                                        ParentForm.Size = NewSize;
                                    }
                                }
                            }
                            break;
                        case NDXDragDir.SW:
                            NewSize = new Size((SizableBordersCursorDownPosStart.X - Cursor.Position.X) + SizableBordersCursorDownSize.Width, (Cursor.Position.Y - SizableBordersCursorDownPosStart.Y) + SizableBordersCursorDownSize.Height);
                            if (NewSize.Height > DragbarHeight && NewSize.Width > SizableBordersWidth * 2)
                            {
                                ParentForm.Location = new Point(Cursor.Position.X - SizableBordersCursorDownPos.X, ParentForm.Location.Y);
                                ParentForm.Size = NewSize;
                            }
                            else
                            {
                                NewSize = new Size(ParentForm.Width, (Cursor.Position.Y - SizableBordersCursorDownPosStart.Y) + SizableBordersCursorDownSize.Height);
                                if (NewSize.Height > DragbarHeight)
                                {
                                    ParentForm.Size = NewSize;
                                }
                                else
                                {
                                    NewSize = new Size((SizableBordersCursorDownPosStart.X - Cursor.Position.X) + SizableBordersCursorDownSize.Width, ParentForm.Height);
                                    if (NewSize.Width > SizableBordersWidth * 2)
                                    {
                                        ParentForm.Location = new Point(Cursor.Position.X - SizableBordersCursorDownPos.X, ParentForm.Location.Y);
                                        ParentForm.Size = NewSize;
                                    }
                                }
                            }
                            break;
                        case NDXDragDir.WN:
                            NewSize = new Size((SizableBordersCursorDownPosStart.X - Cursor.Position.X) + SizableBordersCursorDownSize.Width, (SizableBordersCursorDownPosStart.Y - Cursor.Position.Y) + SizableBordersCursorDownSize.Height);
                            if (NewSize.Height > DragbarHeight && NewSize.Width > SizableBordersWidth * 2)
                            {
                                ParentForm.Location = new Point(Cursor.Position.X - SizableBordersCursorDownPos.X, Cursor.Position.Y - SizableBordersCursorDownPos.Y);
                                ParentForm.Size = NewSize;
                            }
                            else
                            {
                                NewSize = new Size((SizableBordersCursorDownPosStart.X - Cursor.Position.X) + SizableBordersCursorDownSize.Width, ParentForm.Height);
                                if (NewSize.Width > SizableBordersWidth * 2)
                                {
                                    ParentForm.Location = new Point(Cursor.Position.X - SizableBordersCursorDownPos.X, ParentForm.Location.Y);
                                    ParentForm.Size = NewSize;
                                }
                                else
                                {
                                    NewSize = new Size(ParentForm.Width, (SizableBordersCursorDownPosStart.Y - Cursor.Position.Y) + SizableBordersCursorDownSize.Height);
                                    if (NewSize.Height > DragbarHeight)
                                    {
                                        ParentForm.Location = new Point(ParentForm.Location.X, Cursor.Position.Y - SizableBordersCursorDownPos.Y);
                                        ParentForm.Size = NewSize;
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }

        private void SizableBorders_MouseUp(object sender, MouseEventArgs e)
        {
            SizableBordersMove = false;
            ValidateLayout();
        }

        private void DragPanel_DoubleClick(object sender, EventArgs e)
        {
            if (ParentForm.WindowState == FormWindowState.Maximized)
            {
                ParentForm.WindowState = FormWindowState.Normal;
                ValidateLayout();
            }
            else
            {
                if (ParentForm.WindowState == FormWindowState.Normal)
                {
                    ParentForm.WindowState = FormWindowState.Maximized;
                    ValidateLayout();
                }
            }
        }

        private void VScrollBarInner_MouseDown(object sender, MouseEventArgs e)
        {
            ScrollBarInnerCursorDownPos = VerticalScrollPanelInner.Location;
            ScrollBarInnerCursorDownPosStart = Cursor.Position;
            if (OriginalControlPoints.Count == 0)
            {
                for (int i = 0; i < ParentForm.Controls.Count; i++)
                    if ((string)ParentForm.Controls[i].Tag != "NDX")
                        OriginalControlPoints.Add(ParentForm.Controls[i].Location);
            }
            VScrollBarInnerMove = true;
        }

        private void HScrollBarInner_MouseDown(object sender, MouseEventArgs e)
        {
            ScrollBarInnerCursorDownPos = HorisontalScrollPanelInner.Location;
            ScrollBarInnerCursorDownPosStart = Cursor.Position;
            if (OriginalControlPoints.Count == 0)
            {
                for (int i = 0; i < ParentForm.Controls.Count; i++)
                    if ((string)ParentForm.Controls[i].Tag != "NDX")
                        OriginalControlPoints.Add(ParentForm.Controls[i].Location);
            }
            HScrollBarInnerMove = true;
        }

        private void ScrollBarInner_MouseMove(object sender, MouseEventArgs e)
        {
            if (VScrollBarInnerMove)
                MoveVerticalScrollBars((ScrollBarInnerCursorDownPosStart.Y - Cursor.Position.Y));
            if (HScrollBarInnerMove)
                MoveHorisontalScrollBars((ScrollBarInnerCursorDownPosStart.X - Cursor.Position.X));
        }

        void MoveVerticalScrollBars(int WithValue)
        {
            Point NewLoc = new Point(VerticalScrollPanelInner.Location.X, ScrollBarInnerCursorDownPos.Y - WithValue);
            if (NewLoc.Y < ScrollBarInnerMargin)
                NewLoc = new Point(NewLoc.X, ScrollBarInnerMargin);
            if (NewLoc.Y > VerticalScrollPanel.Height - VerticalScrollPanelInner.Height - ScrollBarInnerMargin)
                NewLoc = new Point(NewLoc.X, VerticalScrollPanel.Height - VerticalScrollPanelInner.Height - ScrollBarInnerMargin);

            VerticalScrollPanelInner.Location = NewLoc;
            int Count = 0;
            for (int i = 0; i < ParentForm.Controls.Count; i++)
            {
                if ((string)ParentForm.Controls[i].Tag != "NDX")
                {
                    ParentForm.Controls[i].Location = new Point((int)(OriginalControlPoints[Count].X - ((FarPoint.X - ParentForm.Width + 10) * (float)((float)(HorisontalScrollPanelInner.Location.X - ScrollBarInnerMargin) / (float)((HorisontalScrollPanel.Width - ScrollBarInnerMargin * 2) - HorisontalScrollPanelInner.Width)))), (int)(OriginalControlPoints[Count].Y - ((FarPoint.Y - ParentForm.Height + 10) * (float)((float)(VerticalScrollPanelInner.Location.Y - ScrollBarInnerMargin) / (float)((VerticalScrollPanel.Height - ScrollBarInnerMargin * 2) - VerticalScrollPanelInner.Height)))));
                    Count++;
                }
            }
        }


        void MoveHorisontalScrollBars(int WithValue)
        {
            Point NewLoc = new Point(ScrollBarInnerCursorDownPos.X - WithValue, HorisontalScrollPanelInner.Location.Y);
            if (NewLoc.X < ScrollBarInnerMargin)
                NewLoc = new Point(ScrollBarInnerMargin, NewLoc.Y);
            if (NewLoc.X > HorisontalScrollPanel.Width - HorisontalScrollPanelInner.Width - ScrollBarInnerMargin)
                NewLoc = new Point(HorisontalScrollPanel.Width - HorisontalScrollPanelInner.Width - ScrollBarInnerMargin, NewLoc.Y);

            HorisontalScrollPanelInner.Location = NewLoc;
            int Count = 0;
            for (int i = 0; i < ParentForm.Controls.Count; i++)
            {
                if ((string)ParentForm.Controls[i].Tag != "NDX")
                {
                    ParentForm.Controls[i].Location = new Point((int)(OriginalControlPoints[Count].X - ((FarPoint.X - ParentForm.Width + 10) * (float)((float)(HorisontalScrollPanelInner.Location.X - ScrollBarInnerMargin) / (float)((HorisontalScrollPanel.Width - ScrollBarInnerMargin * 2) - HorisontalScrollPanelInner.Width)))), (int)(OriginalControlPoints[Count].Y - ((FarPoint.Y - ParentForm.Height + 10) * (float)((float)(VerticalScrollPanelInner.Location.Y - ScrollBarInnerMargin) / (float)((VerticalScrollPanel.Height - ScrollBarInnerMargin * 2) - VerticalScrollPanelInner.Height)))));
                    Count++;
                }
            }
        }

        private void ScrollBarInner_MouseUp(object sender, MouseEventArgs e)
        {
            VScrollBarInnerMove = false;
            HScrollBarInnerMove = false;
        }


        private void ParentForm_Scroll(object sender, MouseEventArgs e)
        {
            if (OriginalControlPoints.Count == 0)
            {
                for (int i = 0; i < ParentForm.Controls.Count; i++)
                    if ((string)ParentForm.Controls[i].Tag != "NDX")
                        OriginalControlPoints.Add(ParentForm.Controls[i].Location);
            }
            ScrollBarInnerCursorDownPos = VerticalScrollPanelInner.Location;
            if (e.Delta > 0)
                MoveVerticalScrollBars(ScrollWheenSteps);
            else
                MoveVerticalScrollBars(-ScrollWheenSteps);
        }
    }

    public delegate void MouseMovedEvent();

    public class GlobalMouseHandler : IMessageFilter
    {
        private const int WM_MOUSEMOVE = 0x0200;

        public event MouseMovedEvent TheMouseMoved;

        #region IMessageFilter Members

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_MOUSEMOVE)
            {
                if (TheMouseMoved != null)
                {
                    TheMouseMoved();
                }
            }
            return false;
        }

        #endregion
    }
}
