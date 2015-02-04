using System;
using System.Globalization;
using Cirrious.FluentLayouts.Touch;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Toggl.Emma.Views
{
    [Register ("WidgetCell")]
    public class WidgetCell : UITableViewCell
    {
        private const string defaultTimeValue = "  00:00:00";

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

                // Set time size
                var nsString = new NSString ( defaultTimeValue);
                var attribs = new UIStringAttributes { Font = timeLabel.Font };
                timeLabel.Bounds = new CGRect ( CGPoint.Empty, nsString.GetSizeUsingAttributes (attribs));

                timeLabel.Text = string.IsNullOrEmpty ( value.TimeValue) ? defaultTimeValue : value.TimeValue;

                // Set color
                colorBox.BackgroundColor = ( data.IsEmpty) ? UIColor.Clear : UIColorFromHex ( data.Color);
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
        private UIView textContentView;

        public WidgetCell (IntPtr handle) : base (handle)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

            textContentView = new UIView() {
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            textContentView.Add (projectLabel = new UILabel {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.FromName ( "Helvetica", 16f),
                Text = "Project",
                TextColor = UIColor.White,
            });

            textContentView.Add (descriptionLabel = new UILabel {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.FromName ( "Helvetica", 13f),
                Text = "Description",
                TextColor = UIColor.White,
            });

            ContentView.Add ( colorBox = new UIView() {
                TranslatesAutoresizingMaskIntoConstraints = false,
            });

            ContentView.Add (timeLabel = new UILabel {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = defaultTimeValue,
                Font = UIFont.FromName ( "Helvetica", 13f),
                TextAlignment = UITextAlignment.Right,
                TextColor = UIColor.White,
            });

            ContentView.Add ( startBtn = new StartStopBtn {
                TranslatesAutoresizingMaskIntoConstraints = false,
            });

            ContentView.Add ( textContentView);

            startBtn.TouchUpInside += (sender, e) => {
                startBtn.IsActive = true;
                startBtn.IsRunning = !startBtn.IsRunning;
                if ( StartBtnPressed != null) {
                    StartBtnPressed.Invoke ( this, e);
                }
            };

            var maskLayer = new CAGradientLayer {
                AnchorPoint = CGPoint.Empty,
                StartPoint = new CGPoint (0.0f, 0.0f),
                EndPoint = new CGPoint (1.0f, 0.0f),
                Colors = new [] {
                    UIColor.FromWhiteAlpha (1, 1).CGColor,
                    UIColor.FromWhiteAlpha (1, 1).CGColor,
                    UIColor.FromWhiteAlpha (1, 0).CGColor,
                },
                Locations = new [] {
                    NSNumber.FromFloat (0f),
                    NSNumber.FromFloat (0.9f),
                    NSNumber.FromFloat (1f),
                },
            };

            textContentView.Layer.Mask = maskLayer;

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

                startBtn.AtRightOf (ContentView, 15f),
                startBtn.WithSameCenterY (ContentView),
                startBtn.Height().EqualTo ( 35f),
                startBtn.Width().EqualTo ( 35f),

                timeLabel.WithSameCenterY (ContentView),
                timeLabel.ToLeftOf ( startBtn, 10f),
                timeLabel.Width().EqualTo ( timeLabel.Bounds.Width),

                textContentView.AtLeftOf (ContentView, 50f),
                textContentView.ToLeftOf ( timeLabel, 5f),
                textContentView.WithSameCenterY ( ContentView),
                textContentView.WithSameHeight ( ContentView)
            );

            textContentView.AddConstraints (
                projectLabel.AtLeftOf (textContentView, 0f)
            );

            if ( data.IsEmpty) {
                textContentView.AddConstraints (
                    projectLabel.WithSameCenterY (textContentView),
                    null
                );
            } else {
                textContentView.AddConstraints (
                    projectLabel.AtTopOf (textContentView, 10f),
                    descriptionLabel.WithSameLeft ( projectLabel),
                    descriptionLabel.Below ( projectLabel, 0f),
                    descriptionLabel.AtBottomOf (textContentView, 10f),
                    null
                );
            }

            base.UpdateConstraints ();

            LayoutIfNeeded();
        }

        public override void LayoutSubviews ()
        {
            textContentView.Layer.Mask.Bounds = textContentView.Frame;

            base.LayoutSubviews ();
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
