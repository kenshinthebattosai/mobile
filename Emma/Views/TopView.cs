using System;
using UIKit;
using Foundation;
using Cirrious.FluentLayouts.Touch;

namespace Toggl.Emma.Views
{
    public class TopView : UIView
    {
        private UILabel timeLabel;
        private StartStopBtn startBtn;
        private UILabel titleLabel;

        public TopView ()
        {
            Add (titleLabel = new UILabel () {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = "StartTogglLabel".Tr(),
                Font = UIFont.FromName ( "Helvetica", 18f),
                TextColor = UIColor.White,

            });

            Add (timeLabel = new UILabel () {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = "00:00:00",
                Font = UIFont.FromName ( "Helvetica", 13f),
                TextColor = UIColor.White,
            });

            Add (startBtn = new StartStopBtn () {
                TranslatesAutoresizingMaskIntoConstraints = false,

            });

            startBtn.TouchUpInside += (sender, e) => {
                IsRunning = !IsRunning;
                if ( StartBtnPressed != null) {
                    StartBtnPressed.Invoke ( this, e);
                }
            };
        }

        public override void UpdateConstraints ()
        {
            if (Constraints.Length == 0) {
                this.AddConstraints (

                    titleLabel.AtLeftOf (this, 0f),
                    titleLabel.WithSameCenterY (this),

                    startBtn.AtRightOf ( this, 10f),
                    startBtn.WithSameCenterY ( this),
                    startBtn.Width().EqualTo ( 40f),

                    timeLabel.WithSameCenterY (this),
                    timeLabel.ToLeftOf ( startBtn, 10f),
                    null
                );
            }

            base.UpdateConstraints ();
        }

        public string Title
        {
            get {
                return titleLabel.Text;
            } set {
                if (titleLabel.Text == value) {
                    return;
                }

                titleLabel.Text = value;
                SetNeedsUpdateConstraints ();
            }
        }

        public string TimeValue
        {
            get {
                return timeLabel.Text;
            } set {
                if (timeLabel.Text == value) {
                    return;
                }

                timeLabel.Text = value;
                SetNeedsUpdateConstraints ();
            }
        }

        public bool IsRunning
        {
            get {
                return startBtn.IsRunning;
            }

            set {
                startBtn.IsRunning = value;
                titleLabel.Text = (startBtn.IsRunning) ? "StopTogglLabel".Tr () : "StartTogglLabel".Tr ();
                SetNeedsUpdateConstraints ();
            }
        }

        public event EventHandler StartBtnPressed;

        [Export ("requiresConstraintBasedLayout")]
        public static new bool RequiresConstraintBasedLayout ()
        {
            return true;
        }
    }
}

