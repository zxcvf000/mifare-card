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
    [Activity(Label = "m1_read",  Icon = "@drawable/icon", LaunchMode = Android.Content.PM.LaunchMode.SingleTask)]
    [IntentFilter(
    new[] {NfcAdapter.ActionTechDiscovered}, 
    Categories = new[] {Intent.CategoryDefault,})]
    [MetaData("android.nfc.action.TECH_DISCOVERED", Resource = "@xml/tech_list")]//當Tag讀入時須確認tech_list內的支援類型
    public class m1_read : Activity
    {
        TextView mTV;
        PendingIntent mPendingIntent;
        IntentFilter ndefDetected;
        IntentFilter[] intentF;
        String[][] techLists;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.m1_read);

            // Get our button from the layout resource,
            // and attach an event to it
            Intent Myintent = new Intent(this, GetType());//建立Intent內容
            Myintent.AddFlags(ActivityFlags.SingleTop);
            mPendingIntent = PendingIntent.GetActivity(this, 0, Myintent, 0);//對Intent進行描述
            ndefDetected = new IntentFilter(NfcAdapter.ActionTechDiscovered);
         
            intentF = new IntentFilter[] { ndefDetected };
            techLists = new string[][] {new string[] {"android.nfc.tech.NfcA",
                "android.nfc.tech.MifareClassic"}};//設置card類型

            Button button = FindViewById<Button>(Resource.Id.Back_Button);//建立Button
            mTV = FindViewById<TextView>(Resource.Id.textview);//建立TextView
            button.Click += delegate//Button Back點擊時返回MainActivity
            {
                Intent main_intent = new Intent(this, typeof(MainActivity));
                this.StartActivity(main_intent);
                Finish();
            };

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
            //mTV.Text = "OnNewIntent";
            var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;//取得讀取到的Tag
            var mfc = Android.Nfc.Tech.MifareClassic.Get(tag);//將取得的Tag內容放入MifareClassic
            if (mfc == null)
            {
                mTV.Text = "mfc==null";
            }

            if (mfc != null)
            {
                //Toast.MakeText(this, "檢測到卡片,讀卡中...", ToastLength.Long).Show();
                try
                {
                    mfc.Connect();
                    Boolean auth = false;
                    byte[] KeyA = { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA };
                    //byte[] KeyB = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                    short sectorAddress = 1;
                    auth = mfc.AuthenticateSectorWithKeyA(sectorAddress, KeyA);//認證KEY A
                    //auth = mfc.AuthenticateSectorWithKeyB(sectorAddress, KeyB);//認證KEY B
                    if (auth)
                    {
                        mTV.Text = BitConverter.ToString(mfc.ReadBlock(7));//讀取block7內容
                    }

                }
                catch { }
            }
        }
    }
}