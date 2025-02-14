using GTA;
using GTA.UI;
using System.Drawing;

namespace CinematicDrive
{
    public class CinematicBars
    {
        private readonly ContainerElement[] cinematicBars = new ContainerElement[2];

        public CinematicBars() => Setup(0);
        
        public void Setup(int i)
        {
            if (i == 0)
            {
                cinematicBars[0] = new ContainerElement(new PointF(0.0f, -100f), new SizeF(1280f, 108f), Color.Black);
                cinematicBars[1] = new ContainerElement(new PointF(0.0f, 712f), new SizeF(1280f, 108f), Color.Black);
            }
            else
            {
                if (i != 1)
                    return;
                cinematicBars[0] = new ContainerElement(new PointF(0.0f, 0.0f), new SizeF(1280f, 108f), Color.Black);
                cinematicBars[1] = new ContainerElement(new PointF(0.0f, 612f), new SizeF(1280f, 108f), Color.Black);
            }
        }

        public void Show()
        {
            for (int i = 0; i <= 100; i += 2)
            {
                IncreaseY(2);
                Script.Wait(10);
            }
        }

        public void Hide()
        {
            for (int i = 100; i >= 0; i -= 2)
            {
                DecreaseY(2);
                Script.Wait(10);
            }
        }

        public void IncreaseY(int i)
        {
            if ((double)cinematicBars[0].Position.Y >= 0.0)
                return;

            double x1 = (double)cinematicBars[0].Position.X;
            double y1 = (double)cinematicBars[0].Position.Y + (double)i;
            cinematicBars[0].Position = new PointF((float)x1, (float)y1);

            double x2 = (double)cinematicBars[1].Position.X;
            double y2 = (double)cinematicBars[1].Position.Y - (double)i;
            cinematicBars[1].Position = new PointF((float)x2, (float)y2);
        }

        public void DecreaseY(int i)
        {
            if ((double)cinematicBars[0].Position.Y <= -100.0)
                return;

            double x1 = (double)cinematicBars[0].Position.X;
            double y1 = (double)cinematicBars[0].Position.Y - (double)i;
            cinematicBars[0].Position = new PointF((float)x1, (float)y1);

            double x2 = (double)cinematicBars[1].Position.X;
            double y2 = (double)cinematicBars[1].Position.Y + (double)i;
            cinematicBars[1].Position = new PointF((float)x2, (float)y2);
        }

        public void Draw()
        {
            if ((double)cinematicBars[0].Position.Y <= -100)
                return;
            cinematicBars[0].Draw();
            cinematicBars[1].Draw();
        }
    }
}
