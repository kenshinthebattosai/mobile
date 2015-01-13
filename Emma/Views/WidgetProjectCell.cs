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


        public string ProjectName
        {
            get {
                return projectLabel.Text;
            }

            set {
                if (string.Compare (projectLabel.Text, value, StringComparison.Ordinal) == 0) {
                    return;
                }
                projectLabel.Text = value;
                projectLabel.SizeToFit ();
                NeedsUpdateConstraints ();
            }
        }

        UILabel projectLabel;
        UILabel timeLabel;
        UILabel clientLabel;
        UIView arrowView;

        public WidgetProjectCell (IntPtr handle) : base (handle)
        {
            SelectionStyle = UITableViewCellSelectionStyle.None;

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

            ContentView.Add ( arrowView = new ArrowView {
                TranslatesAutoresizingMaskIntoConstraints = false,
            });
        }

        public override void UpdateConstraints ()
        {
            if (Constraints.Length == 0) {
                ContentView.AddConstraints (

                    projectLabel.AtLeftOf (ContentView, 0f),
                    projectLabel.AtTopOf (ContentView, 10f),

                    clientLabel.AtLeftOf ( ContentView, 0f),
                    clientLabel.Below ( projectLabel, 0f),
                    clientLabel.AtBottomOf (ContentView, 10f),

                    arrowView.AtRightOf (ContentView, 15f),
                    arrowView.WithSameCenterY (ContentView),
                    arrowView.Height().EqualTo ( 35f),
                    arrowView.Width().EqualTo ( 35f),

                    timeLabel.WithSameCenterY (ContentView),
                    timeLabel.ToLeftOf ( arrowView, 10f),

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
    }
}

