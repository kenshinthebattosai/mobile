using System;
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Toggl.Phoebe.Data.Models;
using Toggl.Phoebe.Net;
using XPlatUtils;
using Toggl.Joey.UI.Utils;
using Toggl.Joey.UI.Views;

namespace Toggl.Joey.UI.Adapters
{
    class DrawerListAdapter : BaseAdapter
    {
        protected static readonly int ViewTypeDrawerHeader = 0;
        protected static readonly int ViewTypeDrawerItem = 1;
        protected static readonly int ViewTypeDrawerSubItem = 2;
        public static readonly int TimerPageId = 0;
        public static readonly int ReportsPageId = 1;
        public static readonly int ReportsWeekPageId = 5;
        public static readonly int ReportsMonthPageId = 6;
        public static readonly int ReportsYearPageId = 7;
        public static readonly int SettingsPageId = 2;
        public static readonly int LogoutPageId = 3;
        public static readonly int FeedbackPageId = 4;
        private List<DrawerItem> rowItems;
        private readonly List<DrawerItem> collapsedRowItems;
        private readonly AuthManager authManager;

        public DrawerListAdapter ()
        {
            rowItems = new List<DrawerItem> () {
                new DrawerItem () {
                    Id = TimerPageId,
                    TextResId = Resource.String.MainDrawerTimer,
                    ImageResId = Resource.Drawable.IcNavTimer,
                    IsEnabled = true,
                },
                new DrawerItem () {
                    Id = ReportsPageId,
                    TextResId = Resource.String.MainDrawerReports,
                    ImageResId = Resource.Drawable.IcNavReports,
                    IsEnabled = true,
                    SubItems = new List<DrawerItem> () {
                        new DrawerItem () {
                            Id = ReportsWeekPageId,
                            ChildOf = ReportsPageId,
                            TextResId = Resource.String.MainDrawerReportsWeek,
                            ImageResId = 0,
                            IsEnabled = true,
                        },
                        new DrawerItem () {
                            Id = ReportsMonthPageId,
                            ChildOf = ReportsPageId,
                            TextResId = Resource.String.MainDrawerReportsMonth,
                            ImageResId = 0,
                            IsEnabled = true,
                        },
                        new DrawerItem () {
                            Id = ReportsYearPageId,
                            ChildOf = ReportsPageId,
                            TextResId = Resource.String.MainDrawerReportsYear,
                            ImageResId = 0,
                            IsEnabled = true,
                        }
                    }
                },
                new DrawerItem () {
                    Id = SettingsPageId,
                    TextResId = Resource.String.MainDrawerSettings,
                    ImageResId = Resource.Drawable.IcNavSettings,
                    IsEnabled = true,
                },
                new DrawerItem () {
                    Id = FeedbackPageId,
                    TextResId = Resource.String.MainDrawerFeedback,
                    ImageResId = Resource.Drawable.IcNavFeedback,
                    IsEnabled = true,
                },
                new DrawerItem () {
                    Id = LogoutPageId,
                    TextResId = Resource.String.MainDrawerLogout,
                    ImageResId = Resource.Drawable.IcNavLogout,
                    IsEnabled = true,
                }
            };
            collapsedRowItems = rowItems;
            authManager = ServiceContainer.Resolve<AuthManager> ();
        }

        public override int ViewTypeCount
        {
            get { return 3; }
        }

        public override int GetItemViewType (int position)
        {
            if (position == 0) {
                return ViewTypeDrawerHeader;
            } else if (rowItems [position - 1].ChildOf > 0) {
                return ViewTypeDrawerSubItem;
            } else {
                return ViewTypeDrawerItem;
            }
        }

        public override View GetView (int position, View convertView, ViewGroup parent)
        {
            View view = convertView;

            if (GetItemViewType (position) == ViewTypeDrawerHeader) {
                if (view == null) {
                    view = LayoutInflater.FromContext (parent.Context).Inflate (
                               Resource.Layout.MainDrawerListHeader, parent, false);
                    view.Tag = new HeaderViewHolder (view);
                }

                var holder = (HeaderViewHolder)view.Tag;
                holder.Bind ((UserModel)authManager.User);
            } else if (GetItemViewType (position) == ViewTypeDrawerSubItem) {

                if (view == null) {
                    view = LayoutInflater.FromContext (parent.Context).Inflate (
                                Resource.Layout.MainDrawerSubListItem, parent, false);
                    view.Tag = new DrawerSubItemViewHolder (view);
                }

                var holder = (DrawerSubItemViewHolder)view.Tag;
                holder.Bind (GetDrawerItem (position));
            } else {

                if (view == null) {
                    view = LayoutInflater.FromContext (parent.Context).Inflate (
                               Resource.Layout.MainDrawerListItem, parent, false);
                    view.Tag = new DrawerItemViewHolder (view);
                }

                var holder = (DrawerItemViewHolder)view.Tag;
                holder.Bind (GetDrawerItem (position));
            }

            return view;
        }

        public override int Count
        {
            get { return rowItems.Count + 1; } // + 1 is for header
        }

        public override Java.Lang.Object GetItem (int position)
        {
            return null;
        }

        public int GetItemPosition (int id)
        {
            var idx = rowItems.FindIndex (i => i.Id == id);
            return idx >= 0 ? idx + 1 : -1;
        }

        private DrawerItem GetDrawerItem (int position)
        {
            if (position == 0) {
                return null;
            }
            return rowItems [position - 1]; //Header is 0
        }

        public override long GetItemId (int position)
        {
            if (GetItemViewType (position) == ViewTypeDrawerHeader) {
                return -1;
            } else {
                return GetDrawerItem (position).Id;
            }
        }

        public void ExpandCollapse (int position)
        {
            rowItems = collapsedRowItems;
            if (rowItems [position].SubItems == null) {
                return;
            }
            if (rowItems [position].SubItems.Count > 0) {
                var newList = new List<DrawerItem> ();
                int pos = 0;
                foreach (var row in rowItems) {
                    newList.Add (row);
                    if (pos == position) {
                        foreach (var sub in rowItems[position].SubItems) {
                            newList.Add (sub);
                        }
                    }
                    pos++;
                }
                rowItems = newList;
            }
        }

        public override bool IsEnabled (int position)
        {
            var item = GetDrawerItem (position);
            return item != null && item.IsEnabled;
        }

        private class DrawerItem
        {
            public int Id;
            public int TextResId;
            public int ImageResId;
            public int ChildOf = 0;
            public bool IsEnabled;
            public bool Expanded = false;
            public List<DrawerItem> SubItems;
        }

        private class HeaderViewHolder : ModelViewHolder<UserModel>
        {
            public ProfileImageView IconProfileImageView { get; private set; }

            public TextView TitleTextView { get; private set; }

            public HeaderViewHolder (View root) : base (root)
            {
                IconProfileImageView = root.FindViewById<ProfileImageView> (Resource.Id.IconProfileImageView);
                TitleTextView = root.FindViewById<TextView> (Resource.Id.TitleTextView).SetFont (Font.Roboto);
            }

            protected override void ResetTrackedObservables ()
            {
                Tracker.MarkAllStale ();

                if (DataSource != null) {
                    Tracker.Add (DataSource, HandleUserPropertyChanged);
                }

                Tracker.ClearStale ();
            }

            private void HandleUserPropertyChanged (string prop)
            {
                if (prop == UserModel.PropertyName
                        || prop == UserModel.PropertyImageUrl) {
                    Rebind ();
                }
            }


            protected override void Rebind ()
            {
                // Protect against Java side being GCed
                if (Handle == IntPtr.Zero) {
                    return;
                }

                ResetTrackedObservables ();

                if (DataSource == null) {
                    return;
                }

                IconProfileImageView.ImageUrl = DataSource.ImageUrl;
                TitleTextView.Text = DataSource.Name;
            }
        }

        private class DrawerItemViewHolder : BindableViewHolder<DrawerItem>
        {
            public ImageView IconImageView { get; private set; }

            public TextView TitleTextView { get; private set; }

            public DrawerItemViewHolder (View root) : base (root)
            {
                IconImageView = root.FindViewById<ImageView> (Resource.Id.IconImageView);
                TitleTextView = root.FindViewById<TextView> (Resource.Id.TitleTextView).SetFont (Font.Roboto);
            }

            protected override void Rebind ()
            {
                if (DataSource == null) {
                    return;
                }

                IconImageView.SetImageResource (DataSource.ImageResId);
                TitleTextView.SetText (DataSource.TextResId);
                TitleTextView.Enabled = DataSource.IsEnabled;
            }
        }

        private class DrawerSubItemViewHolder : BindableViewHolder<DrawerItem>
        {
            public TextView TitleTextView { get; private set; }

            public DrawerSubItemViewHolder (View root) : base (root)
            {
                TitleTextView = root.FindViewById<TextView> (Resource.Id.TitleTextView).SetFont (Font.Roboto);
            }

            protected override void Rebind ()
            {
                if (DataSource == null) {
                    return;
                }
                TitleTextView.SetText (DataSource.TextResId);
                TitleTextView.Enabled = DataSource.IsEnabled;
            }
        }
    }
}

