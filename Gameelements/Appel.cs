using System.Windows.Media;
using System.Windows.Shapes;

namespace Snake.Gameelements
{
    class Appel : Gameidentiteit
    {
        public Appel(int size)
        {
            Rectangle rect = new Rectangle();
            rect.Width = size;
            rect.Height = size;
            rect.Fill = Brushes.Red;
            UIElement = rect;
        }

        public override bool Equals(object obj)
        {
            Appel appel = obj as Appel;
            if (appel != null)
            {
                return X == appel.X && Y == appel.Y;
            }
            else
            {
                return false;
            }  
        }
    }
}
