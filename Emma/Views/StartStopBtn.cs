using CoreGraphics;
using UIKit;

namespace Toggl.Emma.Views
{
    public class StartStopBtn : UIButton
    {
        private bool isRunning;

        public bool IsRunning
        {
            get {
                return isRunning;
            }

            set {
                isRunning = value;
                isActive |= isRunning;
                SetNeedsDisplay ();
            }
        }

        private bool isActive;

        public bool IsActive
        {
            get {
                return isActive;
            }

            set {
                isActive = value;
                SetNeedsDisplay ();
            }
        }

        // Color Declarations
        private readonly UIColor greenColor = UIColor.FromRGBA (0.302f, 0.851f, 0.396f, 1.000f);
        private readonly UIColor redColor = UIColor.FromRGBA (1.000f, 61f/255f, 50f/255f, 1.000f);
        private const float radius = 32.0f;

        public StartStopBtn ()
        {
            isRunning = false;
            isActive = false;
        }

        public override void Draw (CGRect rect)
        {
            base.Draw (rect);

            var posX = (rect.Width - radius) / 2;
            var posY = (rect.Height - radius) / 2;

            using (var context = UIGraphics.GetCurrentContext ()) {

                UIBezierPath ovalPath;
                UIBezierPath bezierPath;

                if ( isActive) {
                    if ( isRunning) {

                        // Oval Drawing
                        ovalPath = UIBezierPath.FromOval (new CGRect (posX, posY, radius, radius));
                        redColor.SetFill();
                        ovalPath.Fill();

                        // Rectangle Drawing
                        var rectanglePath = UIBezierPath.FromRect (new CGRect ( posX + 12.0f, posY + 12.0f, 8.0f, 8.0f));
                        UIColor.White.SetFill();
                        rectanglePath.Fill();

                    } else {

                        // Oval Drawing
                        ovalPath = UIBezierPath.FromOval (new CGRect ( posX, posY, radius, radius));
                        greenColor.SetFill ();
                        ovalPath.Fill ();

                        // Bezier Drawing
                        bezierPath = new UIBezierPath ();
                        bezierPath.MoveTo (new CGPoint ( posX + 13.0f, posY + 8.0f));
                        bezierPath.AddLineTo (new CGPoint (posX + 20.0f, posY + 16.47f));
                        bezierPath.AddLineTo (new CGPoint ( posX + 13.0f, posY + 24.0f));
                        UIColor.White.SetStroke ();
                        bezierPath.LineWidth = 1.0f;
                        bezierPath.Stroke ();
                    }
                } else {
                    // Oval Drawing
                    UIColor.White.SetStroke ();

                    ovalPath = UIBezierPath.FromOval (new CGRect ( posX, posY, radius, radius));
                    ovalPath.LineWidth = 1.0f;
                    ovalPath.Stroke ();
                    context.AddPath ( ovalPath.CGPath);

                    // Bezier Drawing
                    bezierPath = new UIBezierPath ();
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