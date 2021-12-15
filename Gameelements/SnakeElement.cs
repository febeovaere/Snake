using System.Windows.Media;
using System.Windows.Shapes;

namespace Snake.Gameelements 
{
    class SnakeElement : Gameidentiteit
    {
        public SnakeElement(int size)
        {
            Rectangle rect = new Rectangle();
            rect.Width = size;
            rect.Height = size;
            rect.Fill = Brushes.Green;
            UIElement = rect;
        }
  

        public bool IsHead { get; set; }

      
    }
}
