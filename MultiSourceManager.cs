using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class MultiSourceManager : MonoBehaviour
{   //MultiSourceManager를 저장할 변수 선언
    public int ColorWidth { get; private set; } //ColorWidth를 저장할 정수타입 변수 선언
    public int ColorHeight { get; private set; }//ColorHeight를 저장할 정수타입 변수 선언 
                                                //Get은 데이터 반환, Set은 데이터 할당

    private KinectSensor _Sensor;               //SDK에서 생성한 KinectSensor의 Object의 주소를 저장하기 위한 변수 선언
    private MultiSourceFrameReader _Reader;     //KinectSensor의 Color Data를 받기위해서 ColorFrameReader를 선언
    private Texture2D _ColorTexture;
    private ushort[] _DepthData;                //ushort(부호없는 16 비트) 배열 변수: _DepthData
    private byte[] _ColorData;                  //byte 배열 변수: _ColorData

    public Texture2D GetColorTexture()    //GetColor메소드는 플러그 인이 직접 그릴 수 있도록 홈 스크린 색 구성표에 대한 정보를 제공
    {
        return _ColorTexture;             //_ColorTexture 반환
    }
    
    public ushort[] GetDepthData()
    {
        return _DepthData;
    }

    void Start ()   //스크립트가 실행될 때 1회 실행
    {
        _Sensor = KinectSensor.GetDefault();        //PC에 연결된 KinectSensor의 Object 를 kinectSensor 클래스 변수에 저장

        if (_Sensor != null)        //만약 _Sensor 가 비어있지 않다면
        {
            _Reader = _Sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);
            //_Reader : multisourceframereader(color, depth)

            var colorFrameDesc = _Sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            //Kinect 모든 Data는 FrameDescription Class을 사용 
            //KinectSensor에서 오는 Image Frame의 속성을 생성/저장

            ColorWidth = colorFrameDesc.Width;      
            ColorHeight = colorFrameDesc.Height;
            
            _ColorTexture = new Texture2D(colorFrameDesc.Width, colorFrameDesc.Height, TextureFormat.RGBA32, false);
            //new는 해당 유형의 생성자 중 하나를 호출
            //Width, Height, ColorFormat, mipmap여부 
            //(3차원 그래픽스의 텍스처 매핑 분야에서, 밉맵(mipmap)은 렌더링 속도를 향상시키기 위한 목적으로 기본 텍스처와 이를 연속적으로 미리 축소시킨 텍스처들로 이루어진 비트맵 이미지의 집합)

            _ColorData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];
            //_ColorData 변수 : {1픽셀에 해당하는 바이트 수*1픽셀에 해당하는 길이} 를 요소로 하는 바이트 배열 

            var depthFrameDesc = _Sensor.DepthFrameSource.FrameDescription;

            _DepthData = new ushort[depthFrameDesc.LengthInPixels];
            //_Data는 새로운 Depth Data의 length당 pixel 정보를 저장


            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }
    
    void Update () 
    {
        if (_Reader != null) 
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                var colorFrame = frame.ColorFrameReference.AcquireFrame();
                if (colorFrame != null)
                {
                    var depthFrame = frame.DepthFrameReference.AcquireFrame();
                    if (depthFrame != null)
                    {
                        colorFrame.CopyConvertedFrameDataToArray(_ColorData, ColorImageFormat.Rgba);
                        _ColorTexture.LoadRawTextureData(_ColorData);
                        _ColorTexture.Apply();
                        
                        depthFrame.CopyFrameDataToArray(_DepthData);
                        
                        depthFrame.Dispose();
                        depthFrame = null;
                    }
                
                    colorFrame.Dispose();
                    colorFrame = null;
                }
                
                frame = null;
            }
        }
    }
    
    void OnApplicationQuit()
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
