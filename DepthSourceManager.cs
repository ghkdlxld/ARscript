using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class DepthSourceManager : MonoBehaviour
{   
    private KinectSensor _Sensor;
    private DepthFrameReader _Reader;
    private ushort[] _Data; //0 ~ 65,535

    public ushort[] GetData() //받아올 data 배열을 선언
    {
        return _Data; //데이터 반환
    }

    void Start () 
    {
        _Sensor = KinectSensor.GetDefault(); //PC에 연결된 KinectSensor의 Object를 _Sensor 변수에 저장

        if (_Sensor != null) //_Sensor 채워져 있으면
        {
            _Reader = _Sensor.DepthFrameSource.OpenReader(); //_Reader는 KinectSensor의 OpenReader()를 사용해 Depth Data를 Open
            _Data = new ushort[_Sensor.DepthFrameSource.FrameDescription.LengthInPixels]; //_Data는 새로운 Depth Data의 length당 pixel 정보를 저장
        }
    }
    
    void Update () 
    {
        if (_Reader != null) //_Reader 채워져 있으면
        {
            var frame = _Reader.AcquireLatestFrame(); //frame은 _Reader의 마지막 프레임
            if (frame != null) //frame이 채워져 있으면
            {
                frame.CopyFrameDataToArray(_Data); //Frame Data를 배열에 복사하여 원하는 형태로 사용
                frame.Dispose(); //현재 프레임 종료
                frame = null; //프레임 비어있음
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
