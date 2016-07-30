using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Nfc;

namespace m1card_test
{
    [Activity(Label = "m1_write", Icon = "@drawable/icon", LaunchMode = Android.Content.PM.LaunchMode.SingleTask)]
    [IntentFilter(
    new[] { NfcAdapter.ActionTechDiscovered },
    Categories = new[] { Intent.CategoryDefault, })]
    [MetaData("android.nfc.action.TECH_DISCOVERED", Resource = "@xml/tech_list")]
    public class m1_write : Activity
    {
        TextView mTV;
        PendingIntent mPendingIntent; 
        IntentFilter ndefDetected;
        IntentFilter[] intentF;
        String[][] techLists;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.m1_write);

            // Get our button from the layout resource,
            // and attach an event to it
            Intent Myintent = new Intent(this, GetType());//建立Intent內容
            Myintent.AddFlags(ActivityFlags.SingleTop);
            mPendingIntent = PendingIntent.GetActivity(this, 0, Myintent, 0);//對Intent進行描述
            ndefDetected = new IntentFilter(NfcAdapter.ActionTechDiscovered);

            intentF = new IntentFilter[] { ndefDetected };
            techLists = new string[][] { new string[] {"android.nfc.tech.NfcA",
                "android.nfc.tech.MifareClassic"}};//設置card類型

            Button button = FindViewById<Button>(Resource.Id.Back_Button);//建立Button
            mTV = FindViewById<TextView>(Resource.Id.textview);//建立TextView
            button.Click += delegate//Button Back點擊時返回MainActivity
            {
                Intent main_intent = new Intent(this, typeof(MainActivity));
                this.StartActivity(main_intent);
                Finish();
            };

            mTV.Text = "scan a tag";
            
            if (NfcAdapter.ActionTechDiscovered.Equals(this.Intent.Action))//自動啟動時由此呼叫OnNewIntent的內容
            {
                OnNewIntent(this.Intent);
            }


        }
        protected override void OnPause()
        {
            base.OnPause();
            NfcManager manager = (NfcManager)GetSystemService(NfcService);
            manager.DefaultAdapter.DisableForegroundDispatch(this);//關閉前台調度
        }

        protected override void OnResume()
        {
            base.OnResume();
            NfcManager manager = (NfcManager)GetSystemService(NfcService);
            manager.DefaultAdapter.EnableForegroundDispatch(this, mPendingIntent, intentF,techLists);//啟動前台調度
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            mTV.Text = "OnNewIntent";
            var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;//取得讀取到的Tag
            var mfc = Android.Nfc.Tech.MifareClassic.Get(tag);//將取得的Tag內容放入MifareClassic
            if (mfc==null)//確認是否有讀到Tag
            {
                mTV.Text="mfc==null";
            }

            if (mfc!=null)
            {
                //Toast.MakeText(this, "讀卡中", ToastLength.Long).Show();
                try {
                    mfc.Connect();
                    Boolean auth = false;
                    //第一區
                    short sectorAddress = 1;
                    //認證
                    byte[] KeyA = { 0xD3, 0xF7, 0xD3, 0xF7, 0xD3, 0xF7 };
                    byte[] KeyB = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                    byte[] theE = { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xFF, 0x07, 0x80, 0x40, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
                    //byte[] theE2 = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                    auth = mfc.AuthenticateSectorWithKeyA(sectorAddress,KeyA);//認證KEY A
                    auth = mfc.AuthenticateSectorWithKeyB(sectorAddress,KeyB);//認證KEY B

                    if (auth==true) {
                        mfc.WriteBlock(7, theE);//寫入block7
                        mfc.Close();
                        mTV.Text = "write OK!";
                    }
                    else
                    {
                        mTV.Text = "認證失敗，密碼錯誤";
                    }

                }
                catch { }
            }
        }
    }
}