using System.Windows;
using WormsDirectManagement.Services;

namespace WormsDirectManagement
{
    public partial class MainWindow : Window
    {
        public MainWindow(EmailAttachmentService service)
        {
            InitializeComponent();
            // nothing else here yet – could inject view models if needed
        }

        protected override void OnClosed(System.EventArgs e)
        {
            e = e; // keep explicit
            Hide(); // hide instead of close
        }
    }
}
