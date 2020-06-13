using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class ColorSourceManager : MonoBehaviour //모든 유니티 스크립트가 상속 받는 클래스

{
    public int ColorWidth { get; private set; } //ColorWidth를 저장할 정수타입 변수 선언
    public int ColorHeight { get; private set; } //ColorHeight를 저장할 정수타입 변수 선언 //Get은 데이터 반환, Set은 데이터 할당

    private KinectSensor _Sensor; //SDK에서 생성한 KinectSensor의 Object의 주소를 저장하기 위한 변수 선언
    private ColorFrameReader _Reader; //KinectSensor의 Color Data를 받기위해서 ColorFrameReader를 선언
    private Texture2D _Texture;
    private byte[] _Data; //프로그램에 할당된 메모리의 양(0 ~ 255)

    public Texture2D GetColorTexture() //GetColor메소드는 플러그 인이 직접 그릴 수 있도록 홈 스크린 색 구성표에 대한 정보를 제공
    {
        return _Texture;
    }
    
    void Start()
    {
        _Sensor = KinectSensor.GetDefault(); //PC에 연결된 KinectSensor의 Object를 _Sensor 변수에 저장

        if (_Sensor != null) //_Sensor가 채워져 있으면
        {
            _Reader = _Sensor.ColorFrameSource.OpenReader(); //_Reader는 KinectSensor의 OpenReader()를 사용해 Color Data를 Open

            var frameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba); //Kinect 모든 Data는 FrameDescription Class을 사용 
            //KinectSensor에서 오는 Image Frame의 속성을 생성/저장
            ColorWidth = frameDesc.Width;
            ColorHeight = frameDesc.Height;
            
            _Texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.RGBA32, false); //new는 해당 유형의 생성자 중 하나를 호출 //Width, Height, ColorFormat, mipmap여부
            _Data = new byte[frameDesc.BytesPerPixel * frameDesc.LengthInPixels]; //1픽셀에 해당하는 바이트 수*1픽셀에 해당하는 길이

            if (!_Sensor.IsOpen) //_Sensor가 Open 되어 있지 않으면
            {
                _Sensor.Open(); //Open
            }
        }
    }
    
    void Update () 
    {
        if (_Reader != null) //_Reader가 채워져있으면
        {
            var frame = _Reader.AcquireLatestFrame(); //변수 frame = _Reader의 마지막 프레임

            if (frame != null) //frame 채워져있으면
            {
                frame.CopyConvertedFrameDataToArray(_Data, ColorImageFormat.Rgba); //색상 정보를 보유하는 배열 받기
                 _Texture.LoadRawTextureData(_Data); //수치화된 TextureData 로드
                _Texture.Apply(); //Texture 인자를 하나로 묶어 배열로 만들어 넣는 것

                frame.Dispose(); //해당 인스터스가 종료될 때 리소스를 해제 //현재 프레임 종료
                frame = null; //변수를 초기화
            }
        }
    }

    void OnApplicationQuit() //어플리케이션을 종료하는 순간에 처리할 행동
    {
        if (_Reader != null) 
        {
            _Reader.Dispose();
            _Reader = null;
        }
        
        if (_Sensor != null) 
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }
            
            _Sensor = null;
        }
    }
}
