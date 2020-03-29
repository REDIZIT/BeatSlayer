using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MenuTrackButton : MonoBehaviour {

    MenuScript_v2 menu2;

    public Text nameText, authorText, lenText, userTrackPathText;
    public TrackItemClass item;
    public UserTrackClass useritem;

    public GameObject downloadedImg;
    public Image img;

    public GameObject statistics;
    public Text downloads, plays, likes, dislikes;
    public Slider rateSlider;

    public GameObject creatorInfo;

    public bool needDownloading;/*,canDeleted;*/
    public string fullname;
    public string author
    {
        get
        {
            if (fullname.Contains("-")) return fullname.Split('-')[0];
            else return "Unknown";
        }
    }
    public string name
    {
        get
        {
            if (fullname.Contains("-")) return fullname.Split('-')[1];
            else return fullname;
        }
    }
    public string path; // if user track

    public bool isAutoTrack;
    public string source;

    public GameObject isNewPanel;

    bool setup;
    private void Update()
    {
        if (setup) setup = false;
    }

    public void Setup(MenuScript_v2 menu2, TrackItemClass item, string author, string label, int mins, int secs, bool _isUserTrack, string source, string creator) // For each item
    {
        this.menu2 = menu2;
        this.item = item;

        setup = true;

        fullname = author + "-" + label;

        authorText.text = author + " • " + mins + ":" + (secs < 10 ? "0" + secs : secs.ToString());
        nameText.text = label;
        //lenText.text = mins + ":" + (secs < 10 ? "0" + secs : secs.ToString());
        isAutoTrack = _isUserTrack;

        //isNewPanel.SetActive(isNew);
        isNewPanel.SetActive(false);

        downloads.text = item.downloads.ToString();
        plays.text = item.plays.ToString();
        likes.text = item.likes.ToString();
        dislikes.text = item.dislikes.ToString();

        rateSlider.maxValue = 1;
        rateSlider.minValue = -1;
        float min = Mathf.Min(item.likes, item.dislikes);
        float max = Mathf.Max(item.likes, item.dislikes);
        float val = 1 - (max == 0 ? 1 : min / max);
        rateSlider.value = item.likes > item.dislikes ? val : -val;

        this.source = source;
        //Debug.Log("Min: " + min + " Max: " + 0 + " => " + (max == 0 ? "Nope" : "" + (min / max)));

        if(creator == "")
        {
            creatorInfo.SetActive(false);
        }
        else
        {
            creatorInfo.SetActive(true);
            creatorInfo.GetComponentInChildren<Text>().text = creator;
        }
    }
    public void Setup(UserTrackClass item)
    {
        useritem = item;

        setup = true;

        fullname = item.trackname;

        path = item.path;
        string folderName = Path.GetDirectoryName(path);
        string folderParent = Directory.GetParent(folderName).FullName;
        string[] folderPathSplitted = folderName.Replace(@"\", "/").Split('/');
        userTrackPathText.text = folderParent.Replace("/", " / ").Replace(@"\", @" / ") + "/ <b>" + folderPathSplitted[folderPathSplitted.Length - 1] + "</b>";

        //Debug.Log(fullname + " -> " + item.author + "-" + item.name);
        authorText.text = item.author;
        nameText.text = item.name;
        //lenText.text = mins + ":" + (secs < 10 ? "0" + secs : secs.ToString());
        //lenText.gameObject.SetActive(false);
        isAutoTrack = true;

        isNewPanel.SetActive(item.isNew);

        statistics.SetActive(false);
    }
    public void PostSetup(bool downloaded) // Only for available
    {
        needDownloading = !downloaded;
        downloadedImg.SetActive(downloaded);
    }

    public void Rescale(bool isPortrait)
    {
        // Disabled in portrait mode
        statistics.SetActive(!isPortrait && !isAutoTrack);
        userTrackPathText.gameObject.SetActive(!isPortrait && isAutoTrack);
    }


    public void SetImage(string fullpath)
    {
        img.sprite = LoadNewSprite(fullpath);
    }
	public void TrackItemClicked()
	{
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MenuScript_v2>().OnTrackItemClicked(this);
	}
    public void OnFavouriteClicked()
    {
        //item.favourite = favouriteToggle.isOn;
        //menu2.database.SaveDB();
    }



    public Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
    {
        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference
        Texture2D SpriteTexture = LoadTexture(FilePath);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);

        return NewSprite;
    }

    public Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }
}
