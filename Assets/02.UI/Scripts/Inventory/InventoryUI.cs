using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InventoryUI : MonoBehaviour
{
    public GameObject UI;
    public GameObject ItemTemplate;
    public GameObject GlowCircle;
    public GameObject TipImg;
    public GameObject TipTitle;
    public GameObject TipWords;
    public Font Font;
    private bool IsOpen = false;
    int Focusx = 0;
    int FocusY = 0;

    public void Start()
    {
        Inventory.InitInv(this);
        UpdateGlowCircle();
    }
    public void Awake()
    {
        CloseInventory();
    }
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            if(IsOpen)
            {
                CloseInventory();
            }
            else if(PlayerManager.state != PlayerManager.StateCode.Stop)
            {
                OpenInventory();
            }
        }
    }
    private void OpenInventory()
    {
        IsOpen = true;
        Inventory.Current = Inventory.ItemType.Props;
        UI.SetActive(true);
        PlayerManager.state = PlayerManager.StateCode.Stop;
        //Enable UI
    }
    private void CloseInventory()
    {
        IsOpen = false;
        UI.SetActive(false);
        PlayerManager.state = PlayerManager.StateCode.Idle;
        //Disable UI
    }

    public void DestroyThis(GameObject d)
    {
        Destroy(d);
    }

    public void GetItem()
    {
        Inventory.GetItem(Inventory.ItemType.Props,0,1);
    }

    public void SetGlowCircle(int a)
    {
        Focusx = a % 4;
        FocusY = a / 4;
        UpdateGlowCircle();
    }

    public void IncGlowCircle(bool a)
    {
        Focusx += a ? 1 : -1;
        if(Focusx<0)
        {
            if(FocusY!=0)
            {
                Focusx = 3;
                FocusY--;
                UpdateGlowCircle();
            }
            else
            {
                Focusx = 0;
            }
        }
        else if(Focusx==4)
        {
            if(FocusY!=3)
            {
                Focusx = 0;
                FocusY++;
                UpdateGlowCircle();
            }
            else
            {
                Focusx = 3;
            }
        }
        else
        {
            UpdateGlowCircle();
        }
    }
    public void UpdateGlowCircle()
    {
        GlowCircle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-653.5f + Focusx * 137.0f, 195.0f - FocusY * 137.0f);
        int id = Inventory.GetItem(Focusx, FocusY);
        if(id==-1)
        {
            TipImg.GetComponent<RectTransform>().sizeDelta = new Vector2(0,0);
            TipTitle.GetComponent<Text>().text = "";
            TipWords.GetComponent<Text>().text = "";
            return;
        }
        Texture2D tex = Resources.Load<Texture2D>("ItemImage/" + Inventory.GetItemName(id,true));
        TipImg.GetComponent<RawImage>().texture = tex;
        float SizeRatio = (float)tex.height / tex.width;
        float ImgHeight = 370;
        float ImgWidth = 370;
        if (SizeRatio > 1)
        {
            ImgWidth /= SizeRatio;
        }
        else
        {
            ImgHeight *= SizeRatio;
        }
        TipImg.GetComponent<RectTransform>().sizeDelta = new Vector2(ImgWidth, ImgHeight);

        
        int index = id;
        while(id!=Inventory.Tips[index].id)
        {
            index--;
            if(index<=0)
            {
                Debug.Log("Item Does Not Exist :"+id.ToString());
                return;
            }
        }
        TipTitle.GetComponent<Text>().text = Inventory.Tips[index].name;
        TipWords.GetComponent<Text>().text = Inventory.Tips[index].description;
    }
    public GameObject CreateImg(string item,Vector2 Pos,int num)
    {
        GameObject Pack = new GameObject();
        Pack.transform.SetParent(UI.transform,false);
        Texture2D tex = Resources.Load<Texture2D>("ItemImage/"+item);
        float SizeRatio = (float)tex.height/tex.width;
        float ImgHeight=80;
        float ImgWidth=80;
        if(SizeRatio>1)
        {
            ImgWidth /= SizeRatio;
        }
        else
        {
            ImgHeight *= SizeRatio;
        }
        GameObject Create = new GameObject("Item", typeof(RawImage));
        Create.transform.position = Pos;//0.48333
        Create.transform.SetParent(Pack.transform,false);
        Create.GetComponent<RectTransform>().sizeDelta = new Vector2(ImgWidth,ImgHeight);
        RawImage img = Create.GetComponent<RawImage>();
        img.texture = tex;
        img.raycastTarget = false;

        GameObject Num = new GameObject("Num",typeof(Text));
        Num.transform.position = new Vector3(Pos.x + 21, Pos.y - 32, 0);
        Num.transform.SetParent(Pack.transform,false);
        //Num.GetComponent<RectTransform>().sizeDelta = new Vector2(150,150);
        Text text = Num.GetComponent<Text>();
        text.text = num.ToString().PadLeft(2,'0');
        text.color = Color.black;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Font;
        text.raycastTarget = false;
        
        return Pack;
    }

}
