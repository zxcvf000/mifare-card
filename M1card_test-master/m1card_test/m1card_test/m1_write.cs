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
            Intent Myintent = new Intent(this, GetType());
            Myintent.AddFlags(ActivityFlags.SingleTop);
            mPendingIntent = PendingIntent.GetActivity(this, 0, Myintent, 0);
            ndefDetected = new IntentFilter(NfcAdapter.ActionTechDiscovered);

            intentF = new IntentFilter[] { ndefDetected };
            techLists = new string[][] { new string[] {"android.nfc.tech.NfcA",
                "android.nfc.tech.MifareClassic"}};

            Button button = FindViewById<Button>(Resource.Id.Back_Button);
            mTV = FindViewById<TextView>(Resource.Id.textview);
            button.Click += delegate
            {
                Intent main_intent = new Intent(this, typeof(MainActivity));
                this.StartActivity(main_intent);
                Finish();
            };

            mTV.Text = "scan a tag";
            
            if (NfcAdapter.ActionTechDiscovered.Equals(this.Intent.Action))
            {
                OnNewIntent(this.Intent);
            }


        }
        protected override void OnPause()
        {
            base.OnPause();
            NfcManager manager = (NfcManager)GetSystemService(NfcService);
            manager.DefaultAdapter.DisableForegroundDispatch(this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            NfcManager manager = (NfcManager)GetSystemService(NfcService);
            manager.DefaultAdapter.EnableForegroundDispatch(this, mPendingIntent, intentF,techLists);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            mTV.Text = "OnNewIntent";
            var tag = intent.GetParcelableExtra(NfcAdapter.ExtraTag) as Tag;
            var mfc = Android.Nfc.Tech.MifareClassic.Get(tag);
            if(mfc==null)
            {
                mTV.Text="mfc==null";
            }

            if (mfc!=null)
            {
                //Toast.MakeText(this, "Ū�d��", ToastLength.Long).Show();
                try {
                    mfc.Connect();
                    Boolean auth = false;
                    //�Ĥ@��
                    short sectorAddress = 1;
                    //�{��
                    byte[] KeyA = { 0xD3, 0xF7, 0xD3, 0xF7, 0xD3, 0xF7 };
                    byte[] KeyB = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                    byte[] theE = { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xFF, 0x07, 0x80, 0x40, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
                    byte[] theE2 = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                    auth = mfc.AuthenticateSectorWithKeyA(sectorAddress,KeyA);
                    auth = mfc.AuthenticateSectorWithKeyB(sectorAddress,KeyB);

                    if (auth==true) {
                        mfc.WriteBlock(7, theE);
                        mfc.Close();
                        mTV.Text = "write OK!";
                    }
                    else
                    {
                        mTV.Text = "�{�ҥ��ѡA�K�X���~";
                    }

                }
                catch { }
            }
        }
    }
}