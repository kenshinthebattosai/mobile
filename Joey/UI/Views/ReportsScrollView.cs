using Android.Animation;
using Android.Content;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Toggl.Joey.UI.Views
{
    public class ReportsScrollView : ScrollView, GestureDetector.IOnGestureListener
    {

        public int BarChartSnapPos = 0;
        public int PieChartSnapPos = 0;
        public ListView InnerList;
        public PieChart InnerPieChart;
        public BarChart InnerBarChart;
        private const float snapPadding = 30;
        private GestureDetectorCompat gestureDetector;
        private int currentSnap = 0;
        private bool autoScrolling = false;
        private bool scrollRegistered = false;
        private bool listTouch = false;
        private bool barChartTouch = false;


        public ReportsScrollView (Context context) :
            base (context)
        {
            Initialize (context);
        }

        public ReportsScrollView (Context context, IAttributeSet attrs) :
            base (context, attrs)
        {
            Initialize (context);
        }

        public ReportsScrollView (Context context, IAttributeSet attrs, int defStyle) :
            base (context, attrs, defStyle)
        {
            Initialize (context);
        }

        private void Initialize (Context ctx)
        {
            gestureDetector = new GestureDetectorCompat (ctx, this);
        }

        public override bool OnInterceptTouchEvent (MotionEvent ev)
        {
            base.OnInterceptTouchEvent (ev);
            if (currentSnap == PieChartSnapPos && currentSnap == ScrollY && InnerList.Top - currentSnap < ev.GetY ()) {
                listTouch = true;
                gestureDetector.OnTouchEvent (ev);
                return false;
            } else if (currentSnap == BarChartSnapPos) {
                barChartTouch = true;
                gestureDetector.OnTouchEvent (ev);
                return false;
            }

            return true;
        }

        public override bool OnTouchEvent (MotionEvent ev)
        {
            if (currentSnap == PieChartSnapPos && ev.Action == MotionEventActions.Up && scrollRegistered) {
                ResolveSnap ();
                scrollRegistered = false;
            } else if (currentSnap == PieChartSnapPos) {
                gestureDetector.OnTouchEvent (ev);
                return true;
            }else if (currentSnap == BarChartSnapPos && !scrollRegistered) {
                gestureDetector.OnTouchEvent (ev);
                return true;
            }

            if (ev.Action == MotionEventActions.Up) {
                ResolveSnap ();
            }

            if (!autoScrolling)
                base.OnTouchEvent (ev);
            return true;
        }

        private void ResolveSnap ()
        {
            if (autoScrolling)
                return;
            if (currentSnap == BarChartSnapPos) {
                if (ScrollY > currentSnap + snapPadding) {
                    FocusSnapPoint (PieChartSnapPos);
                } else {
                    FocusSnapPoint (currentSnap);
                }
            } else {
                if (ScrollY < currentSnap - snapPadding) {
                    FocusSnapPoint (BarChartSnapPos);
                } else {
                    FocusSnapPoint (currentSnap);
                }
            }
        }

        public bool OnDown (MotionEvent e)
        {
            return false;
        }

        public bool OnFling (MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            return false;
        }

        public void OnLongPress (MotionEvent e)
        {
        }

        public bool OnScroll (MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            if (listTouch) {
                InnerList.ScrollTo (0, ScrollY - ((int)distanceX - (int)distanceY)); 
                listTouch = false;
                return false;
            }
            scrollRegistered = true;
            if (!autoScrolling) {
                ScrollTo (0, ScrollY - ((int)distanceX - (int)distanceY));
            }
            return false;
        }

        public void OnShowPress (MotionEvent e)
        {
        }

        public bool OnSingleTapUp (MotionEvent e)
        {
            if (listTouch) {
                InnerList.OnTouchEvent (e);
                listTouch = false;
                return true;
            }
            if (barChartTouch) {
                InnerBarChart.OnTouchEvent (e);
            }
            scrollRegistered = false;
            InnerPieChart.OnTouchEvent (e);
            return true;
        }

        private void FocusSnapPoint (int snapPoint)
        {
            autoScrolling = true;
            currentSnap = snapPoint;
            var animator = ValueAnimator.OfInt (ScrollY, snapPoint);
            animator.SetDuration (250);
            animator.Update += (sender, e) => ScrollPosition = (int)e.Animation.AnimatedValue;
            animator.Start ();
        }

        private int scrollPosition;

        private int ScrollPosition {
            get {
                return scrollPosition;
            }
            set {
                scrollPosition = value;
                AnimatedScroll ();
            }
        }

        private void AnimatedScroll ()
        {
            ScrollY = ScrollPosition;
            if (ScrollY == (int)currentSnap)
                autoScrolling = false;
        }
    }
}

