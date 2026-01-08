using Diffuse.Common;
using System.Windows;
using System.Windows.Controls;

namespace Diffuse.Controls
{
    /// <summary>
    /// Interaction logic for UpscaleInputControl.xaml
    /// </summary>
    public partial class UpscaleInputControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpscaleInputControl"/> class.
        /// </summary>
        public UpscaleInputControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(nameof(Options), typeof(UpscaleInputOptions), typeof(UpscaleInputControl));


        public UpscaleInputOptions Options
        {
            get { return (UpscaleInputOptions)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }
    }
}
