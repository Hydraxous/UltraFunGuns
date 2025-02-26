using UltraFunGuns.Util;
using UnityEngine;
using UnityEngine.UI;

namespace UltraFunGuns
{
    public class UFGInformationPage : MonoBehaviour
    {
        private Button bugReportButton;

        private Button githubButton;
        private Button kofiButton;
        private Button thunderstoreButton;
        private Button discordButton;
        private Button twitterButton;
        private Button youtubeButton;

        private Image pfpImage;
        private Text changelogHeaderText;
        private Text changelogBodyText;

        private static Sprite pfpSprite;


        private void Start()
        {
            pfpImage = transform.LocateComponent<Image>("Image_Profile");
            LoadPFP();

            changelogHeaderText = transform.LocateComponent<Text>("Text_ChangelogHeader");
            changelogHeaderText.text = $"Changelog ({ConstInfo.VERSION})";

            changelogBodyText = transform.LocateComponent<Text>("Text_ChangelogBody");
            changelogBodyText.text = ConstInfo.CHANGELOG;

            bugReportButton = transform.LocateComponent<Button>("Button_BugReport");
            bugReportButton.SetClickAction(() =>
            {
                Application.OpenURL(ConstInfo.GITHUB_BUG_REPORT_URL);
            });

            githubButton = transform.LocateComponent<Button>("Button_Github");
            githubButton.SetClickAction(() =>
            {
                Application.OpenURL(ConstInfo.GITHUB_URL);
            });

            kofiButton = transform.LocateComponent<Button>("Button_Kofi");
            kofiButton.SetClickAction(() =>
            {
                Application.OpenURL(ConstInfo.KOFI_URL);
            });

            thunderstoreButton = transform.LocateComponent<Button>("Button_Thunderstore");
            thunderstoreButton.SetClickAction(() =>
            {
                Application.OpenURL(ConstInfo.THUNDERSTORE_URL);
            });


            discordButton = transform.LocateComponent<Button>("Button_Discord");
            discordButton.SetClickAction(() =>
            {
                Application.OpenURL(ConstInfo.DISCORD_URL);
            });

            twitterButton = transform.LocateComponent<Button>("Button_Twitter");
            twitterButton.SetClickAction(() =>
            {
                Application.OpenURL(ConstInfo.TWITTER_URL);
            });

            youtubeButton = transform.LocateComponent<Button>("Button_Youtube");
            youtubeButton.SetClickAction(() =>
            {
                Application.OpenURL(ConstInfo.YOUTUBE_URL);
            });
        }
        
        private void LoadPFP()
        {
            if(pfpSprite != null)
            {
                if(pfpImage != null)
                    pfpImage.sprite = pfpSprite;
                return;
            }

            TextureLoader.DownloadTexture(ConstInfo.PROFILE_PICTURE_URL, (r) =>
            {
                if (r.Success)
                {
                    pfpSprite = Sprite.Create(r.Texture, new Rect(0, 0, r.Texture.width, r.Texture.height), Vector2.zero);

                    if (pfpImage != null)
                    {
                        pfpImage.sprite = pfpSprite;
                    }
                }
                else
                {
                    UltraFunGuns.Log.LogError("Error retrieving picture...");
                }
            });
        }
    }
}
