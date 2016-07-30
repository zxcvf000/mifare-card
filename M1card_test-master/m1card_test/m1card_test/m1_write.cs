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
    /*將LaunchMOde設置為SingleTask，代表同時只會有一個實例，如果該實例已在stack頂則會在進入此activity時呼叫OnNewIntent而不是OnCreate
      將IntentFilter的Action設置為NfcAdapter.ActionTechDiscovered
                      Categories設置為Intent.CategoryDefault
                      同時NfcAdapter.ActionTechDiscovered會根據tech_list的內容來進行匹配
         */
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
            Myintent.AddFlags(ActivityFlags.SingleTop);  //加入一個flags，若activity已在stack頂，不會啟動一個新的
            mPendingIntent = PendingIntent.GetActivity(this, 0, Myintent, 0); //宣告一個PendingIntent，當特定條件觸發會傳myintent出去
            ndefDetected = new IntentFilter(NfcAdapter.ActionTechDiscovered); //宣告一個IntentFilter

            intentF = new IntentFilter[] { ndefDetected }; //宣告一個IntentFilter陣列
            techLists = new string[][] { new string[] {"android.nfc.tech.NfcA",
                "android.nfc.tech.MifareClassic"}};  //宣告協定，作為卡片過濾使用，其判斷模式與tech_list.xml相同，在此用於ForegroundDispatch

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
            //啟動前台調度，若在讀卡前已進入此activity，則會在進行android的IntentFilter匹配前，先透過前台調度內宣告的條件進行判斷
            //若傳出的intent符合intentF的條件，也符合techLists的協定，則會從本activity傳mPendingIntent給自己
            //由於此activity是設置singleTask模式，會直接呼叫OnNewIntent
            manager.DefaultAdapter.EnableForegroundDispatch(this, mPendingIntent, intentF,techLists);
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
                    //auth = mfc.AuthenticateSectorWithKeyB(sectorAddress,KeyB);//認證KEY B

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