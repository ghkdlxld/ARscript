using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class BodySourceView : MonoBehaviour //외부객체허용, (BodySourceView)_모든 스크립트가 상속받는 기본 클래스

{
    public Material BoneMaterial; //외부객체 접근 허용, Material 사용선언(변수BoneMaterial)

    public GameObject BodySourceManager;    //	외부객체 접근 허용, 새 게임오브젝트(BodySourceManager)를 생성

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    //Dictionary는 key 및 value 모음, key=단어 value=정의
    //TKey=ulog, TValue=GameObject key집합-> value집합으로 매핑
    //기본 초기 용량을 갖고 있고 키 형식에 대한 기본 같음 비교자를 사용하는 비어 있는 Dictionary<TKey,TValue> 클래스의 새 인스턴스를 초기화

    private BodySourceManager _BodyManager; //해당클래스내에서만 접근 BodySourceManager사용선언(변수_BodyManager)


    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    //Dictionary 선언 TKey=Kinect.JointType, TValue=Kinect.JointType
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },          //{왼발,왼발목}
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },          //{왼발목,왼무릎}
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },            //{왼무릎,왼엉덩이}
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },           //{왼엉덩이,가운데엉덩이}
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },        //{오른발,오른발목}
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },        //{오른발목,오른무릎}
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },          //{오른무릎,오른엉덩이}
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase }           //{오른엉덩이,가운데엉덩이}

        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },        //{왼손끝,왼손등}
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },          //{왼엄지,왼손등}
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },          //{왼손등,왼손목}
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },         //{왼손목,왼팔꿈치}
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },      //{왼팔꿈치,왼어깨}
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },  //{왼어깨,가운데어깨}
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },      //{오른손끝,오른손등}
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },        //{오른엄지,오른손등}
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },        //{오른손등,오른손목}
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },       //{오른손목,오른팔꿈치}
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },    //{오른팔꿈치,오른어깨}
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder }, //{오른어깨,가운데어깨}
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },          //{가운데어깨,허리}
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },      //{허리,가운데어깨}
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },          //{가운데어깨,목}
        { Kinect.JointType.Neck, Kinect.JointType.Head },                   //{목,머리}
    };
    
    void Update () //스크립트가 실행되는 동안 매 프레임마다 실행

    {
        if (BodySourceManager == null)  //만약 게임오브젝트 BodySourceManager가 비어있으면
        {
            return;                     //메소드종료
        }
        
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
    //이 스크립트가 포함된 게임오브젝트가 갖고 있는 컴포넌트 중에서 BodySourceManager컴포넌트를 추출하여 _BodyManager 변수에 저장해라

        if (_BodyManager == null)   //만약 _BodyManager변수가 비어있다면
        {
            return;                 //메소드종료
        }
        
        Kinect.Body[] data = _BodyManager.GetData();    //Body배열에 _BodyManager변수에 받은 data를 넣는다
        if (data == null)       //data가 비어있으면
        {
            return;             //메소드종료 
        }
        
        List<ulong> trackedIds = new List<ulong>(); 
        //64비트 부호없는 정수 List : trackedIds 선언
        foreach(var body in data)       //data의 body들을 가지고 아래를 반복
        {
            if (body == null)           //만약 body배열이 비어있다면
            {
                continue;               //반복을 계속해라
              }
                
            if(body.IsTracked)          //만약 body배열이 tracked되고 있다면
            {
                trackedIds.Add (body.TrackingId);   //trackedIds list의 끝에 body추적ID를 추가
            }
        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        // First delete untracked bodies

        //64비트 부호없는 정수형 _Bodies.Keys를 요소로 포함하는 List 선언

        foreach (ulong trackingId in knownIds)      // knownIds의 trackingId를 가지고 아래를 반복
        {
            if(!trackedIds.Contains(trackingId))    //만약 trackedIds리스트가 trackingId를 포함하지 않는다면
            {
                Destroy(_Bodies[trackingId]);       //객체제거
                _Bodies.Remove(trackingId);         //GameObject인 _Bodies에서 trackingId제거
            }
        }

        foreach(var body in data)        //data의 body들을 가지고 아래를 반복           
        {
            if (body == null)            //만약 body배열이 비어있다면
            {
                continue;                //계속
            }
            
            if(body.IsTracked)           //만약 body배열이 tracked되고 있다면
            {
                if(!_Bodies.ContainsKey(body.TrackingId))       //만약 GameObject인 _Bodies가  body.TrackingId key를 포함하지 않는다면
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);   //_Bodies[body.TrackingId]경로에 만들어라
                }
                
                RefreshBodyObject(body, _Bodies[body.TrackingId]);          //데이터 업데이트
            }
        }
    }
    
    private GameObject CreateBodyObject(ulong id)       //해당클래스내에서만 접근 GameObject인 ulog형식 id요소포함 객체생성
    {
        GameObject body = new GameObject("Body:" + id); //"Body:"+id 포함하는 객체:body 선언
        
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
            //jt = 가운데 엉덩이
            //jt가 오른쪽 엄지보다 작거나 같을동안 아래 구문 반복; jt 1씩 추가
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);   //GameObject인 jointObj -> cube기본 유형 생성
            
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();    //선 잇기
            lr.SetVertexCount(2); //선의 세그먼트 수를 설정
            lr.material = BoneMaterial; 
            lr.SetWidth(0.05f, 0.05f); //시작과 끝의 선의 가로 너비를 설정


            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);  //jointObj의 스케일을 (x,y,z) 0.3씩 늘린다
            jointObj.name = jt.ToString();  //object 이름반환
            jointObj.transform.parent = body.transform; //jointObj의 부모=body
        }
        
        return body;    //body 반환
    }
    
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject) //body, bodyObject 업데이트
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
            //jt=가운데엉덩이
            //jt가 오른쪽 엄지보다 작거나 같을동안 아래 구문 반복; jt 1씩 추가
        {
            Kinect.Joint sourceJoint = body.Joints[jt];     //sourceJoint = body의 jt요소
            Kinect.Joint? targetJoint = null;               //targetJoint 선언
            
            if(_BoneMap.ContainsKey(jt))                    //만약 _BoneMap이 jt key를 포함한다면
            {
                targetJoint = body.Joints[_BoneMap[jt]];    //targetJoint = _BoneMap의 jt
            }
            
            Transform jointObj = bodyObject.transform.FindChild(jt.ToString()); //이름이 jt.Tostring이었던 자식 = jointObj
            jointObj.localPosition = GetVector3FromJoint(sourceJoint); //jointObj위치 = sourceJoint
            
            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue)        //만약 targetJoint가 값을 가지면
            {
                lr.SetPosition(0, jointObj.localPosition);  //꼭짓점 위치설정->첫번째배열값 jointObj.localPosition(=sourceJoint)  154
                lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));  //꼭짓점 위치설정->두번째배열값 targetJoint(=body.Joints[_BoneMap[jt]]) 159
                lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
            }
            else
            {
                                            //그렇지않으면
            }
            {
                lr.enabled = false;         //비활성화
            }
        }
    }
    
    private static Color GetColorForState(Kinect.TrackingState state)   //색깔 정하기
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;     //트래킹 상태=green

        case Kinect.TrackingState.Inferred:
            return Color.red;       //지연 상태=red

        default:
            return Color.black;     //기본값=black
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);    //크기 *10 반환
    }
}
