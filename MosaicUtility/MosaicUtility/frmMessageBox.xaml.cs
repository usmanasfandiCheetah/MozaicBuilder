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

namespace MosaicUtility
{
    /// <summary>
    /// Interaction logic for frmMessageBox.xaml
    /// </summary>
    public partial class frmMessageBox : Window
    {
        public MessageBoxResult result = MessageBoxResult.OK;

        public frmMessageBox()
        {
            InitializeComponent();
        }

        public frmMessageBox(string message, string title, MessageBoxType type)
        {
            InitializeComponent();
            lblTitle.Content = title;
            lblMessage.Content = message;

            switch(type)
            {
                case MessageBoxType.Info:
                    border.BorderBrush = Brushes.Green;
                    break;
                case MessageBoxType.Error:
                    border.BorderBrush = Brushes.Red;
                    btnClose.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxType.Alert:
                    border.BorderBrush = Brushes.Red;
                    btnClose.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxType.Confirm:
                    border.BorderBrush = Brushes.Indigo;
                    btnOk.Content = "Yes";
                    btnCancel.Content = "No";
                    btnClose.Content = "Cancel";
                    break;
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            btnOk.Focus();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Cancel;
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.None;
            this.Close();
        }
    }
}
