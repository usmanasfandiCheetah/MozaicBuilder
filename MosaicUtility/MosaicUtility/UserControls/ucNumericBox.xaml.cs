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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MosaicUtility.UserControls
{
    /// <summary>
    /// Interaction logic for ucNumericBox.xaml
    /// </summary>
    public partial class ucNumericBox : UserControl
    {
        public event EventHandler OnValueChanged;
        public int Number
        {
            get { return Convert.ToInt32(txtNumber.Text); }
            set { txtNumber.Text = value.ToString(); }
        } 

        public ucNumericBox()
        {
            InitializeComponent();
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtNumber.Text))
                return;

            int number = Convert.ToInt32(txtNumber.Text);
            if (number == 0)
                return;

            number--;
            txtNumber.Text = number.ToString();
            OnValueChanged(null, null);
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtNumber.Text))
                txtNumber.Text = "0";

            int number = Convert.ToInt32(txtNumber.Text);
            if (number == 100)
                return;

            number++;
            txtNumber.Text = number.ToString();
            OnValueChanged(null, null);
        }
    }
}
