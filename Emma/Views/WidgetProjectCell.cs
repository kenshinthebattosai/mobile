using System;
using UIKit;
using Foundation;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;

namespace Toggl.Emma.Views
{
    [Register ("WidgetProjectCell")]
    public class WidgetProjectCell : UITableViewCell
    {
        public static NSString WidgetProjectCellId = new NSString ("WidgetProjectCellId");

        public event EventHandler StartBtnPressed;

        private ProjectData data;

        public ProjectData Data
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
                    projectLabel.Text = data.ProjectName;
                    clientLabel.Text = data.ClientName;
                }
                startBtn.IsRunning = data.IsRunning;
                startBtn.IsActive = data.IsEmpty;
                clientLabel.Hidden = data.IsEmpty;

                // Set time
                timeLabel.Text = value.TimeValue;

                // Set color
                UIColor newColor;
                try {
                    newColor = ConvertToUIColor ( data.Color);
                } catch (Exception ex) {
                    newColor = UIColor.Clear;
                }
                colorBox.BackgroundColor = (data.IsEmpty) ? UIColor.Clear : newColor;

                projectLabel.SizeToFit();
                clientLabel.SizeToFit();
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
        private UILabel clientLabel;
        private StartStopBtn startBtn;
        private UIView colorBox;
        private nfloat boxWidth = 3;
        private nfloat leftMargin = 60f;

        public WidgetProjectCell (IntPtr handle) : base (handle)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            ContentView.Add ( colorBox = new UIView() {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.White
            });

            ContentView.Add (projectLabel = new UILabel {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.FromName ( "Helvetica", 16f),
                Text = "Project X",
                TextColor = UIColor.White,
            });

            ContentView.Add (clientLabel = new UILabel {
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
                    clientLabel.WithSameLeft ( projectLabel),
                    clientLabel.Below ( projectLabel, 0f),
                    clientLabel.AtBottomOf (ContentView, 10f),
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

        public UIColor ConvertToUIColor ( string hexString)
        {
            hexString = hexString.Replace ("#", "");

            if (hexString.Length == 3) {
                hexString = hexString + hexString;
            }

            if (hexString.Length != 6) {
                throw new Exception ("Invalid hex string");
            }

            nint red = nint.Parse (hexString.Substring (0,2), System.Globalization.NumberStyles.AllowHexSpecifier);
            nint green = nint.Parse (hexString.Substring (2,2), System.Globalization.NumberStyles.AllowHexSpecifier);
            nint blue = nint.Parse (hexString.Substring (4,2), System.Globalization.NumberStyles.AllowHexSpecifier);

            return UIColor.FromRGB (red, green, blue);
        }
    }

    public class ProjectData
    {
        public string ProjectName { get; set; }

        public string ClientName { get; set; }

        public string TimeValue { get; set; }

        public string Color { get; set; }

        public bool IsRunning { get; set; }

        public bool IsEmpty { get; set; }
    }
}