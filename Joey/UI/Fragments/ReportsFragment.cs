using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using Toggl.Phoebe.Data;
using Toggl.Phoebe.Data.Models;
using Toggl.Phoebe.Data.Reports;
using Toggl.Joey.UI.Utils;
using Toggl.Joey.UI.Views;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using ListFragment = Android.Support.V4.App.ListFragment;

namespace Toggl.Joey.UI.Fragments
{
    public class ReportsFragment : ListFragment
    {
        private BarChart barChart;
        private PieChart pieChart;
        private TextView totalValue;
        private TextView billableValue;
        private SummaryReportView summaryReport;
        private int backDate;
        private ReportsScrollView mainView;

        public ReportsFragment (int period, ZoomLevel zoom)
        {
            backDate = period;
            summaryReport = new SummaryReportView ();
            summaryReport.Period = zoom;
        }

        public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate (Resource.Layout.ReportsFragment, container, false);
            mainView = view.FindViewById<ReportsScrollView> (Resource.Id.ReportsScrollView);

            barChart = view.FindViewById<BarChart> (Resource.Id.BarChart);
            pieChart = view.FindViewById<PieChart> (Resource.Id.PieChart);

            totalValue = view.FindViewById<TextView> (Resource.Id.TotalValue);
            billableValue = view.FindViewById<TextView> (Resource.Id.BillableValue);

            LoadElements ();

            return view;
        }

        public override void OnViewCreated (View view, Bundle savedInstanceState)
        {
            base.OnViewCreated (view, savedInstanceState);
            ListView.SetClipToPadding (false);
        }

        public override void OnListItemClick (ListView l, View v, int position, long id)
        {
            var adapter = ListView.Adapter as ProjectListAdapter;
            if (pieChart.CurrentSlice == position) {
                pieChart.SelectSlice (-1);
                adapter.SetFocus (-1);
            } else {
                pieChart.SelectSlice (position);
                adapter.SetFocus (position);
            }
        }

        private void OnSliceSelect (int position)
        {
            var adapter = ListView.Adapter as ProjectListAdapter;
            adapter.SetFocus (position);
            if (position != -1) {
                ListView.SmoothScrollToPositionFromTop (position, 0);
            }
        }

        private void EnsureAdapter ()
        {
            var adapter = new ProjectListAdapter (summaryReport.Projects);
            ListAdapter = adapter;
        }

        private void EmptyState ()
        {
            totalValue.Text = summaryReport.FormatMilliseconds (0);
            billableValue.Text = summaryReport.FormatMilliseconds (0);
        }

        private async void LoadElements ()
        {
            Console.WriteLine ("loading elements, backDate: {0}", backDate);
            await LoadData ();
            Console.WriteLine ("data loaded, backDate: {0}", backDate);

            totalValue.Text = summaryReport.TotalGrand;
            billableValue.Text = summaryReport.TotalBillale;
            EnsureAdapter ();
            GeneratePieChart ();
            GenerateBarChart ();
            StretchUpperView ();
//            StretchListView ();
            mainView.BarChartSnapPos = 0;
            mainView.InnerList = ListView;
            mainView.InnerPieChart = pieChart;
            Console.WriteLine ("loading elements end, backDate:  {0}", backDate);

        }

        private void StretchUpperView ()
        {
//            Console.WriteLine ("Strech upper  view");
            var lp = (ViewGroup.MarginLayoutParams)barChart.LayoutParameters;
            lp.BottomMargin = mainView.Height - barChart.Bottom - pieChart.Height / 3;
            mainView.PieChartSnapPos = barChart.Bottom + lp.BottomMargin;
            barChart.RequestLayout ();
        }

        private void StretchListView ()
        {
            Console.WriteLine ("Strech list view");
            var listViewHeight = mainView.Height - pieChart.Height;
            var layoutParams = ListView.LayoutParameters;
            layoutParams.Height = listViewHeight;
            ListView.LayoutParameters = layoutParams;
            ListView.RequestLayout ();
            Console.WriteLine ("list stretched");

        }

        private void GenerateBarChart ()
        {
            barChart.Reset ();
            foreach (var row in summaryReport.Activity) {
                var bar = new BarItem ();
                bar.Value = (float)row.TotalTime;
                bar.Billable = (float)row.BillableTime;
                bar.Name = row.StartTime.ToShortTimeString ();
                barChart.AddBar (bar);
            }
            barChart.CeilingValue = summaryReport.MaxTotal;
            barChart.BarTitles = summaryReport.ChartRowLabels;
            barChart.LineTitles = summaryReport.ChartTimeLabels;
            barChart.Refresh ();
        }

        private void GeneratePieChart ()
        {
            pieChart.Reset ();
            var listener = new SliceListener ();
            pieChart.SetOnSliceClickedListener (listener);
            pieChart.SliceClicked += OnSliceSelect;
            foreach (var project in summaryReport.Projects) {
                var slice = new PieSlice ();
                slice.Value = project.TotalTime;
                slice.Color = Color.ParseColor (ProjectModel.HexColors [project.Color % ProjectModel.HexColors.Length]);
                pieChart.AddSlice (slice);
            }
            pieChart.Refresh ();
        }

        private async Task LoadData ()
        {
            await summaryReport.Load (backDate);
        }

        public class SliceListener : PieChart.IOnSliceClickedListener
        {
            public void OnClick (int index)
            {
            }
        }
    }

    public class ProjectListAdapter : BaseAdapter
    {
        List<ReportProject> dataView;
        private View colorSquare;
        private TextView projectName;
        private TextView projectDuration;
        private int focus = -1;

        public ProjectListAdapter (List<ReportProject> dataView)
        {
            this.dataView = dataView;
        }

        public override Java.Lang.Object GetItem (int position)
        {
            return null;
        }

        public override long GetItemId (int position)
        {
            return position;
        }

        public override View GetView (int position, View convertView, ViewGroup parent)
        {
            var view = LayoutInflater.FromContext (parent.Context).Inflate (Resource.Layout.ReportsProjectListItem, parent, false);
            projectName = view.FindViewById<TextView> (Resource.Id.ProjectName).SetFont (Font.Roboto);
            colorSquare = view.FindViewById<View> (Resource.Id.ColorSquare);
            projectDuration = view.FindViewById<TextView> (Resource.Id.ProjectDuration).SetFont (Font.Roboto);
            if (String.IsNullOrEmpty (dataView [position].Project)) {
                projectName.SetText (Resource.String.ReportsListViewNoProject);
            } else {
                projectName.Text = dataView [position].Project;
            }

            projectDuration.Text = FormatMilliseconds (dataView [position].TotalTime);
            var SquareDrawable = new GradientDrawable ();
            SquareDrawable.SetCornerRadius (5);
            SquareDrawable.SetColor (Color.ParseColor (ProjectModel.HexColors [dataView [position].Color % ProjectModel.HexColors.Length]));

            if (focus == position) {
                SquareDrawable.SetShape (ShapeType.Oval);
            } else if (focus != -1 && focus != position) {
                SquareDrawable.SetShape (ShapeType.Rectangle);
                SquareDrawable.SetAlpha (150);
                projectName.SetTextColor (Color.LightGray);
                projectDuration.SetTextColor (Color.LightGray);
            }
            colorSquare.SetBackgroundDrawable (SquareDrawable);
            return view;
        }

        public void SetFocus (int selected)
        {
            focus = selected;
            NotifyDataSetChanged ();
        }

        public override int Count {
            get {
                return dataView.Count;
            }
        }

        private string FormatMilliseconds (long ms)
        {
            var timeSpan = TimeSpan.FromMilliseconds (ms);
            return String.Format ("{0}:{1:mm\\:ss}", Math.Floor (timeSpan.TotalHours).ToString ("00"), timeSpan);
        }
    }
}
