using System;
using System.Collections.Generic;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Util;
using Android.Views;
using Toggl.Phoebe.Data;

namespace Toggl.Joey.UI.Views
{
    public class BarChart : View
    {
        public int CeilingValue;
        private List<BarItem> dataObject = new List<BarItem> ();
        private List<String> yAxisLabels = new List<String> ();
        private List<String> xAxisLabels = new List<String> ();
        private Paint canvasPaint = new Paint ();
        private Paint emptyText = new Paint ();
        private Rect basePlate = new Rect ();
        private Rect textBoundsRect = new Rect();
        private Rect rectangle = new Rect ();
        private Rect notBillableRectangle = new Rect ();
        private Canvas baseCanvas;
        private Bitmap baseBitmap;
        private TextPaint textPaint = new TextPaint();
        private Color gridColor = Color.ParseColor ("#CCCCCC");
        private Color lightBlueBarColor = Color.ParseColor ("#80D6FF");
        private Color darkBlueBarColor = Color.ParseColor ("#00AEFF");
        private Color darkGrayBarColor = Color.ParseColor ("#666666");
        private Color emptyStateTextColor = Color.ParseColor ("#808080");
        private ZoomLevel zoomLevel;
        private ChartState currentChartState;
        private int barPadding = 2;
        private int barHeight = 60;
        private int bottomPadding = 35;
        private int leftColumnWidth = 70;
        private int topPadding = 10;
        private int labelTextSize = 20;
        private int yAxisLineWidth = 8;
        private int animationProgress;
        private bool animating;
        private bool redrawBaseBitmap = false;
        private float usableWidth;
        private float loadAnimation;
        private float ceilingSeconds;

        public BarChart (Context context, IAttributeSet attrs) : base (context, attrs)
        {
        }

        public BarChart (Context context, IAttributeSet attrs, int defStyle) : base (context, attrs, defStyle)
        {
        }

        public void Reset ()
        {
            dataObject.Clear ();
            yAxisLabels.Clear ();
            xAxisLabels.Clear ();
        }

        public void AddBar (BarItem point)
        {
            dataObject.Add (point);
        }

        public void Refresh ()
        {
            OnMeasure (MeasuredWidth, MeasuredHeight);
            redrawBaseBitmap = true;
            StartAnimate ();
        }

        public List<string> YAxisLabels {
            get {
                return yAxisLabels;
            }
            set {
                yAxisLabels = value;
            }
        }

        public List<string> XAxisLabels {
            get {
                return xAxisLabels;
            }
            set {
                xAxisLabels = value;
            }
        }

        private string FormatSeconds (double seconds)
        {
            var t = TimeSpan.FromSeconds (seconds);
            return String.Format ("{0}:{1:mm}", (int)t.TotalHours, t);
        }

        private ChartState DetectChartState()
        {
            if (dataObject.Count == 0) {
                return ChartState.Loading;
            }

            bool isEmpty = true;
            foreach (BarItem p in dataObject) {
                if ((int)p.Value > 0) {
                    isEmpty = false;
                }
            }
            return isEmpty ? ChartState.Empty : ChartState.Normal;
        }

        private void BaseBitmap ()
        {
            float usableWidth = Width - leftColumnWidth;
            baseBitmap = Bitmap.CreateBitmap (Width, MeasuredHeight, Bitmap.Config.Argb8888);
            baseCanvas = new Canvas (baseBitmap);

            canvasPaint.Color = Color.White;
            basePlate.Set (leftColumnWidth, 0, Width, Height);
            baseCanvas.DrawRect (basePlate, canvasPaint);

            if (xAxisLabels.Count == 0) {
                xAxisLabels = new List<string> (new [] { "0h", "0h", "0h", "0h", "0h" });
            }
            canvasPaint.Color = gridColor;
            canvasPaint.StrokeWidth = 1;
            canvasPaint.AntiAlias = true;
            for (int i = 0; i <= xAxisLabels.Count - 1; i++) {
                baseCanvas.DrawLine (
                    leftColumnWidth + usableWidth / 6 * (i + 1),
                    topPadding,
                    leftColumnWidth + usableWidth / 6 * (i + 1),
                    Height - bottomPadding,
                    canvasPaint
                );

                canvasPaint.TextSize = labelTextSize;
                canvasPaint.GetTextBounds (xAxisLabels [i], 0, xAxisLabels [i].Length, textBoundsRect);
                baseCanvas.DrawText (
                    xAxisLabels [i],
                    leftColumnWidth + usableWidth / 6 * (i + 1) - textBoundsRect.Width () / 2,
                    Height - bottomPadding / 2 + textBoundsRect.Height () / 2,
                    canvasPaint
                );
            }

            canvasPaint.Color = gridColor;
            canvasPaint.StrokeWidth = yAxisLineWidth;
            baseCanvas.DrawLine (
                leftColumnWidth + yAxisLineWidth / 2,
                0,
                leftColumnWidth + yAxisLineWidth / 2,
                Height,
                canvasPaint
            );

            if (currentChartState == ChartState.Empty || currentChartState == ChartState.Loading) { // draw empty or loading state
                emptyText.Color = emptyStateTextColor;
                emptyText.TextAlign = Paint.Align.Center;
                emptyText.AntiAlias = true;
                emptyText.TextSize = 30;
                baseCanvas.DrawText (
                    currentChartState == ChartState.Empty ? Resources.GetText (Resource.String.ReportsPieChartEmptyHeader) : Resources.GetText (Resource.String.ReportsPieChartLoadingHeader),
                    Width / 2,
                    Height / 2 - 20,
                    emptyText
                );

                textPaint.Color = emptyStateTextColor;
                textPaint.TextAlign = Paint.Align.Center;
                textPaint.AntiAlias = true;
                textPaint.TextSize = 25;

                var emptyStateText = new StaticLayout (
                    currentChartState == ChartState.Empty ? Resources.GetText (Resource.String.ReportsPieChartEmptyText) : Resources.GetText (Resource.String.ReportsPieChartLoadingText),
                    textPaint,
                    500,
                    StaticLayout.Alignment.AlignNormal,
                    1,
                    0,
                    false
                );
                baseCanvas.Translate (Width / 2, Height / 2 - 10);
                emptyStateText.Draw (baseCanvas);
            } else {
                canvasPaint.Color = darkGrayBarColor;
                canvasPaint.TextSize = labelTextSize;
                for (int i = 0; i <= dataObject.Count - 1; i++) {
                    if (zoomLevel != ZoomLevel.Month || (zoomLevel == ZoomLevel.Month && i % 3 == 0)) {
                        canvasPaint.TextSize = labelTextSize;
                        canvasPaint.Color = darkGrayBarColor;

                        textBoundsRect = new Rect ();
                        canvasPaint.GetTextBounds (yAxisLabels [i], 0, yAxisLabels [i].Length, textBoundsRect);

                        baseCanvas.DrawText (
                            yAxisLabels [i],
                            0,
                            (int)((barPadding * 2 + barHeight) * i + topPadding + barPadding + textBoundsRect.Height () / 2 + barHeight / 2),
                            canvasPaint
                        );
                    }
                }
            }

            return;
        }

        public override void Draw (Canvas canvas)
        {
            if (currentChartState != DetectChartState ()) {
                currentChartState = DetectChartState ();
                redrawBaseBitmap = true;
            }
            if (baseBitmap == null || redrawBaseBitmap) {
                BaseBitmap ();
            }
            canvas.DrawBitmap (baseBitmap, 0, 0, canvasPaint);

            loadAnimation = animating ? (float)(animationProgress / 100F) : 1;
            ceilingSeconds = (float)CeilingValue * 3600F;
            usableWidth = Width - leftColumnWidth - (Width - leftColumnWidth) / 6;

            for (int i = 0; i < dataObject.Count; i++) {

                MakeBarAt (i);
                if (notBillableRectangle.Width() > 0) {
                    canvasPaint.Color = lightBlueBarColor;
                    canvas.DrawRect (notBillableRectangle, canvasPaint);
                }
                canvasPaint.Color = (int)dataObject[i].Value > 0 ? darkBlueBarColor : darkGrayBarColor;
                canvas.DrawRect (rectangle, canvasPaint);

                if (animationProgress == 100 && zoomLevel != ZoomLevel.Month && (int)dataObject[i].Value > 0 ) {
                    canvasPaint.TextSize = labelTextSize;
                    var barTitle = FormatSeconds (dataObject[i].Value);
                    canvasPaint.GetTextBounds (barTitle, 0, barTitle.Length, textBoundsRect);
                    canvas.DrawText (
                        barTitle,
                        leftColumnWidth + 10 + (int)((usableWidth * (dataObject[i].Value / ceilingSeconds))),
                        (int)((barPadding * 2) * i + barPadding + barHeight * i + topPadding + barHeight / 2 + textBoundsRect.Height () / 2),
                        canvasPaint
                    );
                }
            }
        }

        private void MakeBarAt(int count)
        {
            if (dataObject [count].Value == 0) {
                rectangle.Set (
                    leftColumnWidth,
                    BarTopPosition (count),
                    leftColumnWidth + yAxisLineWidth,
                    BarBottomPosition (count)
                );
                return;
            }

            var bar = dataObject [count];
            if (bar.Billable < bar.Value) {
                float notBillable = bar.Value - bar.Billable;
                float totalWidth = (usableWidth * (bar.Value / ceilingSeconds));
                float billableWidth = (usableWidth * (bar.Billable / ceilingSeconds));
                float notBillableWidth = (usableWidth * (notBillable / ceilingSeconds));
                if ((loadAnimation * totalWidth) > billableWidth) {
                    rectangle.Set (
                        leftColumnWidth,
                        BarTopPosition (count),
                        leftColumnWidth + (int)(billableWidth),
                        BarBottomPosition (count)
                    );
                    notBillableRectangle.Set (
                        leftColumnWidth,
                        BarTopPosition (count),
                        leftColumnWidth + (int)(notBillableWidth * loadAnimation + billableWidth),
                        BarBottomPosition (count)
                    );
                } else {
                    rectangle.Set (
                        leftColumnWidth,
                        BarTopPosition (count),
                        leftColumnWidth + (int)(loadAnimation * totalWidth),
                        BarBottomPosition (count)
                    );
                }
            } else {
                float totalWidth = (usableWidth * (bar.Value / ceilingSeconds));
                rectangle.Set (
                    leftColumnWidth,
                    BarTopPosition (count),
                    leftColumnWidth + (int)(loadAnimation * totalWidth),
                    BarBottomPosition (count)
                );
                notBillableRectangle = new Rect ();;
            }
        }

        private int BarTopPosition (int count)
        {
            return (barPadding * 2) * count + barPadding + barHeight * count + topPadding;
        }

        private int BarBottomPosition (int count)
        {
            return (barPadding * 2) * count + barPadding + barHeight * (count + 1) + topPadding;
        }
        private void DetermineZoomLevel()
        {
            if (dataObject.Count == 12) {
                zoomLevel = ZoomLevel.Year;
                barHeight = 45;
                labelTextSize = 18;
            } else if (dataObject.Count > 12) {
                zoomLevel = ZoomLevel.Month;
                barHeight = 20;
                barPadding = 1;
                labelTextSize = 16;
            } else {
                zoomLevel = ZoomLevel.Week;
                barHeight = 60;
            }
        }

        protected override void OnMeasure (int widthMeasureSpec, int heightMeasureSpec)
        {
            DetermineZoomLevel ();
            int barCount = dataObject.Count == 0 ? 7 : dataObject.Count;
            int heightSize = (barPadding * 2 + barHeight) * barCount + bottomPadding + topPadding;
            int widthSize = MeasureSpec.GetSize (widthMeasureSpec);

            SetMeasuredDimension (widthSize, heightSize);
        }

        public void StartAnimate ()
        {
            animating = true;
            var animator = ValueAnimator.OfInt (1, 100);
            animator.SetDuration (750);
            animator.Update += (sender, e) => AnimationProgress = (int)e.Animation.AnimatedValue;
            animator.Start ();
        }

        public int AnimationProgress {
            get {
                return animationProgress;
            }
            set {
                animationProgress = value;
                if (value == 100) {
                    animating = false;
                }
                PostInvalidate ();
            }
        }

        private enum ChartState {
            Loading,
            Empty,
            Normal
        }
    }

    public class BarItem
    {
        public Color Color { get; set; }

        public String Name { get; set; }

        public float Value { get; set; }

        public float Billable { get; set; }

        public Path Path { get; set; }

        public Region Region { get; set; }
    }
}

