using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.HSNR.AccountManager
{
    class AccountInfo
    {

        public string id;
        public string username;
        public string password;
        public int indexServer;
        public string checkCPU;
        public bool isLowCPU => checkCPU == "1";

        public static AccountInfo myAccount;
        public static bool canGetAccount()
        {
            return myAccount != null && Main.isPC;
        }

        public static void loadAccount()
        {
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length >= 2)
                {
                    infoAccount = args[1].Split('|');
                    Debug.LogError("Start load account : " + args[1]);
                    if (infoAccount.Length >= 3)
                    {
                        myAccount = new AccountInfo();
                        myAccount.id = infoAccount[0];
                        myAccount.username = infoAccount[1];
                        myAccount.password = infoAccount[2];
                        myAccount.checkCPU = infoAccount[3];
                        myAccount.indexServer = int.Parse(infoAccount[4]);
                        if (myAccount.isLowCPU)
                        {
                            //  turnONLowCPU();
                        }
                        Debug.LogError("Finish load account : " + infoAccount[1] + " - " + infoAccount[2]);
                    }
                }
            }
            catch (Exception e)
            {
                GameCanvas.startOK("Có lỗi xảy ra khi load Account từ AccountManager! \n" +
                    e.ToString(), 8885, null);
            }
        }

        public static string[] infoAccount;
        private static bool isLoadAccount;
        private static bool isLogin;

        static int status = 0;
        public static void Login()
        {
            if (!canGetAccount()) return;

            if (Rms.loadRMSInt("svselect") != myAccount.indexServer && status == 0)
            {
                Rms.saveRMSInt("svselect", myAccount.indexServer);
                ServerListScreen.ipSelect = myAccount.indexServer;
                GameCanvas.serverScreen.selectServer();
                status = 1;
                return;
            }
            else if (status == 0)
            {
                status = 1;
                return;
            }
            if (status == 1)
            {
                GameCanvas.loginScr ??= new LoginScr();
                GameCanvas.loginScr.doLogin();
                GameCanvas.loginScr.switchToMe();
                status = 2;
            }
        }
        private static long lastOpenGame = mSystem.currentTimeMillis();
        public static void update(mScreen screen)
        {
            if (screen is ServerListScreen && ServerListScreen.loadScreen)
            {
                if (!isLoadAccount)
                {
                    isLoadAccount = true;
                    loadAccount();
                }
                else if (status < 2 && isLoadAccount && canGetAccount() && mSystem.currentTimeMillis() - lastOpenGame >= 1000)
                {
                    lastOpenGame = mSystem.currentTimeMillis();
                    isLogin = true;
                    Login();
                }
            }
        }


        public static void turnONLowCPU()
        {
            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 0;
            //  QualitySettings.SetQualityLevel(0);
            Time.fixedDeltaTime = 0.05f;
            QualitySettings.particleRaycastBudget = 4; // Giảm ngân sách raycast
            QualitySettings.shadows = ShadowQuality.Disable; // Tắt bóng đổ
            QualitySettings.lodBias = 0.3f; // Giảm chi tiết
            QualitySettings.pixelLightCount = 0; // Giảm đèn pixel
            QualitySettings.globalTextureMipmapLimit = 2; // Giảm độ phân giải texture
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            QualitySettings.asyncUploadTimeSlice = 1;
            QualitySettings.asyncUploadBufferSize = 2;
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }
        public static void turnONHIGHCPU()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;
            QualitySettings.SetQualityLevel(5);
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
        }
    }
}
