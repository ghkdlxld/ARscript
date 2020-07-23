using UnityEngine;
using System.Collections;
using Windows.Kinect;

public enum DepthViewMode //열거형 class
{
    SeparateSourceReaders,
    MultiSourceReader,
}

public class DepthSourceView : MonoBehaviour //모든 유니티 스크립트가 상속 받는 클래스
{
    public DepthViewMode ViewMode = DepthViewMode.SeparateSourceReaders; 
    
    public GameObject ColorSourceManager; //현재의 컴포넌트가 첨부된 게임 오브젝트를 나타냄
    public GameObject DepthSourceManager;
    public GameObject MultiSourceManager;
    
    private KinectSensor _Sensor;
    private CoordinateMapper _Mapper;
    private Mesh _Mesh;
    private Vector3[] _Vertices;
    private Vector2[] _UV;
    private int[] _Triangles;
    
    // Only works at 4 right now
    private const int _DownsampleSize = 4; //상수선언(int : 4byte 정수)
    private const double _DepthScale = 0.1f; //double : 8byte 실수(f=float)
    private const int _Speed = 50; //깊이측정 속도
    
    private MultiSourceManager _MultiManager;
    private ColorSourceManager _ColorManager;
    private DepthSourceManager _DepthManager;

    //스크립트가 실행되면 _Sensor에 Object 저장, 좌표 매핑, Mesh 생성
    void Start()
    {
        _Sensor = KinectSensor.GetDefault(); //PC에 연결된 KinectSensor의 Object를 _Sensor 변수에 저장
        if (_Sensor != null) //_Sensor가 채워져있으면
        {
            _Mapper = _Sensor.CoordinateMapper; //좌표 매퍼 준비
            var frameDesc = _Sensor.DepthFrameSource.FrameDescription; //KinectSensor에서 오는 Depth Frame의 속성을 생성/저장

            // Downsample to lower resolution
            CreateMesh(frameDesc.Width / _DownsampleSize, frameDesc.Height / _DownsampleSize); //Mesh 생성

            if (!_Sensor.IsOpen) //_Sensor가 열려있지 않으면
            {
                _Sensor.Open(); //열기
            }
        }
    }

    //매쉬가 생성되면 반복문 실행
    void CreateMesh(int width, int height) //매쉬 생성
    {
        _Mesh = new Mesh(); //_Mesh 중 하나를 새롭게 생성
        GetComponent<MeshFilter>().mesh = _Mesh;

        _Vertices = new Vector3[width * height]; //_Vertices에 점을 만들고 점을 찍음, []은 배열
        _UV = new Vector2[width * height]; //Texture를 넣을려면 UV 좌표를 설정해줘야하고 UV의 길이는 Vertices와 동일
        _Triangles = new int[6 * ((width - 1) * (height - 1))]; //[]안에 계산된 값은 _Triangles 변수의 개수
         //triangle로 삼각형을 그릴 수 있게 해줌
         //삼각형을 여러개 이어 나간다면 한개의 mesh가 탄생

        int triangleIndex = 0; //triangleIndex를 0으로 선언
        for (int y = 0; y < height; y++) //변수 y가 0부터 height보다 작을 때까지 1씩 증가하며 반복
        {
            for (int x = 0; x < width; x++) //변수 x가 0부터 width보다 작을 때까지 1씩 증가하며 반복
            {
                int index = (y * width) + x; //변수 index는 (y * width) + x

                _Vertices[index] = new Vector3(x, -y, 0); 
                _UV[index] = new Vector2(((float)x / (float)width), ((float)y / (float)height));

                // Skip the last row/col
                if (x != (width - 1) && y != (height - 1)) //AND 연산자, x는 width - 1이 아니고 y는 height - 1이 아니면
                {
                    int topLeft = index; 
                    int topRight = topLeft + 1; //index + 1
                    int bottomLeft = topLeft + width; //index + width + 1 
                    int bottomRight = bottomLeft + 1; //index + width + 2

                    _Triangles[triangleIndex++] = topLeft; //_Triangles의 첫번째 배열과 index가 같다..?
                    _Triangles[triangleIndex++] = topRight;
                    _Triangles[triangleIndex++] = bottomLeft;
                    _Triangles[triangleIndex++] = bottomLeft;
                    _Triangles[triangleIndex++] = topRight;
                    _Triangles[triangleIndex++] = bottomRight;
                }
            }
        }

        _Mesh.vertices = _Vertices;
        _Mesh.uv = _UV;
        _Mesh.triangles = _Triangles;
        _Mesh.RecalculateNormals();
    }
    
    void OnGUI() //Update() 함수 뿐만 아니라 포함하는 스크립트가 활성화 될 때마다 호출
    {
        GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height)); //GUI가 위치하게 될 기준 영역을 지정
        GUI.TextField(new Rect(Screen.width - 250 , 10, 250, 20), "DepthMode: " + ViewMode.ToString());
        GUI.EndGroup();
    }

    void Update()
    {
        if (_Sensor == null)
        {
            return;
        }
        
        if (Input.GetButtonDown("Fire1")) //버튼을 누를때 한번 True가 발생
        {
            if(ViewMode == DepthViewMode.MultiSourceReader) //Ctrl 누를때마다 ViewMode 바뀜
            {
                ViewMode = DepthViewMode.SeparateSourceReaders;
            }
            else
            {
                ViewMode = DepthViewMode.MultiSourceReader;
            }
        }
        
        float yVal = Input.GetAxis("Horizontal");
        float xVal = -Input.GetAxis("Vertical");

        transform.Rotate(
            (xVal * Time.deltaTime * _Speed), 
            (yVal * Time.deltaTime * _Speed), 
            0, 
            Space.Self); //스피드
            
        if (ViewMode == DepthViewMode.SeparateSourceReaders) //separate이면, color랑 depth 반환
        {
            if (ColorSourceManager == null)
            {
                return;
            }
            
            _ColorManager = ColorSourceManager.GetComponent<ColorSourceManager>();
            if (_ColorManager == null)
            {
                return;
            }
            
            if (DepthSourceManager == null)
            {
                return;
            }
            
            _DepthManager = DepthSourceManager.GetComponent<DepthSourceManager>();
            if (_DepthManager == null)
            {
                return;
            }
            
            gameObject.GetComponent<Renderer>().material.mainTexture = _ColorManager.GetColorTexture();
            RefreshData(_DepthManager.GetData(),
                _ColorManager.ColorWidth,
                _ColorManager.ColorHeight);
        }
        else //multisourceview이면 multi 반환
        {
            if (MultiSourceManager == null)
            {
                return;
            }
            
            _MultiManager = MultiSourceManager.GetComponent<MultiSourceManager>();
            if (_MultiManager == null)
            {
                return;
            }
            
            gameObject.GetComponent<Renderer>().material.mainTexture = _MultiManager.GetColorTexture();
            
            RefreshData(_MultiManager.GetDepthData(),
                        _MultiManager.ColorWidth,
                        _MultiManager.ColorHeight);
        }
    }
    
    private void RefreshData(ushort[] depthData, int colorWidth, int colorHeight) //깊이, 넓이, 높이
    {
        var frameDesc = _Sensor.DepthFrameSource.FrameDescription;
        
        ColorSpacePoint[] colorSpace = new ColorSpacePoint[depthData.Length];
        _Mapper.MapDepthFrameToColorSpace(depthData, colorSpace);
        
        for (int y = 0; y < frameDesc.Height; y += _DownsampleSize) 
        {
            for (int x = 0; x < frameDesc.Width; x += _DownsampleSize)
            {
                int indexX = x / _DownsampleSize;
                int indexY = y / _DownsampleSize;
                int smallIndex = (indexY * (frameDesc.Width / _DownsampleSize)) + indexX;
                
                double avg = GetAvg(depthData, x, y, frameDesc.Width, frameDesc.Height);
                
                avg = avg * _DepthScale;
                
                _Vertices[smallIndex].z = (float)avg;
                
                // Update UV mapping with CDRP
                var colorSpacePoint = colorSpace[(y * frameDesc.Width) + x];
                _UV[smallIndex] = new Vector2(colorSpacePoint.X / colorWidth, colorSpacePoint.Y / colorHeight);
            }
        }
        
        _Mesh.vertices = _Vertices;
        _Mesh.uv = _UV;
        _Mesh.triangles = _Triangles;
        _Mesh.RecalculateNormals();
    }
    
    private double GetAvg(ushort[] depthData, int x, int y, int width, int height)
    {
        double sum = 0.0;
        
        for (int y1 = y; y1 < y + 4; y1++)
        {
            for (int x1 = x; x1 < x + 4; x1++)
            {
                int fullIndex = (y1 * width) + x1;
                
                if (depthData[fullIndex] == 0)
                    sum += 4500;
                else
                    sum += depthData[fullIndex];
                
            }
        }

        return sum / 16;
    }

    void OnApplicationQuit()
    {
        if (_Mapper != null)
        {
            _Mapper = null;
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
