using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;


public class GObject
{
    public GameObject glass;
    public GameObject leftFlowWater;
    public GameObject RightFlowWater;
    public float height;
    float radius;
    float total_time;
    float maxVolume;


    private float volume;
    public GObject(GameObject obj, float _height) { 
        glass = obj;
        leftFlowWater = obj.transform.Find("leftflowWater").gameObject;
        RightFlowWater = obj.transform.Find("rightflowWater").gameObject;
        leftFlowWater.SetActive(false);
        RightFlowWater.SetActive(false);


        Material mat_left = new Material(leftFlowWater.GetComponent<Image>().material);
        leftFlowWater.GetComponent<Image>().material = mat_left;
        Material mat_right = new Material(RightFlowWater.GetComponent<Image>().material);
        RightFlowWater.GetComponent<Image>().material = mat_right;

        height = _height; 
        radius = _height / 2.0f;
        maxVolume = 2 / 3.0f * Mathf.PI * radius * radius * radius;
        Volume = 0.0f;
        total_time = 0.0f;
        SetVolume(Volume);
    }

    public float Volume
        {
            get { return volume; }
            set { volume = value; }
        }

    public void SetParent(GameObject parent_obj)
    {
        glass.transform.SetParent(parent_obj.transform);
    }

    public void SetHeight(float h)
    {

        Material mat = glass.GetComponent<Image>().material; // 获取材质
        mat.SetFloat("_WaterHight", h); // 设置 Shader 中某 Color 变量的值
    }

    public void SetPosition(float posX, float PosY)
    {
        glass.GetComponent<RectTransform>().anchoredPosition = new Vector3(posX, PosY, 0f);
    }

    public void SetSize(float width, float height)
    {
        glass.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
    }

    public void SetVolume(float val)
    {
        volume = val;
        
        if(val > maxVolume)
        {
            leftFlowWater.SetActive(true);
            RightFlowWater.SetActive(true);

            leftFlowWater.GetComponent<Image>().material.SetFloat("_StartVal", total_time);
            RightFlowWater.GetComponent<Image>().material.SetFloat("_StartVal", total_time);
            Debug.Log($"total_time = {total_time}");
            SetHeight(1.0f);
        }
        else if(val < 0.01f)
        {
            SetHeight(0.0f);
        }
        else
        {
            var _h = GetHeightByVolumeAsync(val);
            SetHeight(_h);
        }
    }

    public float GetHeightByVolumeAsync(float volume)
    {
        float start = 0;
        float end = radius;
        float R = radius;
        float cur_h = 0;
        float mid = radius / 2;
        float res = Mathf.PI * mid * mid * (R - mid / 3.0f);
        while (Mathf.Abs(volume - res) > 100.0f && start < end )
        {
            mid = (start + end) / 2.0f;
            if (volume < res)
            {
                end = mid;
            }
            else
            {
                start = mid;
            }
            cur_h = (start + end) / 2.0f;
            res = Mathf.PI * cur_h * cur_h * (R - cur_h / 3.0f);
        }

        return cur_h / radius;
    }


    public void Refresh(float dt)
    {
        if(Volume > maxVolume)
        {
            total_time += dt;
        }
        SetVolume(Volume);
    }

}



public class Champagne : MonoBehaviour
{
    // Start is called before the first frame update

    GameObject glass;
    Vector2 startPos;
    Dictionary<int, Dictionary<int,GObject>> glassDic;
    int rowNum;
    int width;
    int height;
    int extraHeight;
    float totalVolume;
    float maxUnitVolume;
    public float speed;
    float total_time;


    private void Awake()
    {
        startPos = new Vector2(0, -50);
        rowNum = 5;
        width = height = 100;
        extraHeight = 30;
        glassDic = new Dictionary<int, Dictionary<int, GObject>>();
        speed = 1000000;
        totalVolume = 0.0f;
        float radius = height / 2;
        maxUnitVolume = 2 / 3.0f * Mathf.PI * radius * radius * radius;
        total_time = 0.0f;
    }

    void Start()
    {
        InitChampagneGlass();
    }


    void InitChampagneGlass()
    {
        for(int i =0; i<rowNum; ++i)
            for(int j=0; j<i+1; ++j)
            {
                glass = Resources.Load<GameObject>("prefab/Image1");
                glass = Instantiate<GameObject>(glass);
                Material mat = new Material(glass.GetComponent<Image>().material);
                glass.GetComponent<Image>().material = mat;

                float posY = startPos.y - (height / 2.0f + extraHeight) * i;
                float posX = startPos.x - i * (3 / 4.0f * width) + j * (3 / 2.0f * width);
                GObject gObject = new GObject(glass, height);
                gObject.SetParent(gameObject);
                gObject.SetPosition(posX, posY);
                gObject.SetSize(width, height);
                if (! glassDic.ContainsKey(i))
                {
                    var cur_dic = new Dictionary<int, GObject>() { { j, gObject } };
                    glassDic[i] = cur_dic;
                }
                else
                {
                    var cur_dic = glassDic[i];
                    cur_dic.Add(j, gObject);
                }

            }
    }

    // Update is called once per frame
    void Update()
    {
        float addVal = Time.deltaTime * speed;
        totalVolume += addVal;
        glassDic[0][0].Volume = totalVolume;
        glassDic[0][0].Refresh(Time.deltaTime);
        List<float> curList = new List<float>() { totalVolume };
        for (int i =1; i<=rowNum; ++i)
        {
            List<float> nextList = new List<float>();
            for(int k=0; k<i+1; ++k)
            {
                nextList.Add(0);
            }
            for(int j = 0; j < i; ++j)
            {
                float num = curList[j];
                float _addVal = Mathf.Max(0, num - maxUnitVolume) / 2.0f;
                if(glassDic.ContainsKey(i))
                {
                    nextList[j] += _addVal;
                    glassDic[i][j].Volume = nextList[j];
                    glassDic[i][j].Refresh(Time.deltaTime);
                    nextList[j + 1] += _addVal;
                    glassDic[i][j + 1].Volume = nextList[j + 1];
                    glassDic[i][j + 1].Refresh(Time.deltaTime);
                }
                
            }
            curList = nextList;
        }
    }
    
}
