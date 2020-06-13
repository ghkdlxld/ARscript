using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class BodySourceManager : MonoBehaviour //외부객체허용 (BodySourceManager)_모든 스크립트가 상속받는 기본 클래스
{
    private KinectSensor _Sensor;       //해당클래스내에서만 접근 KinectSensor사용선언(변수_Sensor)
    private BodyFrameReader _Reader;    //KinectSensor의 Body Data를 받기위해서 BodyFrameReader를 선언(변수_Reader)
    private Body[] _Data = null;        //Body 배열 생성 
    
  public Body[] GetData()       //Body배열에 Data반환
    {
        return _Data;          
    }  
    

    void Start ()  //스크립트가 실행될 때 1회 실행
    {
        _Sensor = KinectSensor.GetDefault();
        //PC에 연결된 KinectSensor의 Object 를 kinectSensor 클래스 변수에 저장
        
        //현재 V2는 Multi Kinect 연결 구성이 지원되지 않습니다.
        //향후 SDK 패치를 통해서 Multi Kinect 지원을 하겠다고 MS에서는 이야기 하고 있습니다.

        if (_Sensor != null)        //만약 _Sensor 가 비어있지 않다면
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader(); //KinectSensor의 OpenReader()를 사용해 BodyFrameData 를 Open

            if (!_Sensor.IsOpen)    //만약 _Sensor가 열려있지 않으면
            {
                _Sensor.Open();     //_Sensor를 Open
            }
        }   
    }
    
    void Update ()      //스크립트가 실행되는 동안 매 프레임마다 실행
    {
        if (_Reader != null)    //_Reader 변수가 비어있지 않다면
        {
            var frame = _Reader.AcquireLatestFrame();   // frame변수에 마지막 Frame data저장
            if (frame != null)      //frame변수가 비어있지 않다면 if문 실행
            {
                if (_Data == null)  //만약 _Data변수(Body배열)가 비어있다면
                {
                    _Data = new Body[_Sensor.BodyFrameSource.BodyCount];    
                    //새 Body배열에 body count(시스템에서 tracking 할 수 있는 body 수 및 이러한 본문을 저장하는 데 사용해야하는 컬렉션의 크기)를 가져옴
                }
                
                frame.GetAndRefreshBodyData(_Data); //새로 고친 신체 데이터를 가져옴

                frame.Dispose();    //현재의 frame만 종료
                frame = null;       //frame변수를 비운다
            }
        }    
    }
    
    void OnApplicationQuit()    //어플종료
    {
        if (_Reader != null)    //만약 Reader가 비어있지않다면
        {
            _Reader.Dispose();  //종료
            _Reader = null;     //변수 비우기
        }
        
        if (_Sensor != null)    //센서가 비어있지않다면
        {
            if (_Sensor.IsOpen) //센서가 열려있다면
            {
                _Sensor.Close();//센서를 닫는다
            }
            
            _Sensor = null;     //Sensor변수를 비운다
        }
    }
}
