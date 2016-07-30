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
            SetContentView(Resource.Layout.Main); //將動作輸出給畫面

            Button button = FindViewById<Button>(Resource.Id.MyButton); //宣告按鈕
            Button button2 = FindViewById<Button>(Resource.Id.MyButton2);

            button.Click += delegate //點擊時啟動m1_read
            {
                Intent read_intent = new Intent(this, typeof(m1_read)); 
                this.StartActivity(read_intent);  
                Finish();
            };
            button2.Click += delegate//點擊時啟動m1_write
            {
                Intent read_intent = new Intent(this, typeof(m1_write));
                this.StartActivity(read_intent);
                Finish();
            };

        }
    }
}

