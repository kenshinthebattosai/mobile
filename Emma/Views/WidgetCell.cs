using System;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using Foundation;
using UIKit;
using System.Globalization;

namespace Toggl.Emma.Views
{
    [Register ("WidgetCell")]
    public class WidgetCell : UITableViewCell
    {
        public static NSString WidgetProjectCellId = new NSString ("WidgetCellId");

        public event EventHandler StartBtnPressed;

        private WidgetEntryData data;

        public WidgetEntryData Data
        {
            get {
                return data;
            }

            set {
                data = value;

                // Set state
                if ( data.IsEmpty) {
                    projectLabel.Text = (startBtn.IsRunning) ? "StopTogglLabel".Tr () : "StartTogglLabel".Tr ();
                } else {
                    projectLabel.Text = string.IsNullOrEmpty ( data.ProjectName) ? "CellNoProject".Tr() : data.ProjectName;
                    descriptionLabel.Text = string.IsNullOrEmpty ( data.Description) ? "CellNoDescription".Tr() : data.Description;
                }
                startBtn.IsRunning = data.IsRunning;
                startBtn.IsActive = data.IsEmpty || data.IsRunning;
                descriptionLabel.Hidden = data.IsEmpty;

                // Set time
                timeLabel.Text = value.TimeValue;

                // Set color
                colorBox.BackgroundColor = ( data.IsEmpty) ? UIColor.Clear : UIColorFromHex ( data.Color);

                projectLabel.SizeToFit();
                descriptionLabel.SizeToFit();
                timeLabel.SizeToFit();
            }
        }

        public string TimeValue
        {
            get {
                return timeLabel.Text;
            } set {
                timeLabel.Text = value;
            }
        }

        private UILabel projectLabel;
        private UILabel timeLabel;
        private UILabel descriptionLabel;
        private StartStopBtn startBtn;
        private UIView colorBox;

        public WidgetCell (IntPtr handle) : base (handle)
        {
            SelectionStyle = UITableViewCellSelectionStyle.Gray;

            ContentView.Add ( colorBox = new UIView() {
                TranslatesAutoresizingMaskIntoConstraints = false,
            });

            ContentView.Add (projectLabel = new UILabel {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.FromName ( "Helvetica", 16f),
                Text = "Project X",
                TextColor = UIColor.White,
            });

            ContentView.Add (descriptionLabel = new UILabel {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.FromName ( "Helvetica", 13f),
                Text = "SubProject X",
                TextColor = UIColor.White,
            });

            ContentView.Add (timeLabel = new UILabel {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = "00:00:00",
                Font = UIFont.FromName ( "Helvetica", 13f),
                TextColor = UIColor.White,
            });

            ContentView.Add ( startBtn = new StartStopBtn {
                TranslatesAutoresizingMaskIntoConstraints = false,
            });

            startBtn.TouchUpInside += (sender, e) => {
                startBtn.IsActive = true;
                startBtn.IsRunning = !startBtn.IsRunning;
                if ( StartBtnPressed != null) {
                    StartBtnPressed.Invoke ( this, e);
                }
            };
        }

        public override void UpdateConstraints ()
        {
            if ( ContentView.Constraints.Length > 0) {
                return;
            }

            ContentView.AddConstraints (

                colorBox.AtLeftOf (ContentView, 0f),
                colorBox.AtTopOf (ContentView, 10f),
                colorBox.AtBottomOf (ContentView, 10f),
                colorBox.Width().EqualTo ( 3f),

                projectLabel.AtLeftOf (ContentView, 50f),

                startBtn.AtRightOf (ContentView, 15f),
                startBtn.WithSameCenterY (ContentView),
                startBtn.Height().EqualTo ( 35f),
                startBtn.Width().EqualTo ( 35f),

                timeLabel.WithSameCenterY (ContentView),
                timeLabel.ToLeftOf ( startBtn, 10f)
            );

            if ( data.IsEmpty) {
                ContentView.AddConstraints (
                    projectLabel.WithSameCenterY (ContentView),
                    null
                );
            } else {
                ContentView.AddConstraints (
                    projectLabel.AtTopOf (ContentView, 10f),
                    descriptionLabel.WithSameLeft ( projectLabel),
                    descriptionLabel.Below ( projectLabel, 0f),
                    descriptionLabel.AtBottomOf (ContentView, 10f),
                    null
                );
            }

            base.UpdateConstraints ();
        }

        internal class ArrowView : UIView
        {

            public static nfloat Radius = 31f;

            public ArrowView () : base ( new CGRect ( 0, 0, Radius, Radius))
            {
                BackgroundColor = UIColor.Clear;
            }

            public override void Draw (CGRect rect)
            {
                base.Draw (rect);

                var posX = (rect.Width - Radius) / 2;
                var posY = (rect.Height - Radius) / 2;

                using (var context = UIGraphics.GetCurrentContext ()) {

                    // Oval Drawing
                    UIColor.White.SetStroke ();

                    var ovalPath = UIBezierPath.FromOval (new CGRect ( posX, posY, Radius, Radius));
                    ovalPath.LineWidth = 1.0f;
                    ovalPath.Stroke ();
                    context.AddPath ( ovalPath.CGPath);

                    // Bezier Drawing
                    var bezierPath = new UIBezierPath ();
                    bezierPath.MoveTo (new CGPoint ( posX + 13.0f, posY + 8.0f));
                    bezierPath.AddLineTo (new CGPoint (posX + 20.0f, posY + 16.47f));
                    bezierPath.AddLineTo (new CGPoint ( posX + 13.0f, posY + 24.0f));
                    bezierPath.LineWidth = 1.0f;
                    bezierPath.Stroke ();
                    context.AddPath ( bezierPath.CGPath);
                }
            }
        }

        private UIColor UIColorFromHex ( string hexValue, float alpha = 1f)
        {
            hexValue = hexValue.TrimStart ('#');

            int rgb;
            if (!Int32.TryParse (hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out rgb)) {
                throw new ArgumentException ("Invalid hex string.", "hexValue");
            }

            switch (hexValue.Length) {
            case 6:
                return new UIColor (
                           ((rgb & 0xFF0000) >> 16) / 255.0f,
                           ((rgb & 0x00FF00) >> 8) / 255.0f,
                           (rgb & 0x0000FF) / 255.0f,
                           alpha
                       );
            case 3:
                return new UIColor (
                           (((rgb & 0xF00) >> 4) | ((rgb & 0xF00) >> 8)) / 255.0f,
                           ((rgb & 0x0F0) | (rgb & 0x0F0) >> 4) / 255.0f,
                           ((rgb & 0x00F << 4) | (rgb & 0x00F)) / 255.0f,
                           alpha
                       );
            default:
                throw new ArgumentException ("Invalid hex string.", "hexValue");
            }
        }
    }

    public class WidgetEntryData
    {
        public string Id { get; set; }

        public string ProjectName { get; set; }

        public string Description { get; set; }

        public string TimeValue { get; set; }

        public string Color { get; set; }

        public bool IsRunning { get; set; }

        public bool IsEmpty { get; set; }
    }
}
