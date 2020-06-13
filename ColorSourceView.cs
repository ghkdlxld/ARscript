using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class ColorSourceView : MonoBehaviour
{
    public GameObject ColorSourceManager; //ColorSourceManager라는 게임 오브젝트를 public으로 선언
    private ColorSourceManager _ColorManager;
    
    void Start ()
    {
        gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
        //offset값을 프레임마다 변환해서 Texture가 움직이는 것 처럼 보이게 만드는 코드, 텍스처의 배치 축척을 가져온다.
        //변경된 offset값을 material의 mainTextureOffset에 직접 넣어주거나 SetTextureOffset함수를 이용할 수 있다.


    }

    void Update()
    {
        if (ColorSourceManager == null) //ColorSourceManager가 비어있으면,
        {
            return; //반환
        }
        
        _ColorManager = ColorSourceManager.GetComponent<ColorSourceManager>(); //동일한 게임오브젝트(ColorSourceManager)가 갖고 있는 특정 다른 컴포넌트에 접근해야 할 경우 사용
        if (_ColorManager == null) //_ColorManager가 비어있으면,
        {
            return; //반환
        }
        
        gameObject.GetComponent<Renderer>().material.mainTexture = _ColorManager.GetColorTexture(); //_ColorManager에 GetColorTexture 가져오기..?
        //offset값을 프레임마다 변환해서 Texture가 움직이는 것처럼 보이게 만듦
        //변경된 offset값을 material의 mainTexture에 직접 넣어줌


    }
}