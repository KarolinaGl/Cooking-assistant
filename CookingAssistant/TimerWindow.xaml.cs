using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CookingAssistant
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class TimerWindow : Window
    {
        int programmedTime;
        DateTime finishTime;
        bool isCounting;
        public TimerWindow()
        {
            InitializeComponent();
            this.isCounting = false;
            this.programmedTime = 1;
        }

        private void TimerScrollbarUp(object sender, RoutedEventArgs e)
        {

        }

        private void TimerScrollbarDown(object sender, RoutedEventArgs e)
        {
        }

        private void timerStartButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void timerStopButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
