using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace m1card_test
{
    [Activity(Label = "m1card_test", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);
            Button button2 = FindViewById<Button>(Resource.Id.MyButton2);

            button.Click += delegate
            {
                Intent read_intent = new Intent(this, typeof(m1_read));
                this.StartActivity(read_intent);
                Finish();
            };
            button2.Click += delegate
            {
                Intent read_intent = new Intent(this, typeof(m1_write));
                this.StartActivity(read_intent);
                Finish();
            };

        }
    }
}

