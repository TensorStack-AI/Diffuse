using Diffuse.Common;
using System.Windows;
using System.Windows.Controls;

namespace Diffuse.Controls
{
    /// <summary>
    /// Interaction logic for ExtractInputControl.xaml
    /// </summary>
    public partial class ExtractInputControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractInputControl"/> class.
        /// </summary>
        public ExtractInputControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(nameof(Options), typeof(ExtractInputOptions), typeof(ExtractInputControl));
        public static readonly DependencyProperty ExtractorTypeProperty = DependencyProperty.Register(nameof(ExtractorType), typeof(ExtractorType), typeof(ExtractInputControl));


        public ExtractInputOptions Options
        {
            get { return (ExtractInputOptions)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }

        public ExtractorType ExtractorType
        {
            get { return (ExtractorType)GetValue(ExtractorTypeProperty); }
            set { SetValue(ExtractorTypeProperty, value); }
        }
    }
}
