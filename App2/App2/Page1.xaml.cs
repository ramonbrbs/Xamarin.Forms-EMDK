using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace App2
{
    public partial class Page1 : ContentPage
    {
        public Page1()
        {
            InitializeComponent();
            MessagingCenter.Subscribe<Application, string>(App.Current, "InitialScan", (sender, args) => {

                Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("BarCode", args, "Ok");
                });

            });
        }

    }
}
