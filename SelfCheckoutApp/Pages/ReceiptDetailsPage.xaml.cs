namespace SelfCheckoutApp.Pages
{
    public partial class ReceiptDetailsPage : ContentPage
    {
        public ReceiptDetailsPage(ReceiptResponse receipt)
        {
            InitializeComponent();
            BindingContext = receipt;
        }
    }
}
