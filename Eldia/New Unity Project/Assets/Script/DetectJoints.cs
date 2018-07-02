using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using UnityEngine.UI;
using System;

public class DetectJoints : MonoBehaviour {
	public GameObject BodySrcManager;
	public JointType TrackedJoint;
	private BodySourceManager bodyManager;
	private Body[] bodies;
    private int count;
	public UnityEngine.AudioSource source, source1;
	public UnityEngine.AudioClip clip;

	int _xFinal ;
	int _yFinal ;
	int _xFinal2 ;
	int _yFinal2 ;
    float profLaser1, profLaser1end, profLaser2, profLaser2end, profLaser3, profLaser3end;
    float laser1x, laser1y;
    float minX, minY, maxX, maxY;
    
    double rootY1;
    double rootX1;

    private int ccont = 0;

    double rootY2;
    double rootX2;

    LineRenderer line, laser1, laser2, laser3;
    Vector3 start, startLaser1, startLaser2,startLaser3;
    Vector3 end, endLaser1, endLaser2, endLaser3;
    float avg1Z, avg2Z, avg3Z, avgZ, avgY, avgX;
    BoxCollider col;
    Vector2 startLaser1v2, endLaser1v2, startLaser2v2, endLaser2v2, startLaser3v2, endLaser3v2;
    bool isCollide;
    Material sword;
    Material sword1;
	KinectSensor _sensor;
	MultiSourceFrameReader _reader;

	public float multiplier = 1.0f;
    ushort prof1;
    ushort prof2;
    float factor = 0.01f;//velocidad lento//0.05f;//

    int infraredIndexAnt1, infraredIndexAnt2;

    void configureLine(LineRenderer line) {
        line.positionCount = 2;
        line.startWidth = 0.3f;
        line.endWidth = 0.3f;
    }

    void setPosLineRenderer(LineRenderer line, Vector3 start, Vector3 end)
    {
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }
	// Use this for initialization
	void Start () {
        
        count = 0;
		_sensor = KinectSensor.GetDefault();
		
        if (_sensor != null)
		{
			_sensor.Open();

			_reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
			_reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
		}
		if(BodySrcManager == null)
		{
			Debug.Log("Assign Game Object with Body Source Manager");
		}
		else
		{
			bodyManager = BodySrcManager.GetComponent<BodySourceManager>();
		}
		line = gameObject.AddComponent<LineRenderer>();
		source = gameObject.GetComponent<UnityEngine.AudioSource> ();
		source1 = gameObject.AddComponent<UnityEngine.AudioSource> ();

        laser1 = new GameObject("Laser1").AddComponent<LineRenderer>();
        laser2 = new GameObject("Laser2").AddComponent<LineRenderer>();
        laser3 = new GameObject("Laser3").AddComponent<LineRenderer>();
        start = new Vector3 (0.0f, 0.0f);
        end = new Vector3(0.5f, 0.5f);
        
        profLaser1 = 0.9f;
        profLaser1end = profLaser1+0.2f;
        profLaser2 = 0.9f;
        profLaser2end = 0.7f;
        profLaser3 = 0.9f;
        profLaser3end = 0.7f;

        laser1x = 0.1f * multiplier;
        laser1y = 0.4f * multiplier;
        startLaser1 = new Vector3(laser1x, laser1y, profLaser1 * multiplier);
        endLaser1 = new Vector3(laser1x, laser1y, profLaser1end * multiplier);

        startLaser2 = new Vector3(0.2f * multiplier, 0.2f * multiplier, profLaser2 * multiplier);
        endLaser2 = new Vector3(0.21f * multiplier, 0.21f * multiplier, profLaser2end * multiplier);
        Debug.Log(string.Format("Laser2startX: {0},startY: {1}", 0.2f * multiplier, 0.2f * multiplier));
        Debug.Log(string.Format("endX: {0},endY: {1}", 0.21f * multiplier, 0.2f * multiplier));

        startLaser3 = new Vector3(0.2f * multiplier, 0.4f * multiplier, profLaser3 * multiplier);
        endLaser3 = new Vector3(0.21f * multiplier, -0.3f * multiplier, profLaser3end * multiplier);
        //Debug.Log(string.Format("start3X: {0},start3Y: {1}", 0.2f * multiplier, 0.4f * multiplier));
        //Debug.Log(string.Format("end3X: {0},end3Y: {1}", 0.21f * multiplier, -0.3f * multiplier));
        configureLine(line);
        configureLine(laser1);
        configureLine(laser2);
        configureLine(laser3);

        setPosLineRenderer(line,start,end);
        setPosLineRenderer(laser1, startLaser1, endLaser1);
        setPosLineRenderer(laser2, startLaser2, endLaser2);
        setPosLineRenderer(laser3, startLaser3, endLaser3);
        
        sword = Resources.Load ("Materials/LaserMat", typeof(Material)) as Material;
		clip = Resources.Load ("Materials/fx4", typeof(AudioClip)) as AudioClip;
		source.clip = clip;
		source.Play ();

		//sword1 = Resources.Load("Materials/laser", typeof(Image)) as Texture2D;


        if(sword!=null) line.material = sword;
        sword1 = Resources.Load("Materials/LaserMat1", typeof(Material)) as Material;
        if (sword1 != null) laser1.material = sword1;
        if (sword1 != null) laser2.material = sword1;
		if (sword1 != null) laser3.material = sword1;
		//if (sword1 != null) line.material.mainTexture = sword1;
	}

	void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
	{
		
        
        ccont++;
        //if (ccont % 2 == 0)
        //{
            var reference = e.FrameReference.AcquireFrame();
            // Infrared
            using (var frame = reference.InfraredFrameReference.AcquireFrame())
            {

                if (frame != null)
                {
                    //do something
                    int width = frame.FrameDescription.Width;
                    int height = frame.FrameDescription.Height;

                    ushort[] infraredData = new ushort[width * height];
                    frame.CopyFrameDataToArray(infraredData);

                    int maxIr = 0;
                    //int Ir = 0;
                    int _x = 0;
                    int _y = 0;
                    double dist = 0;
                    double maxdist = 0;
                    int _xant = -1;
                    int _yant = -1;

                    int cont = 0;

                    //if (infraredData[infraredIndexAnt1] >= 65000 && infraredData[infraredIndexAnt2] >= 65000)
                    //{
                    //    ccont++;
                    ///NADA
                    /*if(infraredData[infraredIndexAnt2] >= 65000)
                        {
                            ///nada
                        }
                    else
                    {
                        //todavia nada
                    }*/

                    //}else{
                    //ccont = 0;
                    /*_xFinal = 0;
				    _yFinal = 0;
				    _xFinal2 = 0;
				    _yFinal2 = 0;*/
                    for (int infraredIndex = 0; infraredIndex < infraredData.Length; ++infraredIndex)
                    {
                        ushort ir = infraredData[infraredIndex];

                        if (ir >= 65000)
                        {
                            cont++;
                            _y = infraredIndex / width;
                            _x = infraredIndex - (_y * width);
                            infraredIndexAnt1 = infraredIndex;
                            if (_x != 509)
                            {
                                _xFinal = _x;
                            }
                            if (_y != 360)
                            {
                                _yFinal = _y;
                            }

                            if (_xant != -1 && _yant != -1)
                            {
                                dist = Math.Sqrt(Math.Pow((double)_xant - _xFinal, 2.0) + Math.Pow((double)_yant - _yFinal, 2.0));
                                if (dist > maxdist)
                                {
                                    maxdist = dist;
                                }
                                if (dist > 30.0f)
                                {
                                    _xFinal2 = _xant;
                                    _yFinal2 = _yant;
                                    infraredIndexAnt2 = infraredIndex;
                                }
                            }

                            //Ir = ir;
                            _xant = _xFinal;
                            _yant = _yFinal;

                        }

                        if (ir > maxIr)
                        {
                            maxIr = ir;
                        }


                    }
                }
                //}
            }
            // Open depth frame
            using (var frame = reference.DepthFrameReference.AcquireFrame())
            {

                if (frame != null)
                {

                    rootX1 = _xFinal2 - ((_xFinal2 - _xFinal) / 4);//lambda * v1 + _xFinal; //
                    rootY1 = _yFinal2 - ((_yFinal2 - _yFinal) / 4);//lambda * v2 + _yFinal; //

                    rootX2 = ((_xFinal2 - _xFinal) / 4) + _xFinal;//lambda * v1 + _xFinal; //
                    rootY2 = ((_yFinal2 - _yFinal) / 4) + _yFinal;//lambda * v2 + _yFinal; //

                    int width = frame.FrameDescription.Width;
                    int height = frame.FrameDescription.Height;
                    ushort[] depthData = new ushort[width * height];
                    frame.CopyFrameDataToArray(depthData);

                    prof1 = depthData[width * (ushort)rootY1 + (ushort)rootX1];
                    prof2 = depthData[width * (ushort)rootY2 + (ushort)rootX2];
                }
            }
        }
	//}

    Vector2 GetIntersectionPointCoordinates(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2)
    {
        float tmp = (B2.x - B1.x) * (A2.y - A1.y)  - (B2.y - B1.y) * (A2.x - A1.x);

        if (tmp == 0)
        {
            // No solution!
            isCollide = false;
          //  Debug.Log(string.Format("found: {0}", isCollide));
            return Vector2.zero;
        }

        float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;

        isCollide = true;

        //Debug.Log(string.Format("found: {0}", isCollide));

        return new Vector2(
            B1.x + (B2.x - B1.x) * mu,
            B1.y + (B2.y - B1.y) * mu
        );

    }

	// Update is called once per frame
	void Update () {

		clip = Resources.Load ("Materials/lasrhit2", typeof(AudioClip)) as AudioClip;
		source1.clip = clip;
		float _X = ((float)_xFinal/256.0f)-1.0f;
		float _Y = ((float)_yFinal/-424.0f)+0.55f;
		float _X2 = ((float)_xFinal2/256.0f)-1.0f;
		float _Y2 = ((float)_yFinal2/-424.0f)+0.55f;

        float _Z = (prof2 / 1500.0f) - 1.0f;
        float _Z2 = (prof1 / 1500.0f) - 1.0f;

        //float _rootX1 = ((float)rootX1 / 256.0f) - 1.0f;
        //float _rootY1 = ((float)rootY1 / -424.0f) + 0.55f;

         float _rootX2 = ((float)rootX2 / 256.0f) - 1.0f;
         float _rootY2 = ((float)rootY2 / -424.0f) + 0.55f;        

        //gameObject.transform.position = new Vector3 ((float)_xFinal * multiplier, (float)_yFinal * multiplier);
		//gameObject.transform.position = new Vector3 (_X * multiplier, _Y * multiplier);
        ///***Debug.Log(string.Format("_x{0},_y{1},_xX{2},_yY{3},rootX{4},rootY{5}", _X, _Y, _X2, _Y2, _rootX1, _rootY1));
        //start = new Vector3 (_X * multiplier, _Y * multiplier);
        //end = new Vector3(_X2 * multiplier, _Y2 * multiplier);
         if (prof1 == 0 || prof2 == 0)
         {
             start = new Vector3(_X * multiplier, _Y * multiplier, 0.0f * multiplier);
            end = new Vector3(_X2 * multiplier, _Y2 * multiplier,0.0f*multiplier);
            
        }else{

            start = new Vector3(_X * multiplier, _Y * multiplier, _Z * multiplier);
            end = new Vector3(_X2 * multiplier, _Y2 * multiplier, _Z2 * multiplier);
            //Debug.Log(string.Format("SwordstartX: {0},SwordStartY: {1}", start.x, start.y));
           // Debug.Log(string.Format("SwordendX: {0},SwordendY: {1}", end.x, end.y));
        

        }
        
        //gameObject.transform.position = new Vector3(_rootX2*multiplier, _rootY2*multiplier, _Z2 * multiplier);

        setPosLineRenderer(line, start, end);

        if (profLaser1 < -0.4) {
            //laser1.material = null;
            laser1.material = sword1;
            
            //profLaser1 = UnityEngine.Random.Range(0.5f, 1.0f);
            profLaser1 = 0.9f;
            profLaser1end = profLaser1 + 0.2f;
            laser1x = UnityEngine.Random.Range(-0.5f,0.5f)* multiplier;
            laser1y = UnityEngine.Random.Range(-0.1f, 0.3f) * multiplier;
        }
        else
        {
            profLaser1 = profLaser1 - factor;
            profLaser1end = profLaser1end - factor;          
            startLaser1.Set(laser1x,laser1y, profLaser1 * multiplier);
            endLaser1.Set(laser1x, laser1y, profLaser1end * multiplier);        
        }

        if (profLaser2 < -0.6)
        {
            profLaser2 = 0.9f;
            profLaser2end = profLaser2 + 0.2f;
            //laser2.material = null;
            laser2.material = sword1;
        }
        else {
            profLaser2 = profLaser2 - factor;
            profLaser2end = profLaser2end - factor;
			startLaser2.Set(0.25f * multiplier, 0.25f * multiplier, profLaser2 * multiplier);
			endLaser2.Set(0.26f * multiplier, 0.26f * multiplier, profLaser2end * multiplier);
        }
        
		if (profLaser3 < -0.6)
		{
			profLaser3 = 0.7f;
			profLaser3end = profLaser3 + 0.2f;
			//laser3.material = null;
			laser3.material = sword1;
		}
		else {
			profLaser3 = profLaser3 - (factor/2);//(factor+ UnityEngine.Random.Range(-0.1f,0.1f)) ;
			profLaser3end = profLaser3end - (factor/2);//(factor+ UnityEngine.Random.Range(-0.1f,0.1f));
			startLaser3.Set(-0.10f * multiplier, 0.10f * multiplier, profLaser3 * multiplier);
			endLaser3.Set(-0.11f * multiplier, 0.11f * multiplier, profLaser3end * multiplier);
		}
        setPosLineRenderer(laser1, startLaser1, endLaser1);
        setPosLineRenderer(laser2, startLaser2, endLaser2);
        setPosLineRenderer(laser3, startLaser3, endLaser3);
        
		isCollide =false;
        //Debug.Log(string.Format("prof1:{0},prof2:{1}", profLaser1, profLaser2));
        
		startLaser3v2 = new Vector2(startLaser3.x, startLaser3.y);
        endLaser3v2 = new Vector2(endLaser3.x, endLaser3.y);


		avg3Z = profLaser3end * multiplier;
        //avg3Z = ((profLaser3 * multiplier) + (profLaser3end * multiplier)) / 2.0f;
        avgZ = ((_Z * multiplier) + (_Z2 * multiplier)) / 2.0f;
        
        //Debug.Log(string.Format("z2:{0},z3:{1}",  avg2Z, avg3Z));
        //if (Math.Abs(avg2Z - avgZ) < 0.01f)
		if (Math.Abs (avg3Z - avgZ) < 0.3f) {
		
			Vector2 result = GetIntersectionPointCoordinates (startLaser3v2, endLaser3v2, start, end);
			avgX = (endLaser3v2.x + startLaser3v2.x) / 2;
			avgY = (endLaser3v2.y + startLaser3v2.y) / 2;
			if (Math.Abs(result.x - avgX) < 0.5f && Math.Abs(result.y - avgY) < 0.5f)
			{
				Debug.Log(string.Format("col"));
				source1.Play ();
				laser3.material = sword;
				count = count + 1;
			}
		}
		isCollide =false;
		startLaser2v2 = new Vector2(startLaser2.x, startLaser2.y);
		endLaser2v2 = new Vector2(endLaser2.x, endLaser2.y);
		avg2Z = profLaser2end * multiplier;
		if (Math.Abs(avg2Z - avgZ) < 0.3f)
        {
            Vector2 result = GetIntersectionPointCoordinates(startLaser2v2, endLaser2v2, start, end);
            ///Vector2 result = GetIntersectionPointCoordinates(startLaser2v2, endLaser2v2, startLaser3v2, endLaser3v2);
            //Debug.Log(string.Format("z2:{0},z:{1},puntox:{2}, puntoy:{3} ", avg2Z, avgZ,result.x,result.y));
           //1 Debug.Log(string.Format("SwordstartX: {0},SwordStartY: {1}", start.x, start.y));
           // Debug.Log(string.Format("SwordendX: {0},SwordendY: {1}", end.x, end.y));
            /*Debug.Log(result.x <= endLaser2v2.x && result.x >= startLaser2v2.x);//Para saber si pertenece a los 2 segmentos
            Debug.Log(result.x <= endLaser3v2.x && result.x >= startLaser2v2.x );
              Debug.Log( result.y <= endLaser2v2.y);
              Debug.Log(result.y >= startLaser2v2.y);
              Debug.Log(result.y);
              Debug.Log(startLaser2v2.y);
            Debug.Log(result.y <= startLaser3v2.y && result.y >= endLaser3v2.y);*/
            
            //if (result.x <=endLaser2v2.x && result.x >= startLaser2v2.x && result.x <= endLaser3v2.x && result.x >= startLaser3v2.x && result.y <= endLaser2v2.y && result.y >= startLaser2v2.y && result.y <= startLaser3v2.y && result.y >= endLaser3v2.y)
            //{
            /*if(isCollide){
                Debug.Log(string.Format("col"));
                laser2.material = sword;
            }*/
            //}
            
            //if (result.x <= endLaser2v2.x && result.x >= startLaser2v2.x && result.x <= end.x && result.x >= start.x && result.y <= endLaser2v2.y && result.y >= startLaser2v2.y && result.y <= start.y && result.y >= end.y)
            avgX = (endLaser2v2.x + startLaser2v2.x) / 2;
            avgY = (endLaser2v2.y + startLaser2v2.y) / 2;
            
			//if (result.x <= endLaser2v2.x && result.x >= startLaser2v2.x && result.y <= endLaser2v2.y && result.y >= startLaser2v2.y) {
			/*if (result.x >= endLaser2v2.x && result.x <= startLaser2v2.x && result.y >= endLaser2v2.y && result.y <= startLaser2v2.y) {
				laser2.material = sword;
				count = count + 1;
			}*/
                //if(isCollide)
            if (Math.Abs(result.x - avgX) < 0.5f && Math.Abs(result.y - avgY) < 0.5f)
            {
                Debug.Log(string.Format("col"));
				source1.Play ();
                laser2.material = sword;
                count = count + 1;
            }
        }


        isCollide = false;
        avg1Z = profLaser1end * multiplier;
        startLaser1v2 = new Vector2(startLaser1.x, startLaser1.y);
        endLaser1v2 = new Vector2(endLaser1.x, endLaser1.y);
        avgZ = ((_Z * multiplier) + (_Z2 * multiplier)) / 2.0f;

        //Debug.Log(string.Format("z2:{0},z3:{1}",  avg2Z, avg3Z));
        //if (Math.Abs(avg2Z - avgZ) < 0.01f)
        if (Math.Abs(avg1Z - avgZ) < 0.1f)
        {
            Vector2 result = GetIntersectionPointCoordinates(startLaser1v2, endLaser1v2, start, end);
            ///Vector2 result = GetIntersectionPointCoordinates(startLaser2v2, endLaser2v2, startLaser3v2, endLaser3v2);
            //Debug.Log(string.Format("z2:{0},z:{1},puntox:{2}, puntoy:{3} ", avg1Z, avgZ, result.x, result.y));
           //1 Debug.Log(string.Format("SwordstartX: {0},SwordStartY: {1}", start.x, start.y));
            Debug.Log(string.Format("SwordendX: {0},SwordendY: {1}", end.x, end.y));
            avgX = (endLaser1v2.x + startLaser1v2.x) / 2;
            avgY = (endLaser1v2.y + startLaser1v2.y) / 2;
            
            if (Math.Abs(result.x - avgX) < 0.3f && Math.Abs(result.y - avgY) < 0.3f)
            {
                Debug.Log(string.Format("colision laser 1"));
				source1.Play ();
                laser1.material = sword;
                count = count + 1;
            
			}
        }
        if (bodyManager == null) {
			return;
		}
		bodies = bodyManager.GetData ();
		if (bodies == null) {
			return;
		}

		foreach (var body in bodies) {
			if (body == null) {
				
				continue;
			}

			if (body.IsTracked) {
				var pos = body.Joints [TrackedJoint].Position;
				//gameObject.transform.position = new Vector3 (pos.X * multiplier, pos.Y * multiplier,pos.Z*multiplier-1.0f) ;
                //gameObject.transform.position = new Vector3(pos.X * multiplier, pos.Y * multiplier, 0.0f * multiplier);
                //Debug.Log(string.Format("x: {0}, y:{1},z: {2}", pos.X, pos.Y, pos.Z));

			}
		}

	}

    public GUIStyle guiStyle = new GUIStyle();

    void OnGUI()
    {
        guiStyle.fontSize = 20;
        guiStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(10, 10, 500, 100), "Score: " + count.ToString(), guiStyle);
        GUI.Label(new Rect(10, 30, 500, 100), "Cont: " + ccont.ToString(), guiStyle);
    }
 }
